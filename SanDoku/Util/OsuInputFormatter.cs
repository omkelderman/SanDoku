using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using SanDoku.Models;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SanDoku.Util
{
    public class OsuInputFormatter : TextInputFormatter
    {
        public const string ContentType = "text/osu";
        public const string WrongButLegacyContentType = "plain/osu";

        public OsuInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ContentType));
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(WrongButLegacyContentType));
            SupportedEncodings.Add(Encoding.UTF8);
        }

        protected override bool CanReadType(Type type)
        {
            return type == typeof(BeatmapInput);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding effectiveEncoding)
        {
            var httpContext = context.HttpContext;
            var serviceProvider = httpContext.RequestServices;
            var logger = serviceProvider.GetRequiredService<ILogger<OsuInputFormatter>>();

            if (!effectiveEncoding.Equals(Encoding.UTF8))
            {
                logger.LogError($"Error while parsing osu beatmap: wrong encoding: {effectiveEncoding}");
                return await InputFormatterResult.FailureAsync();
            }

            try
            {
                var beatmapInput = await BeatmapInput.BuildFromStream(httpContext.Request.Body);
                return await InputFormatterResult.SuccessAsync(beatmapInput);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while reading osu file content");
                return await InputFormatterResult.FailureAsync();
            }
        }
    }
}