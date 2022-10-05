namespace SanDoku.Models;

public record DiffCalcResult
{
    /// <summary>
    /// Used by all
    /// </summary>
    public double StarRating { get; set; }

    /// <summary>
    /// Used by all
    /// </summary>
    public int MaxCombo { get; set; }

    /// <summary>
    /// Used by osu
    /// </summary>
    public double AimStrain { get; set; }

    /// <summary>
    /// Used by osu
    /// </summary>
    public double SpeedStrain { get; set; }

    /// <summary>
    /// Used by osu
    /// </summary>
    public double FlashlightRating { get; set; }

    /// <summary>
    /// Used by osu
    /// </summary>
    public double SliderFactor { get; set; }

    /// <summary>
    /// Used by osu, taiko, catch
    /// </summary>
    public double ApproachRate { get; set; }

    /// <summary>
    /// Used by osu
    /// </summary>
    public double OverallDifficulty { get; set; }

    /// <summary>
    /// Used by osu
    /// </summary>
    public double DrainRate { get; set; }

    /// <summary>
    /// Used by osu
    /// </summary>
    public int HitCircleCount { get; set; }

    /// <summary>
    /// Used by osu
    /// </summary>
    public int SliderCount { get; set; }

    /// <summary>
    /// Used by osu
    /// </summary>
    public int SpinnerCount { get; set; }

    /// <summary>
    /// Used by taiko
    /// </summary>
    public double StaminaStrain { get; set; }

    /// <summary>
    /// Used by taiko
    /// </summary>
    public double RhythmStrain { get; set; }

    /// <summary>
    /// Used by taiko
    /// </summary>
    public double ColourStrain { get; set; }

    /// <summary>
    /// Used by taiko, mania
    /// </summary>
    public double GreatHitWindow { get; set; }

    /// <summary>
    /// Used by mania
    /// </summary>
    public double ScoreMultiplier { get; set; }
}