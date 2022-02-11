using System.Collections.Generic;

namespace SanDoku.Models
{
    public class PpOutput
    {
        public double? Pp { get; set; }
        public Dictionary<string, double?> ExtraValues { get; set; }
    }
}