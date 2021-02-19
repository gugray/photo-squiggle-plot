using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Script
{
    class SvgBuilder
    {
        const string strokeColor = "#331800";
        const string strokeWidth = "0.7";

        const string skeleton =
           "<?xml version='1.0'?>\n" +
           "<svg width='135mm' height='95mm'\n" +
           "     viewBox='0 0 1350 950' preserveAspectRatio='none'\n" +
           "     xmlns='http://www.w3.org/2000/svg'>\n" +
           "  {0}\n" +
           "</svg>";

        readonly StringBuilder sb = new StringBuilder();

        const string rect = "<rect x='{0}' y='{1}' width='{2}' height='{3}' fill='none' stroke='{4}' stroke-width='{5}' />\n";

        public void Rect(int x, int y, int width, int height)
        {
            sb.Append(string.Format(rect, x, y, width, height, strokeColor, strokeWidth));
        }

        const string line = "<line x1='{0}' y1='{1}' x2='{2}' y2='{3}' stroke='{4}' stroke-width='{5}' />\n";

        public void Line(int x1, int y1, int x2, int y2)
        {
            sb.Append(string.Format(line, x1, y1, x2, y2, strokeColor, strokeWidth));
        }

        const string circle = "<circle cx='{0}' cy='{1}' r='{2}' stroke='{3}' stroke-width='{4}' fill='none' />\n";

        public void Circle(int cx, int cy, int r)
        {
            sb.Append(string.Format(circle, cx, cy, r, strokeColor, strokeWidth));
        }

        const string path = "<path d='{0}' fill='none' stroke='{1}' stroke-width='{2}' />\n";

        public void Path(string d)
        {
            sb.Append(string.Format(path, d, strokeColor, strokeWidth));
        }

        public void WriteDirect(string what)
        {
            sb.Append(what);
        }

        public string GetSvg()
        {
            return string.Format(skeleton, sb.ToString());
        }

        public void SaveToFile(string fn)
        {
            File.WriteAllText(fn, GetSvg());
        }

    }
}
