using System;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ColinChang.FFmpegHelper.Sample
{
    class Program
    {
        static RtspHelper rtsp =
            new RtspHelper("rtsp://admin:12345qwert@192.168.0.109:554/h264/ch1/main/av_stream", 3000);

        static async Task Main(string[] args)
        {
            //await RecordAsync();
            //await WatermarkAsync();
            //await ScreenshotAsync();

            //await ConvertToAsync();
            await ExtractVideoAsync();
            // await ReplaceBackgroundColorAsync();

            Console.WriteLine("All done");
            Console.ReadKey();
        }

        private static async Task RecordAsync()
        {
            var output = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\Temp\output.mp4"
                : "/Users/colin/Downloads/output.mp4";

            //await rtsp.Record2VideoFileAsync(output, TimeSpan.FromSeconds(2));

            await rtsp.Record2VideoFileAsync(
                output,
                TimeSpan.FromSeconds(2),
                null,
                Transport.Tcp
            );
        }

        private static async Task WatermarkAsync()
        {
            var output = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\Temp\output.mkv"
                : "/Users/colin/Downloads/output.mkv";

            //var watermark = new Watermark("logo.png", 30, 30);
            var watermark = new Watermark("../../logo.png", 30, 30, Color.Black, 0.5f, 0.5f);
            await rtsp.Record2VideoFileAsync(output, TimeSpan.FromSeconds(2), watermark: watermark);
        }

        private static async Task ScreenshotAsync()
        {
            var output = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\Temp\"
                : "/Users/colin/Downloads/";


            //await rtsp.ScreenshotAsync($"{output}ss.png");

            try
            {
                await rtsp.ScreenshotAsync(output, "colin_", 2, TimeSpan.FromSeconds(10));
            }
            catch (Exception e)
            {
                Console.WriteLine("*************************************************************");
                Console.WriteLine(e.Message);
            }
        }

        private static async Task ConvertToAsync()
        {
            var input = "/Users/colin/Downloads/macos64/src.mp4";
            var output = "/Users/colin/Downloads/macos64/src-1.mp4";

            await FFmpegHelper.ConvertToAsync(input, output, TimeSpan.FromSeconds(5));
        }

        private static async Task ExtractVideoAsync()
        {
            var input = "/Users/colin/Downloads/input.mov";
            var output = "/Users/colin/Downloads/output.mov";

            await FFmpegHelper.ExtractVideoAsync(input, output, TimeSpan.Parse("00:01:00"), TimeSpan.Parse("00:01:30"));
        }

        private static async Task ReplaceBackgroundColorAsync()
        {
            var input = "/Users/colin/Downloads/macos64/src.mp4";
            var output = "/Users/colin/Downloads/macos64/src-3.mp4";
            await FFmpegHelper.ReplaceBackgroundAsync(input, output, "70de77", 0.1f, 0.2f, "000000", 640, 480,
                TimeSpan.FromSeconds(10));
        }

        private static async Task ReplaceBackgroundAsync()
        {
            var input = "/Users/colin/Downloads/macos64/src.mp4";
            var output = "/Users/colin/Downloads/macos64/src-3.mp4";
            var background = "/Users/colin/Downloads/macos64/bg.mp4";

            await FFmpegHelper.ReplaceBackgroundAsync(input, output, "70de77", 0.1f, 0.2f, background,
                TimeSpan.FromSeconds(10));
        }
    }
}