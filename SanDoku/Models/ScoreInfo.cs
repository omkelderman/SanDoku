using osu.Game.Beatmaps.Legacy;

namespace SanDoku.Models;

public class ScoreInfo
{
    public LegacyMods Mods { get; set; }
    public int MaxCombo { get; set; }
    public long TotalScore { get; set; }
    public int Count50 { get; set; }
    public int Count100 { get; set; }
    public int Count300 { get; set; }
    public int CountMiss { get; set; }
    public int CountKatu { get; set; }
    public int CountGeki { get; set; }
}