using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Catch.Difficulty;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mania.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Difficulty;
using osu.Game.Skinning;
using osu.Game.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SanDoku.Util
{
    public abstract class RulesetUtil
    {
        private static readonly Dictionary<LegacyGameMode, RulesetUtil> RulesetUtils = new RulesetUtil[]
        {
            new OsuRulesetUtil(),
            new TaikoRulesetUtil(),
            new CatchRulesetUtil(),
            new ManiaRulesetUtil()
        }.ToDictionary(rulesetUtil => rulesetUtil.LegacyGameMode);

        public static RulesetUtil GetForLegacyGameMode(LegacyGameMode gameMode)
        {
            if (RulesetUtils.TryGetValue(gameMode, out var rulesetUtil)) return rulesetUtil;
            throw new ArgumentOutOfRangeException(nameof(gameMode), gameMode, $"game mode {gameMode} does not exist");
        }

        private static LegacyMods GetDifficultyAffectingLegacyModsForRuleset(Ruleset ruleset)
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

        protected readonly LegacyGameMode LegacyGameMode;
        protected readonly Ruleset Ruleset;
        protected LegacyMods DifficultyAffectingLegacyMods { get; set; }

        protected RulesetUtil(LegacyGameMode legacyGameMode, Ruleset ruleset)
        {
            Ruleset = ruleset;
            LegacyGameMode = legacyGameMode;
            DifficultyAffectingLegacyMods = GetDifficultyAffectingLegacyModsForRuleset(ruleset);
        }

        public void AddRulesetInfoToBeatmapInfo(BeatmapInfo beatmapInfo)
        {
            var beatmapGameMode = (LegacyGameMode) beatmapInfo.RulesetID;
            var beatmapRulesetUtil = GetForLegacyGameMode(beatmapGameMode);
            beatmapInfo.Ruleset = beatmapRulesetUtil.Ruleset.RulesetInfo;
        }

        public IEnumerable<Mod> ConvertFromLegacyModsFilteredByDifficultyAffecting(LegacyMods legacyMods)
        {
            var filteredLegacyMods = legacyMods & DifficultyAffectingLegacyMods;
            lock (Ruleset)
            {
                return Ruleset.ConvertFromLegacyMods(filteredLegacyMods);
            }
        }

        public abstract Task<DiffCalcResult> CalculateDifficultyAttributes(IWorkingBeatmap beatmap, IEnumerable<Mod> mods, CancellationToken ct);
    }

    public abstract class RulesetUtil<TRuleset, TDiffAttr> : RulesetUtil where TRuleset : Ruleset, new() where TDiffAttr : DifficultyAttributes
    {
        protected RulesetUtil(LegacyGameMode legacyGameMode) : base(legacyGameMode, new TRuleset())
        {
        }

        public override async Task<DiffCalcResult> CalculateDifficultyAttributes(IWorkingBeatmap beatmap, IEnumerable<Mod> mods, CancellationToken ct)
        {
            DifficultyCalculator calculator;
            lock (Ruleset)
            {
                calculator = Ruleset.CreateDifficultyCalculator(beatmap);
            }

            // TODO possibly find a way to make this better
            var diffAttr = await Task.Run(() => calculator.Calculate(mods, ct), ct);
            if (diffAttr is not TDiffAttr tDiff)
            {
                throw new InvalidOperationException(
                    $"unexpected DifficultyAttributes type {diffAttr.GetType().FullName}, expected {typeof(TDiffAttr).FullName}");
            }

            LegacyMods legacyModsUsed;
            lock (Ruleset)
            {
                legacyModsUsed = Ruleset.ConvertToLegacyMods(tDiff.Mods);
            }

            var diff = new DiffCalcResult
            {
                BeatmapGameMode = (LegacyGameMode) beatmap.Beatmap.BeatmapInfo.RulesetID,
                GameMode = LegacyGameMode,
                Mods = legacyModsUsed,
                StarRating = tDiff.StarRating,
                MaxCombo = tDiff.MaxCombo
            };
            Map(diff, tDiff);
            return diff;
        }

        protected abstract void Map(DiffCalcResult diffCalcResult, TDiffAttr tDiff);
    }

    internal class EmptyWorkingBeatmap : WorkingBeatmap
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

    public class OsuRulesetUtil : RulesetUtil<OsuRuleset, OsuDifficultyAttributes>
    {
        public OsuRulesetUtil() : base(LegacyGameMode.Osu)
        {
        }

        protected override void Map(DiffCalcResult diffCalcResult, OsuDifficultyAttributes osuDiff)
        {
            diffCalcResult.AimStrain = osuDiff.AimStrain;
            diffCalcResult.SpeedStrain = osuDiff.SpeedStrain;
            diffCalcResult.FlashlightRating = osuDiff.FlashlightRating;
            diffCalcResult.SliderFactor = osuDiff.SliderFactor;
            diffCalcResult.ApproachRate = osuDiff.ApproachRate;
            diffCalcResult.OverallDifficulty = osuDiff.OverallDifficulty;
            diffCalcResult.DrainRate = osuDiff.DrainRate;
            diffCalcResult.HitCircleCount = osuDiff.HitCircleCount;
            diffCalcResult.SliderCount = osuDiff.SliderCount;
            diffCalcResult.SpinnerCount = osuDiff.SpinnerCount;
        }
    }

    public class TaikoRulesetUtil : RulesetUtil<TaikoRuleset, TaikoDifficultyAttributes>
    {
        public TaikoRulesetUtil() : base(LegacyGameMode.Taiko)
        {
        }

        protected override void Map(DiffCalcResult diffCalcResult, TaikoDifficultyAttributes taikoDiff)
        {
            diffCalcResult.ApproachRate = taikoDiff.ApproachRate;
            diffCalcResult.StaminaStrain = taikoDiff.StaminaStrain;
            diffCalcResult.RhythmStrain = taikoDiff.RhythmStrain;
            diffCalcResult.ColourStrain = taikoDiff.ColourStrain;
            diffCalcResult.GreatHitWindow = taikoDiff.GreatHitWindow;
        }
    }

    public class CatchRulesetUtil : RulesetUtil<CatchRuleset, CatchDifficultyAttributes>
    {
        public CatchRulesetUtil() : base(LegacyGameMode.Catch)
        {
        }

        protected override void Map(DiffCalcResult diffCalcResult, CatchDifficultyAttributes catchDiff)
        {
            diffCalcResult.ApproachRate = catchDiff.ApproachRate;
        }
    }

    public class ManiaRulesetUtil : RulesetUtil<ManiaRuleset, ManiaDifficultyAttributes>
    {
        public ManiaRulesetUtil() : base(LegacyGameMode.Mania)
        {
        }

        protected override void Map(DiffCalcResult diffCalcResult, ManiaDifficultyAttributes maniaDiff)
        {
            diffCalcResult.GreatHitWindow = maniaDiff.GreatHitWindow;
            diffCalcResult.ScoreMultiplier = maniaDiff.ScoreMultiplier;
        }
    }
}