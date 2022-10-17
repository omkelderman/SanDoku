using Microsoft.AspNetCore.Mvc;
using NJsonSchema.Annotations;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Utils;
using SanDoku.Extensions;
using SanDoku.Models;
using SanDoku.Util;
using System.Net;

namespace SanDoku.Controllers;

[ApiController]
[Route("")]
public class ProcessorController : ControllerBase
{
    private readonly ILogger<ProcessorController> _logger;

    public ProcessorController(ILogger<ProcessorController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Process a .osu file and calculate all difficulty properties of the map
    /// </summary>
    /// <param name="beatmap">The contents of an .osu file, must be the correct Content-Type, optionally supports Content-Encoding "gzip" and "br"</param>
    /// <param name="mode">Override game mode</param>
    /// <param name="mods">Optionally provide mods</param>
    /// <param name="ct"></param>
    /// <returns></returns>
    [HttpPost("diff")]
    [Consumes(OsuInputFormatter.ContentType)]
    [ProducesResponseType(typeof(DiffResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int) HttpStatusCode.BadRequest)]
    public ActionResult<DiffResult> CalcDiff([FromBody, JsonSchemaType(typeof(byte[]))] BeatmapInput beatmap, [FromQuery] LegacyGameMode? mode = null,
        [FromQuery] LegacyMods mods = LegacyMods.None, CancellationToken ct = default)
    {
        if (beatmap.ContentLength == 0)
        {
            _logger.LogDebug("[diff-calc] empty input error");
            ModelState.AddModelError(nameof(beatmap), "Empty input not valid");
            return ValidationProblem();
        }

        IBeatmap beatmapActual;
        try
        {
            // this next call is expensive, and osu does not provide an async variant,
            // hence why we're doing it here where we're already inside a thread-pool within the controller context (so doing expensive work, while not ideal, is ok)
            // and not within the InputFormatter where everything needs to be async
            beatmapActual = beatmap.DecodeBeatmap();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "[diff-calc] input parse error");
            ModelState.AddModelError(nameof(beatmap), $"parse error: {ex.GetType()}: {ex.Message}");
            return ValidationProblem();
        }

        _logger.LogInformation("[diff-calc] [{md5}] {beatmapActual}", beatmap.Md5Checksum, beatmapActual.ToString());

        var modeToPick = mode ?? (LegacyGameMode) beatmapActual.BeatmapInfo.Ruleset.OnlineID;
        var rulesetUtil = RulesetUtil.GetForLegacyGameMode(modeToPick);
        var filtered = rulesetUtil.ConvertFromLegacyModsFilteredByDifficultyAffectingAndAddClassicMod(mods);

        if (!ModUtils.CheckCompatibleSet(filtered, out var invalid))
        {
            var invalidModsStr = string.Join(',', invalid.Select(mod => mod.Acronym));
            _logger.LogDebug("Invalid mod combination requested {invalidModsStr} for map {md5}", invalidModsStr, beatmap.Md5Checksum);
            ModelState.AddModelError(nameof(mods), $"invalid mod combination: {invalidModsStr}");
            return ValidationProblem();
        }

        rulesetUtil.AddRulesetInfoToBeatmapInfo(beatmapActual.BeatmapInfo);

        var workingBeatmap = new ProcessorWorkingBeatmap(beatmapActual);

        _logger.LogDebug("[diff-calc] [{md5}] start processing...", beatmap.Md5Checksum);
        var (diffCalcResult, beatmapGameMode, gameModeUsed, modsUsed) = rulesetUtil.CalculateDifficultyAttributes(workingBeatmap, filtered, ct);
        _logger.LogDebug("[diff-calc] [{md5}] processing done!", beatmap.Md5Checksum);

        return new DiffResult(beatmapGameMode, beatmap.Md5Checksum, gameModeUsed, modsUsed, diffCalcResult);
    }

    /// <summary>
    /// Calculate PP of a certain score
    /// </summary>
    /// <param name="ppInput">diffcalc values and score values</param>
    /// <returns></returns>
    [HttpPost("pp")]
    [ProducesResponseType(typeof(PpOutput), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    public ActionResult<PpOutput> CalcPp([FromBody] PpInput ppInput)
    {
        if (!Enum.IsDefined(ppInput.GameMode)) ModelState.AddModelError(nameof(ppInput.GameMode), $"invalid game mode value: {ppInput.GameMode}");
        if (!LegacyModsUtil.IsDefined(ppInput.ScoreInfo.Mods))
            ModelState.AddModelError($"{nameof(ppInput.ScoreInfo)}.{nameof(ppInput.ScoreInfo.Mods)}", $"invalid mods value: {ppInput.ScoreInfo.Mods}");

        if (!ModelState.IsValid)
        {
            _logger.LogDebug("[pp-calc] validation errors: {error}", ModelState.ToErrorMessage());
            return ValidationProblem();
        }

        var rulesetUtil = RulesetUtil.GetForLegacyGameMode(ppInput.GameMode);

        var scoreInfoWithModArray = rulesetUtil.MapToScoreInfoObjectWithNewStyleModsWithClassicMod(ppInput.ScoreInfo);

        // check if mods are illegal
        if (!ModUtils.CheckCompatibleSet(scoreInfoWithModArray.Mods, out var invalid))
        {
            var invalidModsStr = string.Join(',', invalid.Select(mod => mod.Acronym));
            _logger.LogDebug("[pp-calc] invalid mod combination requested: {invalidModsStr}", invalidModsStr);
            ModelState.AddModelError($"{nameof(ppInput.ScoreInfo)}.{nameof(ppInput.ScoreInfo.Mods)}", $"invalid mod combination: {invalidModsStr}");
            return ValidationProblem();
        }

        _logger.LogDebug("[pp-calc] start calculating...");
        var ppOutput = rulesetUtil.CalculatePerformance(ppInput.DiffCalcResult, scoreInfoWithModArray);
        _logger.LogDebug("[pp-calc] calculating done!");

        return ppOutput;
    }
}