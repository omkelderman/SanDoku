using osu.Game.Beatmaps.Legacy;
using SanDoku.Util;

namespace SanDoku.Models;

public record DiffResult
(
    LegacyGameMode BeatmapGameMode,
    string BeatmapMd5,
    LegacyGameMode GameModeUsed,
    LegacyMods ModsUsed,
    DiffCalcResult DiffCalcResult
);