using osu.Game.Beatmaps.Legacy;
using SanDoku.Util;

namespace SanDoku.Models
{
    public class DiffResult
    {
        public LegacyGameMode BeatmapGameMode { get; set; }

        public LegacyGameMode GameModeUsed { get; set; }

        public LegacyMods ModsUsed { get; set; }
        public DiffCalcResult DiffCalcResult { get; set; }
    }
}