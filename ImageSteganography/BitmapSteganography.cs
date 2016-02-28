using System;
using System.Drawing;
using System.IO;

namespace ImageSteganography
{
    public class BitmapSteganography
    {
        private const int MaxMessageLength = 0x00FFFFFF - 1;
        protected Stream KeyStream;

        public BitmapSteganography(Stream keyStream)
        {
            if (keyStream == null)
                throw new ArgumentNullException(nameof(keyStream), "Please specify key stream!");

            KeyStream = keyStream;
        }

        public Bitmap HideMessage(Bitmap srcBitmap, Stream messageStream)
        {
            if (srcBitmap == null)
                throw new ArgumentNullException(nameof(srcBitmap), "Please specify source bitmap!");
            if (messageStream == null)
                throw new ArgumentNullException(nameof(messageStream), "Please specify message stream!");
            if (messageStream.Length >= MaxMessageLength)
                throw new ArgumentException($"Message too long, maximum {MaxMessageLength} bytes allowed!");

            var countImagePixels = srcBitmap.Width * srcBitmap.Height - 1;
            var countRequiredPixels = 1 + messageStream.Length * 3;

            if (countRequiredPixels > countImagePixels)
                throw new ArgumentException($"Input image is too small to hide message! You need {countRequiredPixels} pixels image!");

            var destBitmap = new Bitmap(srcBitmap);

            var firstPixelValue = (int)messageStream.Length;
            var r = (firstPixelValue & 0x00FF0000) >> 16;
            var g = (firstPixelValue & 0x0000FF00) >> 8;
            var b = firstPixelValue & 0x000000FF;

            destBitmap.SetPixel(0, 0, Color.FromArgb(r, g, b));

            KeyStream.Seek(0, SeekOrigin.Begin);
            messageStream.Seek(0, SeekOrigin.Begin);
            int curImageX = 1, curImageY = 0;

            for (var messageIndex = 0; messageIndex < messageStream.Length; messageIndex++)
            {
                if (KeyStream.Position == KeyStream.Length)
                    KeyStream.Seek(0, SeekOrigin.Begin);

                var currentByte = messageStream.ReadByte() ^ KeyStream.ReadByte();

                var currentPixel = srcBitmap.GetPixel(curImageX, curImageY);
                destBitmap.SetPixel(curImageX, curImageY, SetBitBasedColor(currentPixel, (byte)(currentByte & 0x01),
                                                                                         (byte)((currentByte & 0x02) >> 1),
                                                                                         (byte)((currentByte & 0x04) >> 2)));
                IncrementImageCoords(srcBitmap, ref curImageX, ref curImageY);

                currentPixel = srcBitmap.GetPixel(curImageX, curImageY);
                destBitmap.SetPixel(curImageX, curImageY, SetBitBasedColor(currentPixel, (byte)((currentByte & 0x08) >> 3),
                                                                                         (byte)((currentByte & 0x10) >> 4),
                                                                                         (byte)((currentByte & 0x20) >> 5)));
                IncrementImageCoords(srcBitmap, ref curImageX, ref curImageY);

                currentPixel = srcBitmap.GetPixel(curImageX, curImageY);
                destBitmap.SetPixel(curImageX, curImageY, SetBitBasedColor(currentPixel, (byte)((currentByte & 0x40) >> 6),
                                                                                         (byte)((currentByte & 0x80) >> 7),
                                                                                         0));
                IncrementImageCoords(srcBitmap, ref curImageX, ref curImageY);
            }

            return destBitmap;
        }

        public Stream DiscoverMessage(Bitmap srcBitmap)
        {
            if (srcBitmap == null)
                throw new ArgumentNullException(nameof(srcBitmap), "Please specify source bitmap!");

            var destStream = new MemoryStream();

            var firstPixel = srcBitmap.GetPixel(0, 0);
            var messageLength = (firstPixel.R << 16) + (firstPixel.G << 8) + firstPixel.B;

            KeyStream.Seek(0, SeekOrigin.Begin);
            int curImageX = 1, curImageY = 0;

            for (var messageIndex = 0; messageIndex < messageLength; messageIndex++)
            {
                if (KeyStream.Position == KeyStream.Length)
                    KeyStream.Seek(0, SeekOrigin.Begin);

                var first = srcBitmap.GetPixel(curImageX, curImageY);
                IncrementImageCoords(srcBitmap, ref curImageX, ref curImageY);

                var second = srcBitmap.GetPixel(curImageX, curImageY);
                IncrementImageCoords(srcBitmap, ref curImageX, ref curImageY);

                var third = srcBitmap.GetPixel(curImageX, curImageY);
                IncrementImageCoords(srcBitmap, ref curImageX, ref curImageY);

                byte streamValue = 0;
                streamValue |= (byte)(first.R & 0x01);
                streamValue |= (byte)((first.G & 0x01) << 1);
                streamValue |= (byte)((first.B & 0x01) << 2);
                streamValue |= (byte)((second.R & 0x01) << 3);
                streamValue |= (byte)((second.G & 0x01) << 4);
                streamValue |= (byte)((second.B & 0x01) << 5);
                streamValue |= (byte)((third.R & 0x01) << 6);
                streamValue |= (byte)((third.G & 0x01) << 7);

                destStream.WriteByte((byte)(KeyStream.ReadByte() ^ streamValue));
            }
            destStream.Seek(0, SeekOrigin.Begin);

            return destStream;
        }

        protected Color SetBitBasedColor(Color baseColor, byte rbit, byte gbit, byte bbit)
        {
            var red = baseColor.R;
            var green = baseColor.G;
            var blue = baseColor.B;

            if (0 == rbit)
                red &= 0xFE;
            else
                red |= 0x01;

            if (0 == gbit)
                green &= 0xFE;
            else
                green |= 0x01;

            if (0 == bbit)
                blue &= 0xFE;
            else
                blue |= 0x01;

            return Color.FromArgb(red, green, blue);
        }

        protected void IncrementImageCoords(Bitmap bmp, ref int x, ref int y)
        {
            x++;

            if (x >= bmp.Width)
            {
                x = 0;
                y++;
            }
        }
    }
}