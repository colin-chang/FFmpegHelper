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

        static void Main(string[] args)
        {
//            RecordAsync();
            WatermarkAsync().Wait();
//            ScreenshotAsync().Wait();
        }

        private static async Task RecordAsync()
        {
            var output = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\Temp\output.mp4"
                : "/Users/zhangcheng/Desktop/output.mp4";

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
                : "/Users/zhangcheng/Desktop/output.mkv";

            //rtsp.Watermark = new Watermark("logo.png", 30, 30);
            rtsp.Watermark = new Watermark("../../logo.png", 30, 30, Color.Black, 0.5f, 0.5f);
            await rtsp.Record2VideoFileAsync(output, TimeSpan.FromSeconds(2));
        }

        private static async Task ScreenshotAsync()
        {
            var output = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\Temp\"
                : "/Users/zhangcheng/Desktop/";


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
    }
}