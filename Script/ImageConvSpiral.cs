using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Script
{
    class ImageConvSpiral : IDisposable
    {
        Bitmap img = null;

        public ImageConvSpiral(string fnImg)
        {
            using (FileStream fs = new FileStream(fnImg, FileMode.Open, FileAccess.Read))
            {
                img = new Bitmap(fs);
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

        int[][] getMonoVals(Point[][] arcPoints, int maxBright)
        {
            double adv = 450.0 / arcPoints.Length;
            int advInt = (int)Math.Ceiling(adv);
            double advHalfSquared = (adv / 2) * (adv / 2);
            double factor = (double)900 / img.Height;
            int[][] accum = new int[arcPoints.Length][];
            int[][] counts = new int[arcPoints.Length][];
            for (int i = 0; i < arcPoints.Length; ++i)
            {
                accum[i] = new int[arcPoints[i].Length];
                counts[i] = new int[arcPoints[i].Length];
            }
            using (var bmp = MakeResized(factor))
            {
                int xOfs = (1350 - bmp.Width) / 2;
                int yOfs = (950 - bmp.Height) / 2;
                for (int i = 0; i < arcPoints.Length; ++i)
                {
                    for (int j = 0; j < arcPoints[i].Length; ++j)
                    {
                        var pt = arcPoints[i][j];
                        for (int x = pt.X - advInt; x < pt.X + advInt; ++x)
                        {
                            for (int y = pt.Y - advInt; y < pt.Y + advInt; ++y)
                            {
                                if (x - xOfs < 0 || y - yOfs < 0) continue;
                                if (x - xOfs >= bmp.Width || y - yOfs >= bmp.Height) continue;
                                var px = bmp.GetPixel(x - xOfs, y - yOfs);
                                int b = px.R + px.G + px.B;
                                double distSq = (x - pt.X) * (x - pt.X) + (y - pt.Y) * (y - pt.Y);
                                if (distSq <= advHalfSquared)
                                {
                                    accum[i][j] += b;
                                    ++counts[i][j];
                                }
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < accum.Length; ++i)
            {
                for (int j = 0; j < accum[i].Length; ++j)
                {
                    if (accum[i][j] == 0) continue;
                    double b = accum[i][j] / counts[i][j];
                    b = (double)maxBright / (255 * 3) * b;
                    accum[i][j] = (int)Math.Round(b);
                }
            }
            return accum;
        }

        public SvgBuilder DrawSpiral(int turns)
        {
            var svg = new SvgBuilder();

            Point[][] arcPoints = new Point[turns][];
            double adv = 450.0 / turns;
            double cx = 1350 / 2;
            double cy = 950 / 2;

            for (int i = 0; i < turns; ++i)
            {
                double startRad = 450 - i * adv;
                double circumference = 2 * startRad * Math.PI;
                int segCount = (int)Math.Round(circumference / adv);
                arcPoints[i] = new Point[segCount];
                for (int j = 0; j < segCount; ++j)
                {
                    // Radius (distance from spiral's center)
                    double radHere = startRad - j * (adv / segCount);
                    // Angle
                    double degHere = j * 2 * Math.PI / segCount;
                    Point pt = new Point
                    {
                        X = (int)Math.Round(cx + radHere * Math.Sin(degHere)),
                        Y = (int)Math.Round(cy - radHere * Math.Cos(degHere)),
                    };
                    arcPoints[i][j] = pt;
                }
            }
            int[][] vals = getMonoVals(arcPoints, 10);
           
            StringBuilder sb = new StringBuilder();
            sb.Append("M " + arcPoints[0][0].X + " " + arcPoints[0][0].Y);
            for (int i = 0; i < turns; ++i)
            {
                int segCount = arcPoints[i].Length;
                for (int j = 0; j < segCount; ++j)
                {
                    double dev = (10 - vals[i][j]) * adv / 17;
                    double degHere = j * 2 * Math.PI / segCount;
                    double cos = Math.Cos(degHere);
                    double sin = Math.Sin(degHere);
                    var pt = arcPoints[i][j];
                    //svg.Circle(pt.X, pt.Y, vals[i][j]);
                    sb.Append(string.Format(" L {0:0.00} {1:0.00}", pt.X + cos * adv / 8 + sin * dev, pt.Y + sin * adv / 8 - cos * dev));
                    sb.Append(string.Format(" L {0:0.00} {1:0.00}", pt.X + cos * 3 * adv / 8 - sin * dev, pt.Y + sin * 3 * adv / 8 + cos * dev));
                    sb.Append(string.Format(" L {0:0.00} {1:0.00}", pt.X + cos * 5 * adv / 8 + sin * dev, pt.Y + sin * 5 * adv / 8 - cos * dev));
                    sb.Append(string.Format(" L {0:0.00} {1:0.00}", pt.X + cos * 7 * adv / 8 - sin * dev, pt.Y + sin * 7 * adv / 8 + cos * dev));
                    sb.Append(string.Format(" L {0:0.00} {1:0.00}", pt.X + cos * adv + sin * dev, pt.Y + sin * adv - cos * dev));
                }
            }
            svg.Path(sb.ToString());

            return svg;
        }

        public void Dispose()
        {
            if (img != null) img.Dispose();
        }
    }
}
