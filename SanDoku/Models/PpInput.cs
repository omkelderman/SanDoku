using System.ComponentModel.DataAnnotations;
using SanDoku.Util;

namespace SanDoku.Models
{
    public class PpInput
    {
        public LegacyGameMode GameMode { get; set; }

        [Required]
        public DiffCalcResult DiffCalcResult { get; set; }

        [Required]
        public ScoreInfo ScoreInfo { get; set; }
    }
}