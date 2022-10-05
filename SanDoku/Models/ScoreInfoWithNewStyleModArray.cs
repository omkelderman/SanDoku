using osu.Game.Rulesets.Mods;

namespace SanDoku.Models;

public record ScoreInfoWithNewStyleModArray
(
    Mod[] Mods,
    ScoreInfo ScoreInfo
);