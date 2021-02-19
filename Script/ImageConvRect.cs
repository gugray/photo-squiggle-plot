using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Script
{
    class ImageConvRect : IDisposable
    {
        Bitmap img = null;

        public ImageConvRect(string fnImg)
        {
            using (FileStream fs = new FileStream(fnImg, FileMode.Open, FileAccess.Read))
            {
                img = new Bitmap(fs);
            }
        }


        public void SaveResizedToHeight(int height, string fn)
        {
            double factor = (double)height / img.Height;
            SaveResized(factor, fn);
        }


        public void SaveResized(double factor, string fn)
        {
            using (var result = MakeResized(factor))
            {
                result.Save(fn, ImageFormat.Bmp);
            }
        }

        public Bitmap MakeResized(double factor)
        {
            int w = (int)(img.Width * factor);
            int h = (int)(img.Height * factor);
            Bitmap result = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, w, h);
            }
            return result;
        }

        public int[,] GetMonoVals(int height, int maxBright)
        {
            double factor = (double)height / img.Height;
            using (var bmp = MakeResized(factor))
            {
                int[,] res = new int[bmp.Width, bmp.Height];
                for (int x = 0; x < bmp.Width; ++x)
                {
                    for (int y = 0; y < bmp.Height; ++y)
                    {
                        var px = bmp.GetPixel(x, y);
                        double b = px.R + px.G + px.B;
                        b = (double)maxBright / (255 * 3) * b;
                        res[x, y] = (int)b;
                    }
                }
                return res;
            }
        }

        public SvgBuilder DrawZigzag(int lineCount)
        {
            SvgBuilder svg = new SvgBuilder();
            int[,] vals = GetMonoVals(lineCount, 10);
            double ypad = 25;
            double xpad = 25;
            double adv = 900.0 / lineCount;
            for (int y = 0; y < lineCount; ++y)
            {
                StringBuilder sb = new StringBuilder();
                double ybase = ypad + y * adv;
                sb.Append(string.Format("M {0:0.00} {1:0.00}", xpad, ybase));
                for (int x = 0; x < vals.GetLength(0); ++x)
                {
                    double xbase = xpad + x * adv;
                    int val = vals[x, y];
                    if (val == 10) sb.Append(string.Format(" L {0:0.00} {1:0.00}", xbase + adv, ybase));
                    else
                    {
                        double ydev = (10 - val) * adv / 17;
                        sb.Append(string.Format(" L {0:0.00} {1:0.00}", xbase + adv/8, ybase - ydev));
                        sb.Append(string.Format(" L {0:0.00} {1:0.00}", xbase + 3 * adv/8, ybase + ydev));
                        sb.Append(string.Format(" L {0:0.00} {1:0.00}", xbase + 5 * adv/8, ybase - ydev));
                        sb.Append(string.Format(" L {0:0.00} {1:0.00}", xbase + 7 * adv/8, ybase + ydev));
                        sb.Append(string.Format(" L {0:0.00} {1:0.00}", xbase + adv, ybase));
                    }
                }
                svg.Path(sb.ToString());
            }
            return svg;
        }

        public void Dispose()
        {
            if (img != null) img.Dispose();
        }
    }
}
