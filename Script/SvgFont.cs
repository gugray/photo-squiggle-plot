using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;

namespace Script
{
    class SvgFont
    {
        class Segment
        {
            public readonly List<double[]> Points = new List<double[]>();
        }

        class Glyph
        {
            public readonly List<Segment> Segments = new List<Segment>();
            public double Adv;
        }

        readonly double Ascent;
        readonly double Descent;
        readonly Dictionary<char, Glyph> glyphs = new Dictionary<char, Glyph>();

        public SvgFont(string fn)
        {
            var doc = new XmlDocument();
            doc.Load(fn);
            var elmFace = doc.GetElementsByTagName("font-face")[0];
            Ascent = double.Parse(elmFace.Attributes["ascent"].Value);
            Descent = double.Parse(elmFace.Attributes["descent"].Value);
            var elmGlyphs = doc.GetElementsByTagName("glyph");
            for (int i = 0; i < elmGlyphs.Count; ++i)
            {
                var elmGlyph = elmGlyphs[i];
                Glyph glyph = new Glyph
                {
                    Adv = double.Parse(elmGlyph.Attributes["horiz-adv-x"].Value),
                };
                char c = elmGlyph.Attributes["unicode"].Value[0];
                if (elmGlyph.Attributes["d"] == null)
                {
                    glyphs[c] = glyph;
                    continue;
                }
                string path = elmGlyph.Attributes["d"].Value.Trim();
                while (true)
                {
                    int len = path.Length;
                    path = path.Replace("  ", " ");
                    if (path.Length == len) break;
                }
                int ixM = path.IndexOf('M');
                while (ixM < path.Length)
                {
                    int nextIxM = path.IndexOf('M', ixM + 1);
                    if (nextIxM == -1) nextIxM = path.Length;
                    string segStr = path.Substring(ixM, nextIxM - ixM).Trim();
                    ixM = nextIxM;
                    string[] parts = segStr.Split(' ');
                    Segment seg = new Segment();
                    for (int j = 0; j < parts.Length; j += 3)
                    {
                        if (parts[j] != "M" && parts[j] != "L")
                        {
                            //Console.WriteLine("Skipped glyph because path contains non-straight-line: " +
                            //    elmGlyph.Attributes["glyph-name"].Value);
                            goto GlyphBarf;
                        }
                        double x = double.Parse(parts[j + 1]);
                        double y = double.Parse(parts[j + 2]);
                        seg.Points.Add(new double[] { x, y });
                    }
                    glyph.Segments.Add(seg);
                }
                glyphs[c] = glyph;
            GlyphBarf:;
            }
        }

        /// <summary>
        /// Writes text at the specified coordinates.
        /// (X,Y) is top left of rendered text's bounding box, or top righ if righ-aligned.
        /// Size is height of bounding box.
        /// </summary>
        public void Write(SvgBuilder svg, int x, int y, int size, string text, bool alignRight = false)
        {
            if (text.Length == 0) return;
            double factor = size / (Ascent - Descent);
            StringBuilder sb = new StringBuilder();
            int i = alignRight ? text.Length - 1 : 0;
            while (true)
            {
                char c = text[i];
                var glyph = glyphs.ContainsKey(c) ? glyphs[c] : glyphs['?'];
                sb.Clear();
                foreach (var seg in glyph.Segments)
                {
                    for (int j = 0; j < seg.Points.Count; ++j)
                    {
                        if (j == 0) sb.Append(" M ");
                        else sb.Append(" L ");
                        int cx = x;
                        if (alignRight) cx -= (int)Math.Round(glyph.Adv * factor);
                        sb.Append((cx + (seg.Points[j][0] * factor)).ToString("0.##"));
                        sb.Append(' ');
                        sb.Append((y + size - ((seg.Points[j][1] - Descent) * factor)).ToString("0.##"));
                    }
                }
                if (sb.Length != 0) svg.Path(sb.ToString());
                if (alignRight)
                {
                    x -= (int)Math.Round(glyph.Adv * factor);
                    --i;
                    if (i < 0) break;
                }
                else
                {
                    x += (int)Math.Round(glyph.Adv * factor);
                    ++i;
                    if (i == text.Length) break;
                }
            }
        }
    }
}
