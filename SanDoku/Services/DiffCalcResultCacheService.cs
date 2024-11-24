using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using osu.Game.Beatmaps.Legacy;
using SanDoku.Models;
using SanDoku.Util;

namespace SanDoku.Services;

public class DiffCalcResultCacheService : IDiffCalcResultCacheService
{
    private readonly IMemoryCache _memoryCache;

    public DiffCalcResultCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public void Set(DiffResult diffResult, ProcessorWorkingBeatmap workingBeatmap)
    {
        var key = new CacheKey(diffResult.BeatmapMd5, diffResult.GameModeUsed, diffResult.ModsUsed);
        var entry = new CacheEntry(diffResult, workingBeatmap);
        _memoryCache.Set(key, entry, TimeSpan.FromDays(1));
    }

    public bool TryGet(string beatmapMd5, LegacyGameMode mode, LegacyMods mods, [MaybeNullWhen(false)] out DiffResult result,
        [MaybeNullWhen(false)] out ProcessorWorkingBeatmap processorWorkingBeatmap)
    {
        var key = new CacheKey(beatmapMd5, mode, mods);
        if (_memoryCache.TryGetValue(key, out CacheEntry entry))
        {
            (result, processorWorkingBeatmap) = entry;
            return true;
        }

        result = null;
        processorWorkingBeatmap = null;
        return false;
    }

    private readonly record struct CacheKey(string BeatmapMd5, LegacyGameMode Mode, LegacyMods Mods);

    private readonly record struct CacheEntry(DiffResult DiffResult, ProcessorWorkingBeatmap WorkingBeatmap);
}