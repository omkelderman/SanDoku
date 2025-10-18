using osu.Game.Beatmaps;
using SanDoku.Models;

namespace SanDoku.Extensions;

public static class BeatmapPropsExtensions
{
    public static BeatmapProps Map(this BeatmapDifficulty x)
    {
        return new BeatmapProps
        {
            DrainRate = x.DrainRate,
            CircleSize = x.DrainRate,
            OverallDifficulty = x.OverallDifficulty,
            ApproachRate = x.ApproachRate,
            SliderMultiplier = x.SliderMultiplier,
            SliderTickRate = x.SliderTickRate,
        };
    }
}