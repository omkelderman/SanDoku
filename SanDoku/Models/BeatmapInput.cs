using osu.Game.Beatmaps;
using osu.Game.IO;
using System.Security.Cryptography;

namespace SanDoku.Models;

public class BeatmapInput
{
    public static readonly BeatmapInput Empty = new(new MemoryStream(Array.Empty<byte>()), "93b885adfe0da089cdf634904fd59f71");

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
        var md5String = await ComputeMd5String(memStream, ct);
        return new BeatmapInput(memStream, md5String);
    }

    private static async Task<string> ComputeMd5String(Stream stream, CancellationToken ct)
    {
        using var md5 = MD5.Create();
        var md5HashBytes = await md5.ComputeHashAsync(stream, ct);
        return Convert.ToHexString(md5HashBytes).ToLowerInvariant();
    }
}