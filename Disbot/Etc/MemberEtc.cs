using Disbot.Models;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Disbot.Etc
{
    public static class MemberEtc
    {
        public static string GetLevelupAvatar(string url, uint level)
        {
            return GetLevelupAvatar(url, (int)level);
        }
        public static string GetLevelupAvatar(string url, int level)
        {
            var temp = Path.GetTempFileName().Replace("tmp", "jpg");
            using (var webClient = new WebClient())
            {
                var bytes = webClient.DownloadData(url);
                using var stream = new MemoryStream(bytes);
                using var avatar = Resize(Image.FromStream(stream), 128, 128);//Resize(Image.FromFile(tempFile), 256, 256);
                using (var g = Graphics.FromImage(avatar))
                {
                    using var font = new Font(FontFamily.GenericMonospace, 12, FontStyle.Bold);
                    using var descFont = new Font(FontFamily.GenericMonospace, 8, FontStyle.Regular);
                    var text = $"Level {level}";
                    string levelDesc = GetLevelDesc(level);
                    var measure = g.MeasureString(text, font);
                    var descMeasure = g.MeasureString(levelDesc, descFont);
                    var point = new Point((int)((avatar.Width - measure.Width) / 2), (int)(avatar.Height * 0.72));
                    var descPoint = new Point((int)((avatar.Width - descMeasure.Width) / 2), (int)(avatar.Height * 0.85));
                    g.FillRectangle(Brushes.Black, new Rectangle(new Point(0, (int)(avatar.Height * 0.7)), new Size(avatar.Width, (int)(avatar.Height * 0.35))));
                    g.DrawString(text, font, Brushes.Gold, point);
                    g.DrawString(levelDesc, descFont, Brushes.White, descPoint);
                }
                avatar.Save(temp);
                return temp;
            }

        }
        private static string GetLevelDesc(int level)
        {
            return GetLevelDesc((uint)level);
        }
        private static string GetLevelDesc(uint level)
        {
            return level switch
            {
                var x when x < 10 => "'เด็กฝึกงาน'",
                var x when x < 20 => "'พนักงานประจำ'",
                var x when x < 30 => "'ผู้จัดการ'",
                var x when x < 40 => "'ผู้อำนวยการ'",
                _ => "'เจ้าของบริษัท'"
            };
        }

        private static Image Resize(Image img, int outputWidth, int outputHeight)
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
