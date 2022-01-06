using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Utils;
using SanDoku.Models;
using SanDoku.Util;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
        public ActionResult<DiffResult> CalcDiff([FromBody] Beatmap beatmap, [FromQuery] LegacyGameMode? mode = null,
            [FromQuery] LegacyMods mods = LegacyMods.None, CancellationToken ct = default)
        {
            if (beatmap == null) return BadRequest();
            var beatmapInfoStr = beatmap.BeatmapInfo.ToString();

            LegacyGameMode modeToPick;
            if (mode.HasValue)
            {
                modeToPick = mode.Value;
                if (!Enum.IsDefined(modeToPick))
                {
                    _logger.LogDebug($"Invalid game mode requested {modeToPick} for map {beatmapInfoStr}");
                    return BadRequest($"invalid game mode: {modeToPick}");
                }
            }
            else
            {
                modeToPick = (LegacyGameMode) beatmap.BeatmapInfo.RulesetID;
            }


            var rulesetUtil = RulesetUtil.GetForLegacyGameMode(modeToPick);
            var filtered = rulesetUtil.ConvertFromLegacyModsFilteredByDifficultyAffecting(mods).ToList();

            if (!ModUtils.CheckCompatibleSet(filtered, out var invalid))
            {
                var invalidModsStr = string.Join(',', invalid.Select(mod => mod.Acronym));
                _logger.LogDebug($"Invalid mod combination requested {invalidModsStr} for map {beatmapInfoStr}");
                return BadRequest($"invalid mod combination: {invalidModsStr}");
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
        public ActionResult<PpOutput> CalcPp([FromBody] PpInput ppInput)
        {
            if (ppInput?.ScoreInfo == null || ppInput.DiffCalcResult == null) return BadRequest();

            if (!Enum.IsDefined(ppInput.GameMode))
            {
                _logger.LogDebug($"Invalid game mode requested {ppInput.GameMode} for map pp calc");
                return BadRequest($"invalid game mode: {ppInput.GameMode}");
            }

            var rulesetUtil = RulesetUtil.GetForLegacyGameMode(ppInput.GameMode);
            var (pp, categoryDifficulty) = rulesetUtil.CalculatePerformance(ppInput.DiffCalcResult, ppInput.ScoreInfo);

            var output = new PpOutput
            {
                Pp = pp,
                CategoryDifficulty = categoryDifficulty
            };
            return Ok(output);
        }
    }
}