using System;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ColinChang.FFmpegHelper.Sample
{
    class Program
    {
        static RtspHelper rtsp = new RtspHelper("rtsp://admin:12345qwert@192.168.0.109:554/h264/ch1/main/av_stream");

        static void Main(string[] args)
        {
            RecordAsync();
            //WatermarkAsync();
            //ScreenshotAsync();
            Console.ReadKey();
        }

        private static async void RecordAsync()
        {
            var output = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? @"C:\Temp\output.mp4"
                : "/Users/zhangcheng/Desktop/output.mp4";

            await rtsp.Record2VideoFileAsync(
                output,
                TimeSpan.FromSeconds(2),
                null,
                Transport.Tcp
            );
        }

        private static async void WatermarkAsync()
        {
            //rtsp.Watermark = new Watermark("logo.png", 30, 30);
            rtsp.Watermark = new Watermark("../../logo.png", 30, 30, Color.Black, 0.5f, 0.5f);
            await rtsp.Record2VideoFileAsync("/Users/zhangcheng/Desktop/output.mkv", TimeSpan.FromSeconds(2));
        }

        private static async void ScreenshotAsync()
        {
            //await rtsp.ScreenshotAsync("/Users/zhangcheng/Desktop/ss.png");

            await rtsp.ScreenshotAsync("/Users/zhangcheng/Desktop/", "colin_", 2, TimeSpan.FromSeconds(6));
        }
    }
}