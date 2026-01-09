namespace SanDoku.Models;

public record DiffCalcResult
{
    #region Attributes

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

    /// <summary>
    /// The difficulty corresponding to the aim skill.
    /// Used by osu.
    /// <c>ATTRIB_ID_AIM = 1</c>
    /// </summary>
    public double Aim { get; set; }

    /// <summary>
    /// The difficulty corresponding to the speed skill.
    /// Used by osu.
    /// <c>ATTRIB_ID_SPEED = 3.</c>
    /// </summary>
    public double Speed { get; set; }

    /// <summary>
    /// The number of clickable objects weighted by difficulty.
    /// Related to <see cref="Speed" />.
    /// Used by osu.
    /// <c>ATTRIB_ID_SPEED_NOTE_COUNT = 21.</c>
    /// </summary>
    public double SpeedNoteCount { get; set; }

    /// <summary>
    /// The difficulty corresponding to the flashlight skill.
    /// Used by osu.
    /// <c>ATTRIB_ID_FLASHLIGHT = 17</c>
    /// </summary>
    public double Flashlight { get; set; }

    /// <summary>
    /// Describes how much of <see cref="Aim" /> is contributed to by hitcircles or sliders.
    /// A value closer to 1.0 indicates most of <see cref="Aim" /> is contributed by hitcircles.
    /// A value closer to 0.0 indicates most of <see cref="Aim" /> is contributed by sliders.
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

    /// <summary>
    /// The perceived approach rate inclusive of rate-adjusting mods (DT/HT/etc).
    /// Used by osu, catch.
    /// <c>ATTRIB_ID_APPROACH_RATE = 7</c>
    /// </summary>
    public double ApproachRate { get; set; }

    /// <summary>
    /// The perceived overall difficulty inclusive of rate-adjusting mods (DT/HT/etc).
    /// Used by osu.
    /// <c>ATTRIB_ID_OVERALL_DIFFICULTY = 5</c>
    /// </summary>
    public double OverallDifficulty { get; set; }

    /// <summary>
    /// The ratio of stamina difficulty from mono-color (single colour) streams to total stamina difficulty.
    /// Used by taiko.
    /// <c>ATTRIB_ID_MONO_STAMINA_FACTOR = 29</c>
    /// </summary>
    public double MonoStaminaFactor { get; set; }

    /// <summary>
    /// The perceived hit window for a GREAT hit inclusive of rate-adjusting mods (DT/HT/etc).
    /// Used by taiko, mania.
    /// <c>ATTRIB_ID_GREAT_HIT_WINDOW = 13</c>
    /// </summary>
    public double GreatHitWindow { get; set; }

    /// <summary>
    /// The perceived hit window for an OK hit inclusive of rate-adjusting mods (DT/HT/etc).
    /// Used by taiko.
    /// <c>ATTRIB_ID_OK_HIT_WINDOW = 27</c>
    /// </summary>
    public double OkHitWindow { get; set; }

    #endregion

    #region Other Data?

    // not in the database attributes list in lazer source code

    /// <summary>Used by osu</summary>
    public double DrainRate { get; set; }

    /// <summary>Used by osu</summary>
    public int HitCircleCount { get; set; }

    /// <summary>Used by osu</summary>
    public int SliderCount { get; set; }

    /// <summary>Used by osu</summary>
    public int SpinnerCount { get; set; }

    /// <summary>
    /// The difficulty corresponding to the stamina skill.
    /// Used by taiko.
    /// </summary>
    public double Stamina { get; set; }

    /// <summary>
    /// The difficulty corresponding to the rhythm skill.
    /// Used by taiko.
    /// </summary>
    public double Rhythm { get; set; }

    /// <summary>
    /// The difficulty corresponding to the colour skill.
    /// Used by taiko.
    /// </summary>
    public double Colour { get; set; }

    /// <summary>
    /// The difficulty corresponding to the hardest parts of the map.
    /// Used by taiko.
    /// </summary>
    public double Peak { get; set; }

    #endregion
}