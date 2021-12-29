using SanDoku.Util;

namespace SanDoku.Models
{
    public class PpInput
    {
        public LegacyGameMode GameMode { get; set; }
        public DiffCalcResult DiffCalcResult { get; set; }
        public ScoreInfo ScoreInfo { get; set; }
    }
}