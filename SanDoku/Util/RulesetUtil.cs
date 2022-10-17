using NuGet.Packaging.Rules;
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
            return RulesetUtils.Values.Select(x => new RulesetInfo(x._ruleset.RulesetInfo.ShortName, x._ruleset.RulesetInfo.Name,
                x._ruleset.RulesetInfo.InstantiationInfo, x._ruleset.RulesetInfo.OnlineID)).ToArray();
        }
    }
    protected readonly LegacyGameMode LegacyGameMode;
    private readonly Ruleset _ruleset;
    private readonly LegacyMods _difficultyAffectingLegacyMods;
    private readonly Mod? _classicMod;

    protected RulesetUtil(LegacyGameMode legacyGameMode, Ruleset ruleset)
    {
        LegacyGameMode = legacyGameMode;
        _ruleset = ruleset;
        (_difficultyAffectingLegacyMods, _classicMod) = LegacyModsUtil.GetDifficultyAffectingLegacyModsAndClassicModForRuleset(_ruleset);
    }

    public void AddRulesetInfoToBeatmapInfo(BeatmapInfo beatmapInfo)
    {
        var beatmapGameMode = (LegacyGameMode)beatmapInfo.Ruleset.OnlineID;
        var beatmapRulesetUtil = GetForLegacyGameMode(beatmapGameMode);
        beatmapInfo.Ruleset = beatmapRulesetUtil._ruleset.RulesetInfo;
    }

    public Mod[] ConvertFromLegacyModsFilteredByDifficultyAffectingAndAddClassicMod(LegacyMods legacyMods)
    {
        var filteredLegacyMods = legacyMods & _difficultyAffectingLegacyMods;
        return ConvertFromLegacyModsAndAddClassicMod(filteredLegacyMods);
    }

    private Mod[] ConvertFromLegacyModsAndAddClassicMod(LegacyMods legacyMods)
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

    public ScoreInfoWithNewStyleModArray MapToScoreInfoObjectWithNewStyleModsWithClassicMod(ScoreInfo scoreInfo)
    {
        var mods = ConvertFromLegacyModsAndAddClassicMod(scoreInfo.Mods);
        return new ScoreInfoWithNewStyleModArray(mods, scoreInfo);
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
            if (ppCalc == null) throw new InvalidOperationException("Unable to create pp calculator, should never happen");
            return ppCalc;
        }
    }

    protected osu.Game.Scoring.ScoreInfo BuildGameScoreInfo(Mod[] mods, ScoreInfo scoreInfo)
    {
        lock (_ruleset)
        {
            var gameScoreInfo = new osu.Game.Scoring.ScoreInfo
            {
                Ruleset = _ruleset.RulesetInfo,
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
    }

    public abstract (DiffCalcResult diffCalcResult, LegacyGameMode beatmapGameMode, LegacyGameMode gameModeUsed, LegacyMods modsUsed)
        CalculateDifficultyAttributes(IWorkingBeatmap beatmap, IEnumerable<Mod> mods, CancellationToken ct);

    public abstract PpOutput CalculatePerformance(DiffCalcResult diffResult, ScoreInfoWithNewStyleModArray scoreInfo);
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
        return (diff, (LegacyGameMode)beatmap.Beatmap.BeatmapInfo.Ruleset.OnlineID, LegacyGameMode, legacyModsUsed);
    }

    public override PpOutput CalculatePerformance(DiffCalcResult diffResult, ScoreInfoWithNewStyleModArray scoreInfo)
    {
        var tDiff = new TDiffAttr { StarRating = diffResult.StarRating, MaxCombo = diffResult.MaxCombo, Mods = scoreInfo.Mods};
        Map(tDiff, diffResult);
        var ppCalc = CreatePerformanceCalculator();
        var osuScoreInfo = BuildGameScoreInfo(scoreInfo.Mods, scoreInfo.ScoreInfo);
        var result = ppCalc.Calculate(osuScoreInfo, tDiff);

        var pp = result.Total.NaNOrInfinityToNull();
        var attributes = result.GetAttributesForDisplay()
            .Select(attr => new PpDisplayAttribute(attr.PropertyName, attr.DisplayName, attr.Value.NaNOrInfinityToNull()))
            .ToList();
        return new PpOutput(pp, attributes);
    }

    protected abstract void Map(DiffCalcResult diffCalcResult, TDiffAttr tDiff);
    protected abstract void Map(TDiffAttr tDiff, DiffCalcResult diffCalcResult);
}

public class OsuRulesetUtil : RulesetUtil<OsuRuleset, OsuDifficultyAttributes>
{
    protected override void Map(DiffCalcResult diffCalcResult, OsuDifficultyAttributes osuDiff)
    {
        diffCalcResult.Aim = osuDiff.AimDifficulty;
        diffCalcResult.Speed = osuDiff.SpeedDifficulty;
        diffCalcResult.OverallDifficulty = osuDiff.OverallDifficulty;
        diffCalcResult.ApproachRate = osuDiff.ApproachRate;
        diffCalcResult.Flashlight = osuDiff.FlashlightDifficulty;
        diffCalcResult.SliderFactor = osuDiff.SliderFactor;
        diffCalcResult.SpeedNoteCount = osuDiff.SpeedNoteCount;

        diffCalcResult.DrainRate = osuDiff.DrainRate;
        diffCalcResult.HitCircleCount = osuDiff.HitCircleCount;
        diffCalcResult.SliderCount = osuDiff.SliderCount;
        diffCalcResult.SpinnerCount = osuDiff.SpinnerCount;
    }

    protected override void Map(OsuDifficultyAttributes osuDiff, DiffCalcResult diffCalcResult)
    {
        osuDiff.AimDifficulty = diffCalcResult.Aim;
        osuDiff.SpeedDifficulty = diffCalcResult.Speed;
        osuDiff.OverallDifficulty = diffCalcResult.OverallDifficulty;
        osuDiff.ApproachRate = diffCalcResult.ApproachRate;
        osuDiff.FlashlightDifficulty = diffCalcResult.Flashlight;
        osuDiff.SliderFactor = diffCalcResult.SliderFactor;
        osuDiff.SpeedNoteCount = diffCalcResult.SpeedNoteCount;

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
        diffCalcResult.GreatHitWindow = taikoDiff.GreatHitWindow;
        diffCalcResult.Stamina = taikoDiff.StaminaDifficulty;
        diffCalcResult.Rhythm = taikoDiff.RhythmDifficulty;
        diffCalcResult.Colour = taikoDiff.ColourDifficulty;
        diffCalcResult.Peak = taikoDiff.PeakDifficulty;
    }

    protected override void Map(TaikoDifficultyAttributes taikoDiff, DiffCalcResult diffCalcResult)
    {
        taikoDiff.GreatHitWindow = diffCalcResult.GreatHitWindow;
        taikoDiff.StaminaDifficulty = diffCalcResult.Stamina;
        taikoDiff.RhythmDifficulty = diffCalcResult.Rhythm;
        taikoDiff.ColourDifficulty = diffCalcResult.Colour;
        taikoDiff.PeakDifficulty = diffCalcResult.Peak;
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
    }

    protected override void Map(ManiaDifficultyAttributes maniaDiff, DiffCalcResult diffCalcResult)
    {
        maniaDiff.GreatHitWindow = diffCalcResult.GreatHitWindow;
    }
}