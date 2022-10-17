using osu.Game.Beatmaps.Formats;
using osu.Game.Rulesets;
using SanDoku.Util;

namespace SanDoku;

public class Program
{
    public static void Main(string[] args)
    {
        // make very old beatmap files work
        LegacyDifficultyCalculatorBeatmapDecoder.Register();
        
        // explicitly set the RulesetStore so we don't get a warning later on about it
        Decoder.RegisterDependencies(new CustomRulesetStore(RulesetUtil.GetAllAvailableRulesetInfos()));

        // run app
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}

public class CustomRulesetStore : RulesetStore
{
    public CustomRulesetStore(IEnumerable<RulesetInfo> availableRulesets)
    {
        AvailableRulesets = availableRulesets;
    }

    public override IEnumerable<RulesetInfo> AvailableRulesets { get; }
}