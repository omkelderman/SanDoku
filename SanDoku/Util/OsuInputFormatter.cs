using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using osu.Game.Beatmaps;
using osu.Game.IO;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SanDoku.Util
{
    public class OsuInputFormatter : TextInputFormatter
    {
        public const string ContentType = "plain/osu";

        public OsuInputFormatter()
        {
            SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ContentType));
            SupportedEncodings.Add(Encoding.UTF8);
        }

        protected override bool CanReadType(Type type)
        {
            return type == typeof(Beatmap);
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
                var memStream = new MemoryStream();
                await httpContext.Request.Body.CopyToAsync(memStream);
                // TODO possibly find a way to make this better
                var beatmap = await Task.Run(() => DecodeBeatmap(memStream));
                return await InputFormatterResult.SuccessAsync(beatmap);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while reading osu file content");
                return await InputFormatterResult.FailureAsync();
            }
        }

        private static Beatmap DecodeBeatmap(MemoryStream memStream)
        {
            memStream.Position = 0;
            using var reader = new LineBufferedReader(memStream);
            var decoder = osu.Game.Beatmaps.Formats.Decoder.GetDecoder<Beatmap>(reader);
            var beatmap = decoder.Decode(reader);
            return beatmap;
        }
    }
}