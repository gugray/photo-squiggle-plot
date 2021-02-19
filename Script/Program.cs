using System;
using System.Collections.Generic;
using System.IO;

namespace Script
{
    class Program
    {
        static void photoSawtooth(string name)
        {
            string fnImgIn = Path.Combine("sources", name + ".jpg");
            string fnImgSvg = Path.Combine("output", name + "-zigzag.svg");
            using (var ic = new ImageConvRect(fnImgIn))
            {
                var svg = ic.DrawZigzag(75);
                svg.SaveToFile(fnImgSvg);
            }
        }

        static void photoSpiral(string name)
        {
            string fnImgIn = Path.Combine("sources", name + ".jpg");
            string fnImgSvg = Path.Combine("output", name + "-spiral.svg");
            using (var ic = new ImageConvSpiral(fnImgIn))
            {
                var svg = ic.DrawSpiral(37);
                svg.SaveToFile(fnImgSvg);
            }
        }

        static void renderBackside(string name)
        {
            var lines = new List<string>();
            using (var sr = new StreamReader(Path.Combine("sources", name + ".txt")))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                    lines.Add(line);
            }
            var font = new SvgFont(Path.Combine("sources", "fonts_EMS_EMSReadability.svg"));
            SvgBuilder svg = new SvgBuilder();
            //svg.Line(0, 25, 25, 25);
            //svg.Line(25, 0, 25, 25);
            //svg.Line(1325, 25, 1350, 25);
            //svg.Line(1325, 0, 1325, 25);
            //svg.Line(0, 925, 25, 925);
            //svg.Line(25, 950, 25, 925);
            //svg.Line(1325, 925, 1350, 925);
            //svg.Line(1325, 950, 1325, 925);
            // Render recipient address
            font.Write(svg, 770, 420, 60, lines[5]);
            font.Write(svg, 770, 520, 60, lines[6]);
            font.Write(svg, 770, 620, 60, lines[7]);
            font.Write(svg, 770, 720, 60, lines[8]);
            // Render sender address
            font.Write(svg, 30, 30, 30, lines[0]);
            font.Write(svg, 30, 70, 30, lines[1]);
            font.Write(svg, 30, 110, 30, lines[2]);
            font.Write(svg, 30, 150, 30, lines[3]);
            // Render description
            for (int i = 0; i < lines.Count - 10; ++i)
            {
                font.Write(svg, 660, 300 + i * 40, 30, lines[i + 10], true);
            }
            // Save SVG
            svg.SaveToFile(Path.Combine("output", name + ".svg"));
        }

        static void Main(string[] args)
        {
            photoSawtooth("leguin");
            photoSpiral("leguin");
            photoSawtooth("ella");
            photoSpiral("borges");
            photoSpiral("falk");
            renderBackside("card-sample");
        }
    }
}
