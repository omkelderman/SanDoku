using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Skinning;
using System.IO;

namespace SanDoku.Util
{
    public class ProcessorWorkingBeatmap : WorkingBeatmap
    {
        private readonly IBeatmap _beatmap;

        public ProcessorWorkingBeatmap(IBeatmap beatmap) : base(beatmap.BeatmapInfo, null)
        {
            _beatmap = beatmap;
        }

        protected override IBeatmap GetBeatmap() => _beatmap;
        protected override Texture GetBackground() => null;
        protected override Track GetBeatmapTrack() => null;
        protected override ISkin GetSkin() => null;
        public override Stream GetStream(string storagePath) => null;
    }
}