using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ColinChang.FFmpegHelper
{
    public class RtspHelper
    {
        public string Rtsp { get; set; }

        /// <summary>
        /// set time for rtsp connection,count by millisecond
        /// </summary>
        public int Timeout { get; set; } = -1;

        public RtspHelper(string rtsp)
        {
            if (!rtsp.Trim().ToLower().StartsWith("rtsp://"))
                throw new ArgumentException("invalid rtsp address");
            Rtsp = rtsp;
        }


        public RtspHelper(string rtsp, int timeout) : this(rtsp)
        {
            Timeout = timeout;
        }

        /// <summary>
        /// Record a video file from a specify rtsp address
        /// </summary>
        /// <param name="outputFile">output file</param>
        /// <param name="duration">record duration seconds of audio/video</param>
        /// <param name="userAgent">User-Agent header</param>
        /// <param name="transport">set RTSP transport protocols</param>
        /// <param name="allowedMediaTypes">set media types to accept from the server</param>
        /// <param name="watermark">watermark for the output video</param>
        /// <returns>whether record successfully</returns>
        public async Task<bool> Record2VideoFileAsync(
            string outputFile,
            TimeSpan duration,
            string userAgent = null,
            Transport transport = Transport.Udp, AllowedMediaTypes allowedMediaTypes = AllowedMediaTypes.All,
            Watermark watermark = null)
        {
            var beforeInput = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(userAgent))
                beforeInput["-user-agent"] = userAgent;
            if (transport != Transport.Udp)
                beforeInput["-rtsp_transport"] = transport.ToString().ToLower();
            if (allowedMediaTypes != AllowedMediaTypes.All)
                beforeInput["-allowed_media_types"] = allowedMediaTypes.ToString().ToLower();
            if (Timeout > 0)
                beforeInput["-stimeout"] = $"{Timeout * 1000}";

            var beforeOutput = new Dictionary<string, string> {["-t"] = duration.ToString()};

            return await FFmpegHelper.WatermarkVideo(Rtsp, outputFile, watermark, beforeInput, beforeOutput);
        }

        /// <summary>
        /// Screenshot once
        /// </summary>
        /// <param name="outputFile"></param>
        /// <returns></returns>
        public async Task<bool> ScreenshotAsync(string outputFile) =>
            await FFmpegHelper.ScreenshotAsync(Rtsp, outputFile, timeout: Timeout);

        /// <summary>
        /// Screenshot by a timer
        /// </summary>
        /// <param name="outputDirectory"></param>
        /// <param name="filenamePrefix">filename prefix of screenshot picture</param>
        /// <param name="interval">how often(seconds) to exec a screenshot.</param>
        /// <param name="duration">how long time will this run</param>
        /// <param name="format">screenshot picture format</param>
        /// <returns></returns>
        public async Task<bool> ScreenshotAsync(string outputDirectory, string filenamePrefix,
            int interval, TimeSpan duration,
            ImageFormat format = ImageFormat.JPG
        ) =>
            await FFmpegHelper.ScreenshotAsync(Rtsp, outputDirectory, filenamePrefix, interval, duration, Timeout,
                format);
    }

    public enum Transport
    {
        Udp,
        Tcp,
        Udp_Multicast,
        Http
    }

    public enum AllowedMediaTypes
    {
        All,
        Video,
        Audio,
        Data,
        Subtitle,
    }
}