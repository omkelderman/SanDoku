using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Utils;
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
        /// 
        /// </summary>
        /// <param name="beatmap">The contents of an .osu file, must be Content-Type of "text/osu", optionally supports Content-Encoding "gzip", "deflate" and "br"</param>
        /// <param name="mode">Override game mode</param>
        /// <param name="mods">Optionally provide mods</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        [HttpPost]
        [SuppressModelStateInvalidFilter]
        [Consumes(OsuInputFormatter.ContentType)]
        public async Task<ActionResult<DiffCalcResult>> Calc([FromBody] Beatmap beatmap, [FromQuery] LegacyGameMode? mode = null,
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
            var diff = await rulesetUtil.CalculateDifficultyAttributes(workingBeatmap, filtered, ct);
            _logger.LogDebug($"[{beatmapInfoStr}] processing done!");

            return Ok(diff);
        }
    }
}