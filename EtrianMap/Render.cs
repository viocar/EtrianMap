using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;
using System.Drawing.Imaging;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace EtrianMap
{
    public partial class EtrianMap : Form
    {
        public class FiveMap
        {
            //All the default tile colours.
            public static Color BACKGROUND_TILE = Color.FromArgb(255, 249, 216);
            public static Color GREEN_TILE = Color.FromArgb(81, 191, 145);
            public static Color BLUE_TILE = Color.FromArgb(76, 147, 197);
            public static Color PURPLE_TILE = Color.FromArgb(173, 130, 175);
            public static Color GREY_TILE = Color.FromArgb(185, 181, 157);
            public static Color YELLOW_TILE = Color.FromArgb(251, 231, 62);
            public static Color ORANGE_TILE = Color.FromArgb(242, 158, 105);
            public static Color PINK_TILE = Color.FromArgb(239, 163, 154);
            public static Color RED_TILE = Color.FromArgb(199, 93, 97);
            public static Color THIN_LINE = Color.FromArgb(246, 221, 172);
            public static Color THICK_LINE = Color.FromArgb(218, 201, 150);
            //The default tile colours while in fog. Probably won't use this.
            public static Color FOG_BACKGROUND_TILE = Color.FromArgb(225, 211, 171);
            public static Color FOG_GREEN_TILE = Color.FromArgb(71, 162, 116);
            public static Color FOG_BLUE_TILE = Color.FromArgb(67, 125, 160);
            public static Color FOG_PURPLE_TILE = Color.FromArgb(153, 110, 140);
            public static Color FOG_GREY_TILE = Color.FromArgb(163, 153, 122);
            public static Color FOG_YELLOW_TILE = Color.FromArgb(221, 196, 50);
            public static Color FOG_ORANGE_TILE = Color.FromArgb(214, 134, 84);
            public static Color FOG_PINK_TILE = Color.FromArgb(211, 138, 126);
            public static Color FOG_RED_TILE = Color.FromArgb(176, 79, 78);
            public static Color FOG_THIN_LINE = Color.FromArgb(207, 174, 121);
            public static Color FOG_THICK_LINE = Color.FromArgb(200, 175, 122);
            //Element sizes.
            public const int DIST_FROM_TOP = 16;
            public const int DIST_FROM_LEFT = 16;
            public const int BOX_WIDTH = 25;
            public const int BOX_HEIGHT = 25;
            public const int LINE_THICKNESS = 2;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            //Render a sample grid for testing purposes.
            if (program_flags.sample_renderer_enabled)
            {
                SolidBrush Brush = new SolidBrush(FiveMap.BACKGROUND_TILE);
                //int current_column = 0;
                //int current_row = 0;
                for (int x = 0; x < 30; x++)
                {
                    for (int y = 0; y < 35; y++)
                    {
                        e.Graphics.FillRectangle(Brush, new Rectangle(
                            FiveMap.DIST_FROM_LEFT + (y * (FiveMap.BOX_WIDTH + FiveMap.LINE_THICKNESS)), 
                            FiveMap.DIST_FROM_TOP + (x * (FiveMap.BOX_HEIGHT + FiveMap.LINE_THICKNESS)), 
                            FiveMap.BOX_WIDTH, 
                            FiveMap.BOX_HEIGHT));
                    }
                }
            }

        }
    }
}