using System;
using System.Drawing;

namespace ColinChang.FFmpegHelper
{
    public class Watermark
    {
        /// <summary>
        /// JPG,PNG,BMP are supported
        /// </summary>
        public string Picture { get; set; }

        public float X { get; set; }

        public float Y { get; set; }

        public Color Color { get; set; }

        private float _similarity;

        /// <summary>
        /// Similarity 
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Similarity value out of range [0~1]</exception>
        public float Similarity
        {
            get => _similarity;
            set
            {
                if (_similarity < 0 || _similarity > 1)
                    throw new ArgumentOutOfRangeException($"Similarity should be between 0 and 1");
                _similarity = value;
            }
        }

        private float _blend;

        /// <summary>
        /// Blend
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Blend value out of range [0~1]</exception>
        public float Blend
        {
            get => _blend;
            set
            {
                if (_blend < 0 || _blend > 1)
                    throw new ArgumentOutOfRangeException($"Blend should be between 0 and 1");
                _blend = value;
            }
        }

        public Watermark(string picture, float x = 0, float y = 0) : this(picture, x, y, Color.Empty, 0, 0)
        {
        }

        public Watermark(string picture, float x, float y, Color color, float similarity, float blend)
        {
            Picture = picture;
            X = x;
            Y = y;
            Color = color;
            Similarity = similarity;
            Blend = blend;
        }
    }
}