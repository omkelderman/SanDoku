using osu.Game.Beatmaps.Legacy;

namespace SanDoku.Util
{
    public class DiffCalcResult
    {
        public LegacyGameMode BeatmapGameMode { get; set; }

        public LegacyGameMode GameMode { get; set; }

        public LegacyMods Mods { get; set; }

        /// <summary>
        /// All
        /// </summary>
        public double StarRating { get; set; }

        /// <summary>
        /// All
        /// </summary>
        public int MaxCombo { get; set; }

        /// <summary>
        /// osu
        /// </summary>
        public double AimStrain { get; set; }

        /// <summary>
        /// osu
        /// </summary>
        public double SpeedStrain { get; set; }

        /// <summary>
        /// osu
        /// </summary>
        public double FlashlightRating { get; set; }

        /// <summary>
        /// osu
        /// </summary>
        public double SliderFactor { get; set; }

        /// <summary>
        /// osu, taiko, catch
        /// </summary>
        public double ApproachRate { get; set; }

        /// <summary>
        /// osu
        /// </summary>
        public double OverallDifficulty { get; set; }

        /// <summary>
        /// osu
        /// </summary>
        public double DrainRate { get; set; }

        /// <summary>
        /// osu
        /// </summary>
        public int HitCircleCount { get; set; }

        /// <summary>
        /// osu
        /// </summary>
        public int SliderCount { get; set; }

        /// <summary>
        /// osu
        /// </summary>
        public int SpinnerCount { get; set; }

        /// <summary>
        /// taiko
        /// </summary>
        public double StaminaStrain { get; set; }

        /// <summary>
        /// taiko
        /// </summary>
        public double RhythmStrain { get; set; }

        /// <summary>
        /// taiko
        /// </summary>
        public double ColourStrain { get; set; }

        /// <summary>
        /// taiko, mania
        /// </summary>
        public double GreatHitWindow { get; set; }

        /// <summary>
        /// mania
        /// </summary>
        public double ScoreMultiplier { get; set; }
    }
}