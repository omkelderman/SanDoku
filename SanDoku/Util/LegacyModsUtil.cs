using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Skinning;
using osu.Game.Utils;

namespace SanDoku.Util;

public static class LegacyModsUtil
{
    private static readonly LegacyMods AllDefined = Enum.GetValues<LegacyMods>().Aggregate((current, m) => current | m);

    private class EmptyWorkingBeatmap : WorkingBeatmap
    {
        public EmptyWorkingBeatmap(BeatmapInfo beatmapInfo) : base(beatmapInfo, null)
        {
        }

        protected override IBeatmap GetBeatmap() => throw new InvalidOperationException();

        protected override Texture GetBackground() => throw new InvalidOperationException();

        protected override Track GetBeatmapTrack() => throw new InvalidOperationException();

        protected override ISkin GetSkin() => throw new InvalidOperationException();

        public override Stream GetStream(string storagePath) => throw new InvalidOperationException();
    }

    public static (LegacyMods, Mod?) GetDifficultyAffectingLegacyModsAndClassicModForRuleset(Ruleset ruleset)
    {
        var difficultyAffectingLegacyMods = GetDifficultyAffectingLegacyMods(ruleset);
        var classicMod = GetClassicMod(ruleset);
        return (difficultyAffectingLegacyMods, classicMod);
    }

    private static LegacyMods GetDifficultyAffectingLegacyMods(Ruleset ruleset)
    {
        var emptyDummyBeatmap = new EmptyWorkingBeatmap(new BeatmapInfo
        {
            Ruleset = ruleset.RulesetInfo,
            Difficulty = new BeatmapDifficulty()
        });
        // this is some incredibly inefficient logic, as CreateDifficultyAdjustmentModCombinations() creates a super handy list of Mod objects
        // where also all the possible combinations are added which I guess is indeed super useful, but we're only interested in the final list of single mods
        // so after that we flatten the whole thing again lol
        // luckily this logic only runs once (per ruleset) on startup and then never again so I don't care
        var difficultyAdjustmentModCombinations = ruleset.CreateDifficultyCalculator(emptyDummyBeatmap).CreateDifficultyAdjustmentModCombinations();
        var mods = ModUtils.FlattenMods(difficultyAdjustmentModCombinations)
            .Where(mod => mod is not ModNoMod)
            .Distinct()
            .ToArray();
        return ruleset.ConvertToLegacyMods(mods);
    }

    private static Mod? GetClassicMod(Ruleset ruleset)
    {
        // once again an incredibly inefficient way, but it works and just as above, it only runs once (per ruleset) on startup so whatever
        return ruleset.CreateAllMods().SingleOrDefault(m => m is ModClassic);
    }

    public static bool IsDefined(LegacyMods mods)
    {
        return (mods & AllDefined) == mods;
    }
}