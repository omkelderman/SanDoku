using System.Diagnostics.CodeAnalysis;
using osu.Game.Beatmaps.Legacy;
using SanDoku.Models;
using SanDoku.Util;

namespace SanDoku.Services;

public interface IDiffCalcResultCacheService
{
    void Set(DiffResult diffResult, ProcessorWorkingBeatmap workingBeatmap);

    bool TryGet(string beatmapMd5, LegacyGameMode mode, LegacyMods mods, [MaybeNullWhen(false)] out DiffResult diffResult,
        [MaybeNullWhen(false)] out ProcessorWorkingBeatmap processorWorkingBeatmap);
}