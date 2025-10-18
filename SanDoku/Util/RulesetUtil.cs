using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Database;
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
using osu.Game.Scoring.Legacy;
using SanDoku.Extensions;
using SanDoku.Models;

namespace SanDoku.Util;

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
        lock (RulesetUtils)
        {
            if (RulesetUtils.TryGetValue(gameMode, out var rulesetUtil)) return rulesetUtil;
        }
        throw new ArgumentOutOfRangeException(nameof(gameMode), gameMode, $"game mode {gameMode} does not exist");
    }

    public static RulesetInfo[] GetAllAvailableRulesetInfos()
    {
        lock (RulesetUtils)
        {
            return RulesetUtils.Values.Select(x => x.GetRulesetInfo()).ToArray();
        }
    }

    public readonly LegacyGameMode LegacyGameMode;
    private readonly Ruleset _ruleset;
    private readonly Mod? _classicMod;

    protected RulesetUtil(LegacyGameMode legacyGameMode, Ruleset ruleset)
    {
        LegacyGameMode = legacyGameMode;
        _ruleset = ruleset;
        _classicMod = LegacyModsUtil.GetClassicMod(ruleset);
    }

    private RulesetInfo GetRulesetInfo()
    {
        lock (_ruleset)
        {
            return _ruleset.RulesetInfo.Clone();
        }
    }

    public Mod[] ConvertFromLegacyModsAndAddClassicMod(LegacyMods legacyMods)
    {
        lock (_ruleset)
        {
            var e = _ruleset.ConvertFromLegacyMods(legacyMods);
            if (_classicMod != null)
            {
                e = e.Append(_classicMod);
            }
            return e.ToArray();
        }
    }

    protected LegacyMods ConvertToLegacyMods(Mod[] mods)
    {
        lock (_ruleset)
        {
            return _ruleset.ConvertToLegacyMods(mods);
        }
    }

    protected DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap)
    {
        lock (_ruleset)
        {
            return _ruleset.CreateDifficultyCalculator(beatmap);
        }
    }

    protected PerformanceCalculator CreatePerformanceCalculator()
    {
        lock (_ruleset)
        {
            var ppCalc = _ruleset.CreatePerformanceCalculator();
            return ppCalc ?? throw new InvalidOperationException("Unable to create pp calculator, should never happen");
        }
    }

    protected osu.Game.Scoring.ScoreInfo BuildGameScoreInfo(Mod[] mods, ScoreInfo scoreInfo, WorkingBeatmap workingBeatmap)
    {
        var rulesetInfo = GetRulesetInfo();
            
        var gameScoreInfo = new osu.Game.Scoring.ScoreInfo
        {
            Ruleset = rulesetInfo,
            Mods = mods,
            MaxCombo = scoreInfo.MaxCombo,
            TotalScore = scoreInfo.TotalScore,

            // we treat the score info coming in as if it was an old stable score
            // which is (afaik) correct for score info objects coming from api v1
            // we need to set this version to trigger reprocessing to new standardised scoring
            TotalScoreVersion = LegacyScoreEncoder.FIRST_LAZER_VERSION + 1,
            IsLegacyScore = true,
            LegacyTotalScore = scoreInfo.TotalScore,
            BeatmapInfo = workingBeatmap.BeatmapInfo
        };
        gameScoreInfo.SetCount50(scoreInfo.Count50);
        gameScoreInfo.SetCount100(scoreInfo.Count100);
        gameScoreInfo.SetCount300(scoreInfo.Count300);
        gameScoreInfo.SetCountMiss(scoreInfo.CountMiss);
        gameScoreInfo.SetCountKatu(scoreInfo.CountKatu);
        gameScoreInfo.SetCountGeki(scoreInfo.CountGeki);
            
        LegacyScoreDecoder.PopulateMaximumStatistics(gameScoreInfo, workingBeatmap);
        StandardisedScoreMigrationTools.UpdateFromLegacy(gameScoreInfo, workingBeatmap);
            
        return gameScoreInfo;
    }

    public abstract (DiffCalcResult diffCalcResult, LegacyMods modsUsed) CalculateDifficultyAttributes(IWorkingBeatmap beatmap, IEnumerable<Mod> mods,
        CancellationToken ct);

    public abstract PpOutput CalculatePerformance(WorkingBeatmap workingBeatmap, DiffCalcResult diffResult, Mod[] modsUsed, ScoreInfo scoreInfo);
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

    public override (DiffCalcResult diffCalcResult, LegacyMods modsUsed) CalculateDifficultyAttributes(IWorkingBeatmap beatmap, IEnumerable<Mod> mods,
        CancellationToken ct)
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
        MapProperties(diff, tDiff);
        return (diff, legacyModsUsed);
    }

    public override PpOutput CalculatePerformance(WorkingBeatmap workingBeatmap, DiffCalcResult diffResult, Mod[] modsUsed, ScoreInfo scoreInfo)
    {
        var tDiff = new TDiffAttr
        {
            Mods = modsUsed,
            StarRating = diffResult.StarRating,
            MaxCombo = diffResult.MaxCombo
        };
        MapProperties(tDiff, diffResult);
        var osuScoreInfo = BuildGameScoreInfo(modsUsed, scoreInfo, workingBeatmap);
        var ppCalc = CreatePerformanceCalculator();
        var result = ppCalc.Calculate(osuScoreInfo, tDiff);

        var pp = result.Total.NaNOrInfinityToNull();
        var attributes = result.GetAttributesForDisplay()
            .Select(attr => new PpDisplayAttribute(attr.PropertyName, attr.DisplayName, attr.Value.NaNOrInfinityToNull()))
            .ToList();
        return new PpOutput(pp, attributes);
    }

    protected virtual void MapProperties(DiffCalcResult diffCalcResult, TDiffAttr tDiff)
    {
    }

    protected virtual void MapProperties(TDiffAttr tDiff, DiffCalcResult diffCalcResult)
    {
    }
}

