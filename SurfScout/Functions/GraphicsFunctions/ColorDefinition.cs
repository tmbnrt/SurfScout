using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace SurfScout.Functions.GraphicsFunctions
{
    public static class ColorDefinition
    {
        public static Color GetColorForWindspeed(double windSpeed)
        {
            if (windSpeed < 6)
                return Color.FromArgb(255, 255, 255);
            if (windSpeed < 7)
                return Color.FromArgb(221, 253, 252);
            if (windSpeed < 8)
                return Color.FromArgb(180, 251, 248);
            if (windSpeed < 9)
                return Color.FromArgb(126, 248, 243);
            if (windSpeed < 10)
                return Color.FromArgb(108, 247, 241);
            if (windSpeed < 11)
                return Color.FromArgb(86, 248, 201);
            if (windSpeed < 12)
                return Color.FromArgb(66, 250, 154);
            if (windSpeed < 13)
                return Color.FromArgb(31, 253, 73);
            if (windSpeed < 14)
                return Color.FromArgb(20, 253, 47);
            if (windSpeed < 15)
                return Color.FromArgb(25, 254, 0);
            if (windSpeed < 16)
                return Color.FromArgb(83, 250, 0);
            if (windSpeed < 17)
                return Color.FromArgb(132, 247, 0);
            if (windSpeed < 18)
                return Color.FromArgb(157, 246, 0);
            if (windSpeed < 19)
                return Color.FromArgb(233, 241, 0);
            if (windSpeed < 20)
                return Color.FromArgb(255, 239, 0);
            if (windSpeed < 21)
                return Color.FromArgb(255, 197, 10);
            if (windSpeed < 22)
                return Color.FromArgb(255, 171, 16);
            if (windSpeed < 23)
                return Color.FromArgb(255, 126, 26);
            if (windSpeed < 24)
                return Color.FromArgb(255, 119, 28);
            if (windSpeed < 25)
                return Color.FromArgb(255, 85, 36);
            if (windSpeed < 26)
                return Color.FromArgb(255, 48, 53);
            if (windSpeed < 27)
                return Color.FromArgb(255, 43, 73);
            if (windSpeed < 28)
                return Color.FromArgb(255, 38, 93);
            if (windSpeed < 29)
                return Color.FromArgb(255, 30, 121);
            if (windSpeed < 30)
                return Color.FromArgb(255, 25, 140);
            if (windSpeed < 31)
                return Color.FromArgb(255, 21, 156);
            if (windSpeed < 32)
                return Color.FromArgb(255, 16, 178);
            if (windSpeed < 33)
                return Color.FromArgb(255, 10, 198);
            if (windSpeed < 34)
                return Color.FromArgb(255, 8, 211);
            if (windSpeed < 35)
                return Color.FromArgb(255, 6, 220);
            if (windSpeed < 36)
                return Color.FromArgb(255, 4, 231);
            if (windSpeed < 37)
                return Color.FromArgb(255, 3, 241);

            return Color.FromArgb(182, 102, 210);

            //if (windSpeed < 3)
            //    return Color.White;
            //else if (windSpeed < 8)
            //    return Color.Blue;
            //else if (windSpeed < 15)
            //    return Color.LightBlue;
            //else if (windSpeed < 20)
            //    return Color.Cyan;
            //else if (windSpeed < 23)
            //    return Color.Green;
            //else if (windSpeed < 26)
            //    return Color.Yellow;
            //else if (windSpeed < 32)
            //    return Color.Orange;
            //else if (windSpeed < 42)
            //    return Color.Red;
            //else
            //    return Color.Purple;
        }
    }
}
