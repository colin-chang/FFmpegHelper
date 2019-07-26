using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ColinChang.FFmpegHelper
{
    public static class FFmpegHelper
    {
        /// <summary>
        /// Screenshot once
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="timeOffset">set the start time offset</param>
        /// <param name="timeout">count by millisecond</param>
        /// <returns></returns>
        public static async Task<bool> ScreenshotAsync(string input, string output, TimeSpan? timeOffset = null,
            int timeout = -1)
        {
            var beforeInput = timeout > 0 ? new Dictionary<string, string> {["-stimeout"] = $"{timeout * 1000}"} : null;
            var beforeOutput = new Dictionary<string, string> {["-vframes"] = "1"};
            if (timeOffset != null)
                beforeOutput["-ss"] = timeOffset.ToString();
            return await ExecuteFfmpegAsync(input, output, beforeInput, beforeOutput);
        }

        /// <summary>
        /// Screenshot by a timer
        /// </summary>
        /// <param name="input"></param>
        /// <param name="outputDirectory"></param>
        /// <param name="filenamePrefix">filename prefix of screenshot picture</param>
        /// <param name="interval">how often(seconds) to exec a screenshot.</param>
        /// <param name="duration">how long time will this run</param>
        /// <param name="timeout">count by millisecond</param>
        /// <param name="format">screenshot picture format</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">timer value isn't greater less than 0</exception>
        public static async Task<bool> ScreenshotAsync(string input, string outputDirectory, string filenamePrefix,
            int interval,
            TimeSpan? duration = null, int timeout = -1,
            ImageFormat format = ImageFormat.JPG
        )
        {
            if (interval <= 0)
                throw new ArgumentException("timer value must be greater than 0");

            var beforeInput = timeout > 0 ? new Dictionary<string, string> {["-stimeout"] = $"{timeout * 1000}"} : null;

            var beforeOutput = new Dictionary<string, string> {["-vf"] = $"fps=1/{interval}"};
            if (duration != null)
                beforeOutput["-t"] = duration.ToString();
            return await ExecuteFfmpegAsync(input,
                Path.Combine(outputDirectory, $"{filenamePrefix}%d.{format.ToString().ToLower()}"), beforeInput,
                beforeOutput);
        }

        /// <summary>
        /// Watermark a video
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="watermark"></param>
        /// <param name="beforeInput"></param>
        /// <param name="beforeOutput"></param>
        /// <returns></returns>
        public static async Task<bool> WatermarkAsync(string input, string output, Watermark watermark,
            Dictionary<string, string> beforeInput = null,
            Dictionary<string, string> beforeOutput = null)
        {
            if (watermark != null)
            {
                beforeOutput = beforeOutput ?? new Dictionary<string, string>();
                beforeOutput["-vf"] = watermark.Color == Color.Empty
                    ? $"'movie={watermark.Picture}[wm]; [in][wm]overlay={watermark.X}:{watermark.Y}[out]'"
                    : $"'movie={watermark.Picture},colorkey=0x{watermark.Color.ToArgb().ToString("X").Substring(2)}:{watermark.Similarity}:{watermark.Blend} [wm]; [in][wm]overlay={watermark.X}:{watermark.Y}[out]'";
            }

            return await ExecuteFfmpegAsync(input, output, beforeInput, beforeOutput);
        }


        /// <summary>
        /// Convert video format
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="duration">how long time of the output video</param>
        /// <returns></returns>
        public static async Task<bool> ConvertToAsync(string input, string output, TimeSpan? duration)
        {
            var beforeOutput = duration != null ? new Dictionary<string, string> {["-t"] = duration.ToString()} : null;
            return await ExecuteFfmpegAsync(input, output, null, beforeOutput);
        }

        /// <summary>
        /// Extract a segment from a video
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="from">start timestamp</param>
        /// <param name="to">end timestamp</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<bool> ExtractVideoAsync(string input, string output, TimeSpan from, TimeSpan to)
        {
            if (from >= to)
                throw new ArgumentException("start time(from) must be less than end time(to).");

            var beforeOutput = new Dictionary<string, string>
            {
                ["-ss"] = from.ToString(),
                ["-c"] = "copy",
                ["-to"] = to.ToString()
            };
            return await ExecuteFfmpegAsync(input, output, null, beforeOutput);
        }

        /// <summary>
        /// Replace the background of input file and overlay it on top of a static background color.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="color">The color(hexadecimal) which will be replaced with transparency.</param>
        /// <param name="similarity">Similarity percentage with the key color.0.01 matches only the exact key color, while 1.0 matches everything.</param>
        /// <param name="blend">Blend percentage.0.0 makes pixels either fully transparent, or not transparent at all.Higher values result in semi-transparent pixels, with a higher transparency the more similar the pixels color is to the key color.</param>
        /// <param name="backgroundColor"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="duration">how long time of the output video</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<bool> ReplaceBackgroundAsync(string input, string output, string color, float similarity,
            float blend, string backgroundColor, int width, int height, TimeSpan? duration = null)
        {
            if (string.IsNullOrWhiteSpace(color))
                throw new ArgumentException("color cannot be null or empty");
            if (!Regex.IsMatch(color, @"[\d,a-f,A-F]{6}"))
                throw new ArgumentException("color is invalid");

            if (similarity < 0)
                similarity = 0;
            else if (similarity > 1)
                similarity = 1.0f;
            else
                similarity = (float) Math.Round(similarity, 2);

            if (blend < 0)
                blend = 0;
            else if (blend > 1)
                blend = 1.0f;
            else
                blend = (float) Math.Round(blend, 2);

            if (string.IsNullOrWhiteSpace(backgroundColor))
                throw new ArgumentException("Background video cannot be null or empty");

            var beforeInput = new Dictionary<string, string>
            {
                ["-f"] = "lavfi",
                ["-i"] = $"color=c={backgroundColor}:s={width}x{height}"
            };
            var beforeOutput = new Dictionary<string, string>
            {
                ["-shortest -filter_complex"] =
                    $"'[1:v]chromakey=0x{color}:{similarity}:{blend}[ckout];[0:v][ckout]overlay[out]'",
                ["-map"] = "'[out]'"
            };
            if (duration != null)
                beforeOutput["-t"] = duration.ToString();

            return await ExecuteFfmpegAsync(input, output, beforeInput, beforeOutput);
        }

        /// <summary>
        /// Replace the background of input file and overlay it on top of the background file.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="color">The color(hexadecimal) which will be replaced with transparency.</param>
        /// <param name="similarity">Similarity percentage with the key color.0.01 matches only the exact key color, while 1.0 matches everything.</param>
        /// <param name="blend">Blend percentage.0.0 makes pixels either fully transparent, or not transparent at all.Higher values result in semi-transparent pixels, with a higher transparency the more similar the pixels color is to the key color.</param>
        /// <param name="background">background video or picture</param>
        /// <param name="duration">how long time of the output video</param>
        /// <returns></returns>
        public static async Task<bool> ReplaceBackgroundAsync(string input, string output, string color, float similarity,
            float blend, string background, TimeSpan? duration = null)
        {
            if (string.IsNullOrWhiteSpace(color))
                throw new ArgumentException("color cannot be null or empty");
            if (!Regex.IsMatch(color, @"[\d,A-F]{6}"))
                throw new ArgumentException("color is invalid");

            if (similarity < 0)
                similarity = 0;
            else if (similarity > 1)
                similarity = 1.0f;
            else
                similarity = (float) Math.Round(similarity, 2);

            if (blend < 0)
                blend = 0;
            else if (blend > 1)
                blend = 1.0f;
            else
                blend = (float) Math.Round(blend, 2);

            if (string.IsNullOrWhiteSpace(background))
                throw new ArgumentException("Background video cannot be null or empty");

            var beforeInput = new Dictionary<string, string> {["-i"] = background};
            var beforeOutput = new Dictionary<string, string>
            {
                ["-shortest -filter_complex"] =
                    $"'[1:v]chromakey=0x{color}:{similarity}:{blend}[ckout];[0:v][ckout]overlay[out]'",
                ["-map"] = "'[out]'"
            };
            if (duration != null)
                beforeOutput["-t"] = duration.ToString();

            return await ExecuteFfmpegAsync(input, output, beforeInput, beforeOutput);
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
                return Task.Run(() => ShellHelper.ShellHelper.ExecuteFile("ffmpeg.bat",
                    $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg_v4.1.4", Environment.Is64BitOperatingSystem ? "win64" : "win32")} {inputParameters} {input} {outputParameters} {output}",
                    true));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (!Environment.Is64BitOperatingSystem)
                    throw new NotSupportedException("only 64bit macOS is supported");

                return Task.Run(() => ShellHelper.ShellHelper.ExecuteFile("ffmpeg.sh",
                    $"{Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg_v4.1.4", "macos64")} {inputParameters} {input} {outputParameters} {output}",
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
}