using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using SanDoku.Models;
using System.Text;

namespace SanDoku.Util;

public class OsuInputFormatter : TextInputFormatter
{
    public const string ContentType = "text/osu";

    public OsuInputFormatter()
    {
        SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ContentType));
        SupportedEncodings.Add(Encoding.UTF8);
    }

    protected override bool CanReadType(Type type)
    {
        return type == typeof(BeatmapInput);
    }

    // base type InputFormatter does a check on context.HttpContext.Request.ContentLength == 0
    // which results in a immediate error result
    // which we do not want, if there is no content, just return an empty BeatmapInput
    // results in a nicer error message to the end user
    public override Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
    {
        return base.ReadRequestBodyAsync(context);
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding effectiveEncoding)
    {
        var httpContext = context.HttpContext;
        var request = httpContext.Request;
        var serviceProvider = httpContext.RequestServices;
        var logger = serviceProvider.GetRequiredService<ILogger<OsuInputFormatter>>();

        try
        {
            var beatmapInput = request.ContentLength is null or 0 ? BeatmapInput.Empty : await BeatmapInput.BuildFromStream(request.Body);
            return await InputFormatterResult.SuccessAsync(beatmapInput);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while reading osu file content");
            return await InputFormatterResult.FailureAsync();
        }
    }
}