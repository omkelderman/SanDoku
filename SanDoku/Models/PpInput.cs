using SanDoku.Util;
using System.ComponentModel.DataAnnotations;

namespace SanDoku.Models;

public record PpInput
(
    LegacyGameMode GameMode,
    [Required] DiffCalcResult DiffCalcResult,
    [Required] ScoreInfo ScoreInfo
);