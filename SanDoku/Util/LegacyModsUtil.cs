using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Skinning;
using osu.Game.Utils;
using System;
using System.IO;
using System.Linq;

namespace SanDoku.Util
{
    public static class LegacyModsUtil
    {
        private static readonly LegacyMods AllDefined = Enum.GetValues<LegacyMods>().Aggregate((current, m) => current | m);

        private class EmptyWorkingBeatmap : WorkingBeatmap
        {
            public EmptyWorkingBeatmap(BeatmapInfo beatmapInfo) : base(beatmapInfo, null)
            {
            }

            protected override IBeatmap GetBeatmap() => throw new NotImplementedException();

            protected override Texture GetBackground() => throw new NotImplementedException();

            protected override Track GetBeatmapTrack() => throw new NotImplementedException();

            protected override ISkin GetSkin() => throw new NotImplementedException();

            public override Stream GetStream(string storagePath) => throw new NotImplementedException();
        }

        public static LegacyMods GetDifficultyAffectingLegacyModsForRuleset(Ruleset ruleset)
        {
            var emptyDummyBeatmap = new EmptyWorkingBeatmap(new BeatmapInfo
            {
                Ruleset = ruleset.RulesetInfo,
                BaseDifficulty = new BeatmapDifficulty()
            });
            var difficultyAdjustmentModCombinations = ruleset.CreateDifficultyCalculator(emptyDummyBeatmap).CreateDifficultyAdjustmentModCombinations();
            var mods = ModUtils.FlattenMods(difficultyAdjustmentModCombinations)
                .Where(mod => mod is not ModNoMod)
                .Distinct()
                .ToArray();
            return ruleset.ConvertToLegacyMods(mods);
        }

        public static bool IsDefined(LegacyMods mods)
        {
            return (mods & AllDefined) == mods;
        }
    }
}