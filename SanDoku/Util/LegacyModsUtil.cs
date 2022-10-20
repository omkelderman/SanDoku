using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

namespace SanDoku.Util;

public static class LegacyModsUtil
{
    private static readonly LegacyMods AllDefinedLegacyMods = Enum.GetValues<LegacyMods>().Aggregate((current, m) => current | m);

    public static Mod? GetClassicMod(Ruleset ruleset)
    {
        // this is pretty inefficient as it creates a list of all available mods for that ruleset (which could potentially be quite a lot)
        // and then we're discarding everything except the classic mod instance, but whatever doesn't matter too much
        // as this only runs once (per ruleset) on startup and then never again so I don't care
        return ruleset.CreateAllMods().SingleOrDefault(m => m is ModClassic);
    }

    public static bool IsDefined(LegacyMods mods)
    {
        return (mods & AllDefinedLegacyMods) == mods;
    }
}