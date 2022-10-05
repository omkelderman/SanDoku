using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Skinning;

namespace SanDoku.Util;

public class ProcessorWorkingBeatmap : WorkingBeatmap
{
    private readonly IBeatmap _beatmap;

    public ProcessorWorkingBeatmap(IBeatmap beatmap) : base(beatmap.BeatmapInfo, null)
    {
        _beatmap = beatmap;
    }

    protected override IBeatmap GetBeatmap() => _beatmap;
    protected override Texture GetBackground() => throw new InvalidOperationException();
    protected override Track GetBeatmapTrack() => throw new InvalidOperationException();
    protected override ISkin GetSkin() => throw new InvalidOperationException();
    public override Stream GetStream(string storagePath) => throw new InvalidOperationException();
}