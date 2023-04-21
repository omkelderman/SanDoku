using osu.Game.Rulesets;

namespace SanDoku.Util;

public class CustomRulesetStore : RulesetStore
{
    public CustomRulesetStore(IEnumerable<RulesetInfo> availableRulesets)
    {
        AvailableRulesets = availableRulesets;
    }

    public override IEnumerable<RulesetInfo> AvailableRulesets { get; }
}