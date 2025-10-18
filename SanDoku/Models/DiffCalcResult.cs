namespace SanDoku.Models;

public record DiffCalcResult
{
    #region All

    /// <summary>
    /// The combined star rating of all skills.
    /// Used by all.
    /// <c>ATTRIB_ID_DIFFICULTY = 11</c>
    /// </summary>
    public double StarRating { get; set; }

    /// <summary>
    /// The maximum achievable combo.
    /// Used by all.
    /// <c>ATTRIB_ID_MAX_COMBO = 9</c>
    /// </summary>
    public int MaxCombo { get; set; }

    #endregion

    #region osu!

    /// <summary>
    /// The difficulty corresponding to the aim skill.
    /// Used by osu.
    /// <c>ATTRIB_ID_AIM = 1</c>
    /// </summary>
    public double AimDifficulty { get; set; }

    /// <summary>
    /// The number of Sliders weighted by difficulty.
    /// Used by osu.
    /// <c>ATTRIB_ID_AIM_DIFFICULT_SLIDER_COUNT = 31.</c>
    /// </summary>
    public double AimDifficultSliderCount { get; set; }

    /// <summary>
    /// The difficulty corresponding to the speed skill.
    /// Used by osu.
    /// <c>ATTRIB_ID_SPEED = 3.</c>
    /// </summary>
    public double SpeedDifficulty { get; set; }

    /// <summary>
    /// The number of clickable objects weighted by difficulty.
    /// Related to <see cref="SpeedDifficulty" />.
    /// Used by osu.
    /// <c>ATTRIB_ID_SPEED_NOTE_COUNT = 21.</c>
    /// </summary>
    public double SpeedNoteCount { get; set; }

    /// <summary>
    /// The difficulty corresponding to the flashlight skill.
    /// Used by osu.
    /// <c>ATTRIB_ID_FLASHLIGHT = 17</c>
    /// </summary>
    public double FlashlightDifficulty { get; set; }

    /// <summary>
    /// Describes how much of <see cref="AimDifficulty" /> is contributed to by hitcircles or sliders.
    /// A value closer to 1.0 indicates most of <see cref="AimDifficulty" /> is contributed by hitcircles.
    /// A value closer to 0.0 indicates most of <see cref="AimDifficulty" /> is contributed by sliders.
    /// Used by osu.
    /// <c>ATTRIB_ID_SLIDER_FACTOR = 19</c>
    /// </summary>
    public double SliderFactor { get; set; }

    /// <summary>
    /// Used by osu.
    /// <c>ATTRIB_ID_AIM_DIFFICULT_STRAIN_COUNT = 25</c>
    /// </summary>
    public double AimDifficultStrainCount { get; set; }

    /// <summary>
    /// Used by osu
    /// <c>ATTRIB_ID_SPEED_DIFFICULT_STRAIN_COUNT = 23</c>
    /// </summary>
    public double SpeedDifficultStrainCount { get; set; }

    /// <summary>Used by osu</summary>
    public double DrainRate { get; set; }

    /// <summary>Used by osu</summary>
    public int HitCircleCount { get; set; }

    /// <summary>Used by osu</summary>
    public int SliderCount { get; set; }

    /// <summary>Used by osu</summary>
    public int SpinnerCount { get; set; }

    #endregion

    #region taiko

    /// <summary>
    /// The difficulty corresponding to the rhythm skill.
    /// Used by taiko.
    /// <c>ATTRIB_ID_RHYTHM_DIFFICULTY = 43</c>
    /// </summary>
    public double RhythmDifficulty { get; set; }

    /// <summary>
    /// The difficulty corresponding to the reading skill.
    /// Used by taiko.
    /// </summary>
    public double ReadingDifficulty { get; set; }

    /// <summary>
    /// The difficulty corresponding to the colour skill.
    /// Used by taiko.
    /// </summary>
    public double ColourDifficulty { get; set; }

    /// <summary>
    /// The difficulty corresponding to the stamina skill.
    /// Used by taiko.
    /// </summary>
    public double StaminaDifficulty { get; set; }

    /// <summary>
    /// The ratio of stamina difficulty from mono-color (single colour) streams to total stamina difficulty.
    /// Used by taiko.
    /// <c>ATTRIB_ID_MONO_STAMINA_FACTOR = 29</c>
    /// </summary>
    public double MonoStaminaFactor { get; set; }

    /// <summary>
    /// Used by taiko.
    /// </summary>
    public double RhythmTopStrains { get; set; }

    /// <summary>
    /// Used by taiko.
    /// </summary>
    public double ColourTopStrains { get; set; }

    /// <summary>
    /// Used by taiko.
    /// </summary>
    public double StaminaTopStrains { get; set; }

    #endregion
}