public class OsuRulesetUtil : RulesetUtil<OsuRuleset, OsuDifficultyAttributes>
{
    protected override void MapProperties(DiffCalcResult diffCalcResult, OsuDifficultyAttributes osuDiff)
    {
        diffCalcResult.AimDifficulty = osuDiff.AimDifficulty;
        diffCalcResult.AimDifficultSliderCount = osuDiff.AimDifficultSliderCount;
        diffCalcResult.SpeedDifficulty = osuDiff.SpeedDifficulty;
        diffCalcResult.SpeedNoteCount = osuDiff.SpeedNoteCount;
        diffCalcResult.FlashlightDifficulty = osuDiff.FlashlightDifficulty;
        diffCalcResult.SliderFactor = osuDiff.SliderFactor;
        // diffCalcResult.AimTopWeightedSliderFactor = osuDiff.AimTopWeightedSliderFactor;
        // diffCalcResult.SpeedTopWeightedSliderFactor = osuDiff.SpeedTopWeightedSliderFactor;
        diffCalcResult.AimDifficultStrainCount = osuDiff.AimDifficultStrainCount;
        diffCalcResult.SpeedDifficultStrainCount = osuDiff.SpeedDifficultStrainCount;
        // diffCalcResult.NestedScorePerObject = osuDiff.NestedScorePerObject;
        // diffCalcResult.LegacyScoreBaseMultiplier = osuDiff.LegacyScoreBaseMultiplier;
        // diffCalcResult.MaximumLegacyComboScore = osuDiff.MaximumLegacyComboScore;

        diffCalcResult.DrainRate = osuDiff.DrainRate;
        diffCalcResult.HitCircleCount = osuDiff.HitCircleCount;
        diffCalcResult.SliderCount = osuDiff.SliderCount;
        diffCalcResult.SpinnerCount = osuDiff.SpinnerCount;
    }

