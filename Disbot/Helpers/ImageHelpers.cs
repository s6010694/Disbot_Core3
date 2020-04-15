using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Disbot.Helpers
{
    internal static class ImageHelpers
    {
        internal static Image Resize(Image img, int outputWidth, int outputHeight)
        {

            if (img == null || (img.Width == outputWidth && img.Height == outputHeight)) return img;
            Bitmap outputImage;
            Graphics graphics;
            try
            {
                outputImage = new Bitmap(outputWidth, outputHeight, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
                graphics = Graphics.FromImage(outputImage);
                graphics.DrawImage(img, new Rectangle(0, 0, outputWidth, outputHeight), new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
                return outputImage;
            }
            catch
            {
                throw;
            }
        }
    }
}
