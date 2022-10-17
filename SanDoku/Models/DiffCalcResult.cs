namespace SanDoku.Models;

public record DiffCalcResult
{
    #region Attributes

    /// <summary>
    /// Used by all
    /// </summary>
    public int MaxCombo { get; set; }

    /// <summary>
    /// Used by all
    /// </summary>
    public double StarRating { get; set; }

    /// <summary>
    /// Used by osu
    /// </summary>
    public double Aim { get; set; }

    /// <summary>
    /// Used by osu
    /// </summary>
    public double Speed { get; set; }

    /// <summary>
    /// Used by osu
    /// </summary>
    public double OverallDifficulty { get; set; }

    /// <summary>
    /// Used by osu, catch
    /// </summary>
    public double ApproachRate { get; set; }

    /// <summary>
    /// Used by taiko, mania
    /// </summary>
    public double GreatHitWindow { get; set; }

    /// <summary>
    /// Used by osu
    /// </summary>
    public double Flashlight { get; set; }

    /// <summary>
    /// Used by osu
    /// </summary>
    public double SliderFactor { get; set; }

    /// <summary>
    /// Used by osu
    /// </summary>
    public double SpeedNoteCount { get; set; }

    #endregion

    #region Other Data?

    // not in the database attributes list in lazer source code

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
    public double Stamina { get; set; }

    /// <summary>
    /// Used by taiko
    /// </summary>
    public double Rhythm { get; set; }

    /// <summary>
    /// Used by taiko
    /// </summary>
    public double Colour { get; set; }

    /// <summary>
    /// Used by taiko
    /// </summary>
    public double Peak { get; set; }

    #endregion
}