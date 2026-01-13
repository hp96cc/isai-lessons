using FFMpegCore;
using System.Drawing;


namespace ISAI.Lessons.WhsiperAIDemo
{
    public  class VideoFrameService
    {
        public async Task<string> ExtractFrameAsync(string videoPath, int second, string outputFolder)
        {
            // Ensure output directory exists
            Directory.CreateDirectory(outputFolder);
            string outputPath = Path.Combine(outputFolder, $"frame_at_{second}s.jpg");

            // FFMpegCore handles the seek (-ss) and frame extraction (-vframes 1) automatically
            await FFMpeg.SnapshotAsync(videoPath, outputPath, new Size(1920, 1080), TimeSpan.FromSeconds(second));

            return outputPath;
        }
    }
}
