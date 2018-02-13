using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace eCademy.NUh16.PhotoShare
{
    public class ImageHelper {
        public static string GetContentType(string filename)
        {
            switch (Path.GetExtension(filename))
            {
                case ".jpg":
                    return "image/jpeg";
                case ".png":
                    return "image/png";
                default:
                    return "image/jpeg";
            }
        }
        public static byte[] ResizeImage(byte[] bytes, int? size)
        {
            if (!size.HasValue)
            {
                return bytes;
            }
            var image = Image.FromStream(new MemoryStream(bytes));
            int height;
            int width;
            if (image.Width > image.Height)
            {
                var ratio = (double)image.Height / (double)image.Width;
                height = size.Value;
                width = (int)((double)size / ratio);
            }
            else
            {
                var ratio = (double)image.Width / (double)image.Height;
                width = size.Value;
                height = (int)((double)size / ratio);
            }

            var bitmap = ResizeImage(image, width, height);
            var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Jpeg);

            return stream.ToArray();
        }

        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}