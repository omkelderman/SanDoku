using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using osu.Game.Beatmaps.Formats;
using SanDoku.Util;

namespace SanDoku
{
    public class Program
    {
        public static void Main(string[] args)
        {
            LegacyDifficultyCalculatorBeatmapDecoder.Register();
            RulesetUtil.GetForLegacyGameMode(LegacyGameMode.Osu);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
