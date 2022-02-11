using System;
using osu.Game.Beatmaps;
using osu.Game.IO;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace SanDoku.Models
{
    public class BeatmapInput
    {
        private readonly MemoryStream _memoryStream;

        public string Md5Checksum { get; }
        public long ContentLength => _memoryStream.Length;

        private BeatmapInput(MemoryStream memoryStream, string md5Checksum)
        {
            _memoryStream = memoryStream;
            Md5Checksum = md5Checksum;
        }

        public IBeatmap DecodeBeatmap()
        {
            _memoryStream.Position = 0;
            using var reader = new LineBufferedReader(_memoryStream);
            var decoder = osu.Game.Beatmaps.Formats.Decoder.GetDecoder<Beatmap>(reader);
            var beatmap = decoder.Decode(reader);
            return beatmap;
        }

        public static async Task<BeatmapInput> BuildFromStream(Stream stream, CancellationToken ct = default)
        {
            var memStream = new MemoryStream();
            await stream.CopyToAsync(memStream, ct);
            memStream.Position = 0;
            using var md5 = MD5.Create();
            var md5HashBytes = await md5.ComputeHashAsync(memStream, ct);
            var md5String = Convert.ToHexString(md5HashBytes).ToLowerInvariant();
            return new BeatmapInput(memStream, md5String);
        }
    }
}