    protected override void MapProperties(OsuDifficultyAttributes osuDiff, DiffCalcResult diffCalcResult)
    {
        osuDiff.AimDifficulty = diffCalcResult.AimDifficulty;
        osuDiff.AimDifficultSliderCount = diffCalcResult.AimDifficultSliderCount;
        osuDiff.SpeedDifficulty = diffCalcResult.SpeedDifficulty;
        osuDiff.SpeedNoteCount = diffCalcResult.SpeedNoteCount;
        osuDiff.FlashlightDifficulty = diffCalcResult.FlashlightDifficulty;
        osuDiff.SliderFactor = diffCalcResult.SliderFactor;
        // osuDiff.AimTopWeightedSliderFactor = diffCalcResult.AimTopWeightedSliderFactor;
        // osuDiff.SpeedTopWeightedSliderFactor = diffCalcResult.SpeedTopWeightedSliderFactor;
        osuDiff.AimDifficultStrainCount = diffCalcResult.AimDifficultStrainCount;
        osuDiff.SpeedDifficultStrainCount = diffCalcResult.SpeedDifficultStrainCount;
        // osuDiff.NestedScorePerObject = diffCalcResult.NestedScorePerObject;
        // osuDiff.LegacyScoreBaseMultiplier = diffCalcResult.LegacyScoreBaseMultiplier;
        // osuDiff.MaximumLegacyComboScore = diffCalcResult.MaximumLegacyComboScore;
    
        osuDiff.DrainRate = diffCalcResult.DrainRate;
        osuDiff.HitCircleCount = diffCalcResult.HitCircleCount;
        osuDiff.SliderCount = diffCalcResult.SliderCount;
        osuDiff.SpinnerCount = diffCalcResult.SpinnerCount;
    }
}

public class TaikoRulesetUtil : RulesetUtil<TaikoRuleset, TaikoDifficultyAttributes>
{
    protected override void MapProperties(DiffCalcResult diffCalcResult, TaikoDifficultyAttributes taikoDiff)
    {
        diffCalcResult.RhythmDifficulty = taikoDiff.RhythmDifficulty;
        diffCalcResult.ReadingDifficulty = taikoDiff.ReadingDifficulty;
        diffCalcResult.ColourDifficulty = taikoDiff.ColourDifficulty;
        diffCalcResult.StaminaDifficulty = taikoDiff.StaminaDifficulty;
        diffCalcResult.MonoStaminaFactor = taikoDiff.MonoStaminaFactor;
        diffCalcResult.RhythmTopStrains = taikoDiff.RhythmTopStrains;
        diffCalcResult.ColourTopStrains = taikoDiff.ColourTopStrains;
        diffCalcResult.StaminaTopStrains = taikoDiff.StaminaTopStrains;
    }

    protected override void MapProperties(TaikoDifficultyAttributes taikoDiff, DiffCalcResult diffCalcResult)
    {
        taikoDiff.RhythmDifficulty = diffCalcResult.RhythmDifficulty;
        taikoDiff.ReadingDifficulty = diffCalcResult.ReadingDifficulty;
        taikoDiff.ColourDifficulty = diffCalcResult.ColourDifficulty;
        taikoDiff.StaminaDifficulty = diffCalcResult.StaminaDifficulty;
        taikoDiff.MonoStaminaFactor = diffCalcResult.MonoStaminaFactor;
        taikoDiff.RhythmTopStrains = diffCalcResult.RhythmTopStrains;
        taikoDiff.ColourTopStrains = diffCalcResult.ColourTopStrains;
        taikoDiff.StaminaTopStrains = diffCalcResult.StaminaTopStrains;
    }
}

public class CatchRulesetUtil : RulesetUtil<CatchRuleset, CatchDifficultyAttributes>
{
}

public class ManiaRulesetUtil : RulesetUtil<ManiaRuleset, ManiaDifficultyAttributes>
{
}