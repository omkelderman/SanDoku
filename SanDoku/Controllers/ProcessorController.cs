using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Utils;
using SanDoku.Extensions;
using SanDoku.Models;
using SanDoku.Util;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;

namespace SanDoku.Controllers
{
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
        /// <param name="beatmap">The contents of an .osu file, must be the correct Content-Type, optionally supports Content-Encoding "gzip", "deflate" and "br"</param>
        /// <param name="mode">Override game mode</param>
        /// <param name="mods">Optionally provide mods</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost("diff")]
        [SuppressModelStateInvalidFilter]
        [Consumes(OsuInputFormatter.ContentType, OsuInputFormatter.WrongButLegacyContentType)]
        [ProducesResponseType(typeof(DiffResult), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), (int) HttpStatusCode.BadRequest)]
        public ActionResult<DiffResult> CalcDiff([FromBody] Beatmap beatmap, [FromQuery] LegacyGameMode? mode = null,
            [FromQuery] LegacyMods mods = LegacyMods.None, CancellationToken ct = default)
        {
            ModelState.RemoveKeysExcept(nameof(mode), nameof(mods));
            string beatmapInfoStr;
            if (beatmap == null)
            {
                ModelState.AddModelError(nameof(beatmap), "Empty input not valid");
                beatmapInfoStr = "Unknown";
            }
            else
            {
                beatmapInfoStr = beatmap.ToString();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogDebug($"[{beatmapInfoStr}] validation errors: {ModelState.ToErrorMessage()}");
                return ValidationProblem();
            }

            // IntelliSense doesn't know, but if beatmap was null, then the ModelState would be invalid
            // and we would have stopped the request already
            Debug.Assert(beatmap != null);

            var modeToPick = mode ?? (LegacyGameMode) beatmap.BeatmapInfo.RulesetID;
            var rulesetUtil = RulesetUtil.GetForLegacyGameMode(modeToPick);
            var filtered = rulesetUtil.ConvertFromLegacyModsFilteredByDifficultyAffecting(mods).ToList();

            if (!ModUtils.CheckCompatibleSet(filtered, out var invalid))
            {
                var invalidModsStr = string.Join(',', invalid.Select(mod => mod.Acronym));
                _logger.LogDebug($"Invalid mod combination requested {invalidModsStr} for map {beatmapInfoStr}");
                ModelState.AddModelError(nameof(mods), $"invalid mod combination: {invalidModsStr}");
                return ValidationProblem();
            }

            rulesetUtil.AddRulesetInfoToBeatmapInfo(beatmap.BeatmapInfo);

            var workingBeatmap = new ProcessorWorkingBeatmap(beatmap);

            _logger.LogDebug($"[{beatmapInfoStr}] start processing...");
            var (diffCalcResult, beatmapGameMode, gameModeUsed, modsUsed) = rulesetUtil.CalculateDifficultyAttributes(workingBeatmap, filtered, ct);
            _logger.LogDebug($"[{beatmapInfoStr}] processing done!");

            var result = new DiffResult
            {
                BeatmapGameMode = beatmapGameMode,
                GameModeUsed = gameModeUsed,
                ModsUsed = modsUsed,
                DiffCalcResult = diffCalcResult
            };
            return Ok(result);
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
                _logger.LogDebug($"[pp-calc] validation errors: {ModelState.ToErrorMessage()}");
                return ValidationProblem();
            }

            var rulesetUtil = RulesetUtil.GetForLegacyGameMode(ppInput.GameMode);

            // check if mods are illegal
            var mods = rulesetUtil.ConvertFromLegacyMods(ppInput.ScoreInfo.Mods);
            if (!ModUtils.CheckCompatibleSet(mods, out var invalid))
            {
                var invalidModsStr = string.Join(',', invalid.Select(mod => mod.Acronym));
                _logger.LogDebug($"[pp-calc] invalid mod combination requested: {invalidModsStr}");
                ModelState.AddModelError($"{nameof(ppInput.ScoreInfo)}.{nameof(ppInput.ScoreInfo.Mods)}", $"invalid mod combination: {invalidModsStr}");
                return ValidationProblem();
            }


            _logger.LogDebug("[pp-calc] start calculating...");
            var (pp, categoryDifficulty) = rulesetUtil.CalculatePerformance(ppInput.DiffCalcResult, ppInput.ScoreInfo);
            _logger.LogDebug("[pp-calc] calculating done!");

            var output = new PpOutput
            {
                Pp = pp,
                CategoryDifficulty = categoryDifficulty
            };
            return Ok(output);
        }
    }
}