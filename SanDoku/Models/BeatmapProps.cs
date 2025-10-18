namespace SanDoku.Models;

public class BeatmapProps
{
    public float DrainRate { get; set; }
    public float CircleSize { get; set; }
    public float OverallDifficulty { get; set; }
    public float ApproachRate { get; set; }

    public double SliderMultiplier { get; set; }
    public double SliderTickRate { get; set; }
}