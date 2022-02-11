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
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Difficulty;
using osu.Game.Scoring.Legacy;
using SanDoku.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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

        protected readonly LegacyGameMode LegacyGameMode;
        private readonly Ruleset _ruleset;
        protected LegacyMods DifficultyAffectingLegacyMods { get; set; }

        protected RulesetUtil(LegacyGameMode legacyGameMode, Ruleset ruleset)
        {
            LegacyGameMode = legacyGameMode;
            _ruleset = ruleset;
            DifficultyAffectingLegacyMods = LegacyModsUtil.GetDifficultyAffectingLegacyModsForRuleset(_ruleset);
        }

        public void AddRulesetInfoToBeatmapInfo(BeatmapInfo beatmapInfo)
        {
            var beatmapGameMode = (LegacyGameMode)beatmapInfo.RulesetID;
            var beatmapRulesetUtil = GetForLegacyGameMode(beatmapGameMode);
            beatmapInfo.Ruleset = beatmapRulesetUtil._ruleset.RulesetInfo;
        }

        public IEnumerable<Mod> ConvertFromLegacyModsFilteredByDifficultyAffecting(LegacyMods legacyMods)
        {
            var filteredLegacyMods = legacyMods & DifficultyAffectingLegacyMods;
            return ConvertFromLegacyMods(filteredLegacyMods);
        }

        private IEnumerable<Mod> ConvertFromLegacyMods(LegacyMods legacyMods)
        {
            lock (_ruleset)
            {
                return _ruleset.ConvertFromLegacyMods(legacyMods);
            }
        }

        protected LegacyMods ConvertToLegacyMods(Mod[] mods)
        {
            lock (_ruleset)
            {
                return _ruleset.ConvertToLegacyMods(mods);
            }
        }

        public ScoreInfoWithNewStyleModArray MapToScoreInfoObjectWithNewStyleMods(ScoreInfo scoreInfo)
        {
            var mods = ConvertFromLegacyMods(scoreInfo.Mods).ToArray();
            return new ScoreInfoWithNewStyleModArray { Mods = mods, ScoreInfo = scoreInfo };
        }

        protected DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap)
        {
            lock (_ruleset)
            {
                return _ruleset.CreateDifficultyCalculator(beatmap);
            }
        }

        protected PerformanceCalculator CreatePerformanceCalculator(DifficultyAttributes attributes, ScoreInfoWithNewStyleModArray scoreInfo)
        {
            lock (_ruleset)
            {
                var score = BuildGameScoreInfo(_ruleset.RulesetInfo, scoreInfo.Mods, scoreInfo.ScoreInfo);
                return _ruleset.CreatePerformanceCalculator(attributes, score);
            }
        }

        private osu.Game.Scoring.ScoreInfo BuildGameScoreInfo(RulesetInfo rulesetInfo, Mod[] mods, ScoreInfo scoreInfo)
        {
            var gameScoreInfo = new osu.Game.Scoring.ScoreInfo
            {
                RulesetID = (int)LegacyGameMode,
                Ruleset = rulesetInfo,
                Mods = mods,
                MaxCombo = scoreInfo.MaxCombo,
                TotalScore = scoreInfo.TotalScore,
                Statistics = new Dictionary<HitResult, int>()
            };
            gameScoreInfo.SetCount50(scoreInfo.Count50);
            gameScoreInfo.SetCount100(scoreInfo.Count100);
            gameScoreInfo.SetCount300(scoreInfo.Count300);
            gameScoreInfo.SetCountMiss(scoreInfo.CountMiss);
            gameScoreInfo.SetCountKatu(scoreInfo.CountKatu);
            gameScoreInfo.SetCountGeki(scoreInfo.CountGeki);
            LegacyScoreDecoder.PopulateAccuracy(gameScoreInfo);
            return gameScoreInfo;
        }

        public abstract (DiffCalcResult diffCalcResult, LegacyGameMode beatmapGameMode, LegacyGameMode gameModeUsed, LegacyMods modsUsed)
            CalculateDifficultyAttributes(IWorkingBeatmap beatmap, IEnumerable<Mod> mods, CancellationToken ct);

        public abstract (double pp, Dictionary<string, double> categoryDifficulty) CalculatePerformance(DiffCalcResult diffResult, ScoreInfoWithNewStyleModArray scoreInfo);
    }

    public abstract class RulesetUtil<TRuleset> : RulesetUtil where TRuleset : Ruleset, ILegacyRuleset, new()
    {
        protected RulesetUtil(TRuleset ruleset) : base((LegacyGameMode)ruleset.LegacyID, ruleset)
        {
        }
    }

    public abstract class RulesetUtil<TRuleset, TDiffAttr> : RulesetUtil<TRuleset> where TRuleset : Ruleset, ILegacyRuleset, new()
        where TDiffAttr : DifficultyAttributes, new()
    {
        protected RulesetUtil() : base(new TRuleset())
        {
        }

        public override (DiffCalcResult diffCalcResult, LegacyGameMode beatmapGameMode, LegacyGameMode gameModeUsed, LegacyMods modsUsed)
            CalculateDifficultyAttributes(IWorkingBeatmap beatmap, IEnumerable<Mod> mods, CancellationToken ct)
        {
            var calculator = CreateDifficultyCalculator(beatmap);
            var diffAttr = calculator.Calculate(mods, ct);
            if (diffAttr is not TDiffAttr tDiff)
            {
                throw new InvalidOperationException(
                    $"unexpected DifficultyAttributes type {diffAttr.GetType().FullName}, expected {typeof(TDiffAttr).FullName}");
            }

            var legacyModsUsed = ConvertToLegacyMods(tDiff.Mods);
            var diff = new DiffCalcResult
            {
                StarRating = tDiff.StarRating,
                MaxCombo = tDiff.MaxCombo
            };
            Map(diff, tDiff);
            return (diff, (LegacyGameMode)beatmap.Beatmap.BeatmapInfo.RulesetID, LegacyGameMode, legacyModsUsed);
        }

        public override (double pp, Dictionary<string, double> categoryDifficulty) CalculatePerformance(DiffCalcResult diffResult, ScoreInfoWithNewStyleModArray scoreInfo)
        {
            var tDiff = new TDiffAttr { StarRating = diffResult.StarRating, MaxCombo = diffResult.MaxCombo };
            Map(tDiff, diffResult);
            var ppCalc = CreatePerformanceCalculator(tDiff, scoreInfo);
            var categoryDifficulty = new Dictionary<string, double>();
            var pp = ppCalc.Calculate(categoryDifficulty);
            return (pp, categoryDifficulty);
        }

        protected abstract void Map(DiffCalcResult diffCalcResult, TDiffAttr tDiff);
        protected abstract void Map(TDiffAttr tDiff, DiffCalcResult diffCalcResult);
    }

    public class OsuRulesetUtil : RulesetUtil<OsuRuleset, OsuDifficultyAttributes>
    {
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

        protected override void Map(OsuDifficultyAttributes osuDiff, DiffCalcResult diffCalcResult)
        {
            osuDiff.AimStrain = diffCalcResult.AimStrain;
            osuDiff.SpeedStrain = diffCalcResult.SpeedStrain;
            osuDiff.FlashlightRating = diffCalcResult.FlashlightRating;
            osuDiff.SliderFactor = diffCalcResult.SliderFactor;
            osuDiff.ApproachRate = diffCalcResult.ApproachRate;
            osuDiff.OverallDifficulty = diffCalcResult.OverallDifficulty;
            osuDiff.DrainRate = diffCalcResult.DrainRate;
            osuDiff.HitCircleCount = diffCalcResult.HitCircleCount;
            osuDiff.SliderCount = diffCalcResult.SliderCount;
            osuDiff.SpinnerCount = diffCalcResult.SpinnerCount;
        }
    }

    public class TaikoRulesetUtil : RulesetUtil<TaikoRuleset, TaikoDifficultyAttributes>
    {
        protected override void Map(DiffCalcResult diffCalcResult, TaikoDifficultyAttributes taikoDiff)
        {
            diffCalcResult.ApproachRate = taikoDiff.ApproachRate;
            diffCalcResult.StaminaStrain = taikoDiff.StaminaStrain;
            diffCalcResult.RhythmStrain = taikoDiff.RhythmStrain;
            diffCalcResult.ColourStrain = taikoDiff.ColourStrain;
            diffCalcResult.GreatHitWindow = taikoDiff.GreatHitWindow;
        }

        protected override void Map(TaikoDifficultyAttributes taikoDiff, DiffCalcResult diffCalcResult)
        {
            taikoDiff.ApproachRate = diffCalcResult.ApproachRate;
            taikoDiff.StaminaStrain = diffCalcResult.StaminaStrain;
            taikoDiff.RhythmStrain = diffCalcResult.RhythmStrain;
            taikoDiff.ColourStrain = diffCalcResult.ColourStrain;
            taikoDiff.GreatHitWindow = diffCalcResult.GreatHitWindow;
        }
    }

    public class CatchRulesetUtil : RulesetUtil<CatchRuleset, CatchDifficultyAttributes>
    {
        protected override void Map(DiffCalcResult diffCalcResult, CatchDifficultyAttributes catchDiff)
        {
            diffCalcResult.ApproachRate = catchDiff.ApproachRate;
        }

        protected override void Map(CatchDifficultyAttributes catchDiff, DiffCalcResult diffCalcResult)
        {
            catchDiff.ApproachRate = diffCalcResult.ApproachRate;
        }
    }

    public class ManiaRulesetUtil : RulesetUtil<ManiaRuleset, ManiaDifficultyAttributes>
    {
        protected override void Map(DiffCalcResult diffCalcResult, ManiaDifficultyAttributes maniaDiff)
        {
            diffCalcResult.GreatHitWindow = maniaDiff.GreatHitWindow;
            diffCalcResult.ScoreMultiplier = maniaDiff.ScoreMultiplier;
        }

        protected override void Map(ManiaDifficultyAttributes maniaDiff, DiffCalcResult diffCalcResult)
        {
            maniaDiff.GreatHitWindow = diffCalcResult.GreatHitWindow;
            maniaDiff.ScoreMultiplier = diffCalcResult.ScoreMultiplier;
        }
    }
}