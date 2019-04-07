using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ColinChang.FFmpegHelper
{
    public static class FFmpegHelper
    {
        public static async Task<bool> ScreenshotAsync(string input, string output, TimeSpan? timeOffset = null)
        {
            var beforeOutput = new Dictionary<string, string> { ["-vframes"] = "1" };
            if (timeOffset != null)
                beforeOutput["-ss"] = timeOffset.ToString();
            return await ExecuteFfmpegAsync(input, output, null, beforeOutput);
        }


        public static async Task<bool> ScreenshotAsync(string input, string outputDirectory, string filenamePrefix,
            int interval,
            TimeSpan? duration = null,
            ImageFormat format = ImageFormat.JPG
        )
        {
            if (interval <= 0)
                throw new ArgumentException("timer value must greater than 0");

            var beforeOutput = new Dictionary<string, string> { ["-vf"] = $"fps=1/{interval}" };
            if (duration != null)
                beforeOutput["-t"] = duration.ToString();
            return await ExecuteFfmpegAsync(input,
                Path.Combine(outputDirectory, $"{filenamePrefix}%d.{format.ToString().ToLower()}"), null,
                beforeOutput);
        }

        /// <summary>
        /// Execute ffmpeg command
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="beforeInput">parameters before input</param>
        /// <param name="beforeOutput">parameters before output</param>
        /// <returns>whether it executed successfully</returns>
        /// <exception cref="ArgumentException">invalid arguments</exception>
        public static Task<bool> ExecuteFfmpegAsync(string input, string output,
            Dictionary<string, string> beforeInput = null,
            Dictionary<string, string> beforeOutput = null)
        {
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(output))
                throw new ArgumentException("input or output cannot be null or empty");

            var inputParameters = beforeInput == null || !beforeInput.Any()
                ? "$$"
                : $"\"{string.Join(" ", beforeInput.Select(kv => $"{kv.Key} {kv.Value}"))}\"";
            var outputParameters = beforeOutput == null || !beforeOutput.Any()
                ? "$$"
                : $"\"{string.Join(" ", beforeOutput.Select(kv => $"{kv.Key} {kv.Value}"))}\"";
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return Task.Run(() => ShellHelper.ShellHelper.Execute("ffmpeg.bat",
                    $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg_v4.1.1", Environment.Is64BitOperatingSystem ? "win64" : "win32")} {inputParameters} {input} {outputParameters} {output}",
                    true));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (!Environment.Is64BitOperatingSystem)
                    throw new NotSupportedException("only 64bit macOS is supported");

                return Task.Run(() => ShellHelper.ShellHelper.Execute("ffmpeg.sh",
                    $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg_v4.1.1", "macos64")} {inputParameters} {input} {outputParameters} {output}",
                    true));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                throw new NotSupportedException(
                    "linux is not supported yet,you can edit this library to support it by yourself");
            }
            else
            {
                throw new NotSupportedException($"unknown OS Platform {RuntimeInformation.OSDescription}");
            }
        }
    }

    public enum ImageFormat
    {
        JPG,
        PNG,
        BMP
    }
}