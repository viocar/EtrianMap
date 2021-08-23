using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Collections;
using System.Diagnostics;
using System.Drawing.Imaging;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace EtrianMap
{
    public partial class EtrianMap : Form
    {
        public static Bitmap ICON_MAP_1 = new Bitmap(Bitmap.FromFile(Directory.GetParent(System.Environment.CurrentDirectory).Parent.FullName + "\\Graphics\\Graphics1.png"));
        public static Bitmap ICON_MAP_2 = new Bitmap(Bitmap.FromFile(Directory.GetParent(System.Environment.CurrentDirectory).Parent.FullName + "\\Graphics\\Graphics2.png"));
        public class MapRender
        {
            public const int TOP_EDGE = 16;
            public const int LEFT_EDGE = 16;
            public const int BOX_WIDTH = 19;
            public const int BOX_HEIGHT = 19;
            public const int LINE_THICKNESS = 2;
            public class Colours
            {
                //All the default tile colours.
                public static Color BACKGROUND_TILE = Color.FromArgb(255, 249, 216);
                public static Color GREEN_TILE = Color.FromArgb(81, 191, 145);
                public static Color BLUE_TILE = Color.FromArgb(76, 147, 197);
                public static Color PURPLE_TILE = Color.FromArgb(173, 130, 175);
                public static Color GREY_TILE = Color.FromArgb(185, 181, 157);
                public static Color YELLOW_TILE = Color.FromArgb(251, 231, 62);
                public static Color ORANGE_TILE = Color.FromArgb(242, 158, 105);
                public static Color PINK_TILE = Color.FromArgb(239, 163, 154); //EO5 only.
                public static Color RED_TILE = Color.FromArgb(199, 93, 97);
                public static Color LIGHT_GREEN_TILE = Color.FromArgb(172, 234, 170); //EON only.
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
                public static Color FOG_PINK_TILE = Color.FromArgb(211, 138, 126); //EO5 only.
                public static Color FOG_RED_TILE = Color.FromArgb(176, 79, 78);
                public static Color FOG_LIGHT_GREEN_TILE = Color.FromArgb(152, 199, 140); //EON only.
                public static Color FOG_THIN_LINE = Color.FromArgb(207, 174, 121); //The lines are somewhat gradiented in-game, so I've picked approximations.
                public static Color FOG_THICK_LINE = Color.FromArgb(200, 175, 122);
                public static Color SELECTION_BOX = Color.FromArgb(255, 0, 242); //Not an EO colour.
            }
            public class Icons
            {
                private static Bitmap BitmapCrop(int bitmap_id, Rectangle rect)
                {
                    Bitmap use = ICON_MAP_1; //A little weird but it works.
                    if (bitmap_id == 2)
                    {
                        use = ICON_MAP_2;
                    }
                    Bitmap crop = new Bitmap(rect.Width, rect.Height);
                    using (Graphics g = Graphics.FromImage(crop))
                    {
                        g.DrawImage(use, -rect.X, -rect.Y);
                        return crop;
                    }
                }
                public class Graphics1 //Honestly, I should have pre-cropped them instead of writing code to do it in software, but, eh. That would have been even more tedious than this.
                {
                    public const int ICON_WIDTH = 19;
                    public const int ICON_HEIGHT = 19;
                    public static Bitmap DOOR = BitmapCrop(1, new Rectangle(0, 0, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap DOOR_LOCKED = BitmapCrop(1, new Rectangle(21, 0, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap STAIRS_UP = BitmapCrop(1, new Rectangle(42, 0, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap STAIRS_DOWN = BitmapCrop(1, new Rectangle(63, 0, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap SIGNPOST = BitmapCrop(1, new Rectangle(84, 0, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ARROW_LEFT = BitmapCrop(1, new Rectangle(105, 0, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap STAIRS_UP_ORANGE = BitmapCrop(1, new Rectangle(126, 0, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap STAIRS_DOWN_ORANGE = BitmapCrop(1, new Rectangle(147, 0, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap TAKE = BitmapCrop(1, new Rectangle(0, 21, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap CHOP = BitmapCrop(1, new Rectangle(21, 21, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap MINE = BitmapCrop(1, new Rectangle(42, 21, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap FISH = BitmapCrop(1, new Rectangle(63, 21, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap CLASSIC_GATHER = BitmapCrop(1, new Rectangle(84, 21, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ARROW_RIGHT = BitmapCrop(1, new Rectangle(105, 21, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap STAIRS_UP_GREY = BitmapCrop(1, new Rectangle(126, 21, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap STAIRS_DOWN_GREY = BitmapCrop(1, new Rectangle(147, 21, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap CHEST_CLOSED = BitmapCrop(1, new Rectangle(0, 42, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap NOTE = BitmapCrop(1, new Rectangle(21, 42, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap EVENT_TILE = BitmapCrop(1, new Rectangle(42, 42, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap EXCLAMATION_POINT = BitmapCrop(1, new Rectangle(63, 42, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap QUESTION_MARK = BitmapCrop(1, new Rectangle(84, 42, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ARROW_DOWN = BitmapCrop(1, new Rectangle(105, 42, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap FLAG_DARK_ORANGE = BitmapCrop(1, new Rectangle(126, 42, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap FLAG_LIGHT_ORANGE = BitmapCrop(1, new Rectangle(147, 42, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ARROW_UP_DOWN = BitmapCrop(1, new Rectangle(0, 63, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ARROW_LEFT_RIGHT = BitmapCrop(1, new Rectangle(21, 63, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap BRICK = BitmapCrop(1, new Rectangle(42, 63, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap GOLEM_STATUE = BitmapCrop(1, new Rectangle(63, 63, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap GRAVE = BitmapCrop(1, new Rectangle(84, 63, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ARROW_UP = BitmapCrop(1, new Rectangle(105, 63, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap FOUNTAIN = BitmapCrop(1, new Rectangle(126, 63, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap SQUARE_RED = BitmapCrop(1, new Rectangle(147, 63, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap CRYSTAL = BitmapCrop(1, new Rectangle(0, 84, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap SWITCH_UP = BitmapCrop(1, new Rectangle(21, 84, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap CIRCLE_GREY = BitmapCrop(1, new Rectangle(42, 84, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap BLANK_1 = BitmapCrop(1, new Rectangle(63, 84, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap TARGET_RETICLE = BitmapCrop(1, new Rectangle(84, 84, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap CHEST_OPEN = BitmapCrop(1, new Rectangle(105, 84, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap BLANK_2 = BitmapCrop(1, new Rectangle(126, 84, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap SQUARE_BROWN = BitmapCrop(1, new Rectangle(147, 84, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap FOE = BitmapCrop(1, new Rectangle(0, 105, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ICON_ONE = BitmapCrop(1, new Rectangle(21, 105, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ICON_TWO = BitmapCrop(1, new Rectangle(42, 105, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ICON_THREE = BitmapCrop(1, new Rectangle(63, 105, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap TAKE_USED = BitmapCrop(1, new Rectangle(84, 105, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap CHOP_USED = BitmapCrop(1, new Rectangle(105, 105, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap MINE_USED = BitmapCrop(1, new Rectangle(126, 105, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap FISH_USED = BitmapCrop(1, new Rectangle(147, 105, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ICON_FOUR = BitmapCrop(1, new Rectangle(0, 126, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ICON_FIVE = BitmapCrop(1, new Rectangle(21, 126, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ICON_SIX = BitmapCrop(1, new Rectangle(42, 126, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ICON_SEVEN = BitmapCrop(1, new Rectangle(63, 126, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ARROW_UP_YELLOW = BitmapCrop(1, new Rectangle(84, 126, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ARROW_DOWN_YELLOW = BitmapCrop(1, new Rectangle(105, 126, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ARROW_UP_DOWN_YELLOW = BitmapCrop(1, new Rectangle(126, 126, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap BLANK_3 = BitmapCrop(1, new Rectangle(147, 126, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ICON_EIGHT = BitmapCrop(1, new Rectangle(0, 147, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ICON_NINE = BitmapCrop(1, new Rectangle(21, 147, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ICON_ZERO = BitmapCrop(1, new Rectangle(42, 147, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap CAMPFIRE = BitmapCrop(1, new Rectangle(63, 147, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ARROW_LEFT_YELLOW = BitmapCrop(1, new Rectangle(84, 147, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ARROW_RIGHT_YELLOW = BitmapCrop(1, new Rectangle(105, 147, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ARROW_LEFT_RIGHT_YELLOW = BitmapCrop(1, new Rectangle(126, 147, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap FOOD = BitmapCrop(1, new Rectangle(147, 147, ICON_WIDTH, ICON_HEIGHT));
                    //From here on, the icons fall off the grid a bit
                    public static Bitmap GEOMAGNETIC_POLE = BitmapCrop(1, new Rectangle(116, 175, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap FOOD_USED = BitmapCrop(1, new Rectangle(147, 175, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap CUTTER_LOG = BitmapCrop(1, new Rectangle(126, 210, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap LADDER = BitmapCrop(1, new Rectangle(147, 210, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap PITFALL = BitmapCrop(1, new Rectangle(168, 210, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap CUTTER_LOG_BROKEN = BitmapCrop(1, new Rectangle(189, 210, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap SWITCH_DOWN = BitmapCrop(1, new Rectangle(126, 231, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap BUTTON_DOWN = BitmapCrop(1, new Rectangle(147, 231, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap BUTTON_UP = BitmapCrop(1, new Rectangle(168, 231, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap FIRE_SCALE = BitmapCrop(1, new Rectangle(189, 231, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap FIRE_SCALE_BROKEN = BitmapCrop(1, new Rectangle(210, 231, ICON_WIDTH, ICON_HEIGHT));
                }
                public class Graphics2
                {
                    public static Bitmap ICON_MAP = new Bitmap(Bitmap.FromFile("Graphics\\Graphics2.png"));
                    public const int ICON_WIDTH = 19;
                    public const int ICON_HEIGHT = 19;
                    public static Bitmap BLOCK_BLUE = BitmapCrop(2, new Rectangle(137, 1, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ROCK_ORANGE = BitmapCrop(2, new Rectangle(158, 1, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ROCK_GREY = BitmapCrop(2, new Rectangle(179, 1, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap CRYSTAL = BitmapCrop(2, new Rectangle(200, 1, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap PLATFORM_VINE_COVER = BitmapCrop(2, new Rectangle(66, 213, ICON_WIDTH, ICON_HEIGHT)); //Get better name.
                    public static Bitmap PLATFORM_UP = BitmapCrop(2, new Rectangle(87, 213, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap ROCK_BLUE = BitmapCrop(2, new Rectangle(108, 213, ICON_WIDTH, ICON_HEIGHT)); //Used for ICE but I'm keeping the naming consistent.
                    public static Bitmap SLEEPING_ROCK_MONSTER = BitmapCrop(2, new Rectangle(129, 213, ICON_WIDTH, ICON_HEIGHT)); //Get better name.
                    public static Bitmap PLATFORM_DOWN = BitmapCrop(2, new Rectangle(87, 234, ICON_WIDTH, ICON_HEIGHT));
                    public static Bitmap FLOATING_PLATFORM = BitmapCrop(2, new Rectangle(108, 234, ICON_WIDTH, ICON_HEIGHT));
                }
            }
            public class DrawingElements
            {
                public class Brushes
                {
                    public static SolidBrush BACKGROUND = new SolidBrush(Colours.BACKGROUND_TILE);
                    public static SolidBrush FLOOR = new SolidBrush(Colours.GREEN_TILE);
                    public static SolidBrush DAMAGE = new SolidBrush(Colours.RED_TILE);
                    public static SolidBrush ICE = new SolidBrush(Colours.GREY_TILE);
                    public static SolidBrush MUD = new SolidBrush(Colours.ORANGE_TILE);
                    public static SolidBrush HIGH_FLOOR = new SolidBrush(Colours.LIGHT_GREEN_TILE);
                }
                public class Pens
                {
                    public static Pen THIN_LINE = new Pen(Colours.THIN_LINE, LINE_THICKNESS);
                    public static Pen THICK_LINE = new Pen(Colours.THICK_LINE, LINE_THICKNESS);
                    public static Pen SELECTION = new Pen(Colours.SELECTION_BOX, LINE_THICKNESS + 1);
                }
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (globals.open_map > - 1)
            {
                System.Diagnostics.Stopwatch timer = new Stopwatch();
                timer.Start();
                for (int x = 0; x < globals.map_data.header.map_x; x++) //Draw boxes.
                {
                    for (int y = 0; y < globals.map_data.header.map_y; y++)
                    {
                        SolidBrush this_brush = MapRender.DrawingElements.Brushes.BACKGROUND;
                        Bitmap this_bitmap = MapRender.Icons.Graphics1.BLANK_1;
                        bool draw_bitmap = false; //No sense in drawing a blank if we don't have to.
                        Rectangle pos = new Rectangle
                        (
                            MapRender.LEFT_EDGE + (x * (MapRender.BOX_WIDTH + MapRender.LINE_THICKNESS)),
                            MapRender.TOP_EDGE + (y * (MapRender.BOX_HEIGHT + MapRender.LINE_THICKNESS)),
                            MapRender.BOX_WIDTH,
                            MapRender.BOX_HEIGHT
                        );
                        switch (globals.map_data.tile_types[0][x + (y * globals.map_data.header.map_x)].type)
                        {
                            case 0x1:
                                this_brush = MapRender.DrawingElements.Brushes.FLOOR;
                                break;
                            case 0x2:
                                this_brush = MapRender.DrawingElements.Brushes.DAMAGE;
                                break;
                            case 0x3:
                                this_brush = MapRender.DrawingElements.Brushes.ICE;
                                break;
                            case 0x9:
                                this_brush = MapRender.DrawingElements.Brushes.MUD;
                                break;
                            case 0xC:
                                this_brush = MapRender.DrawingElements.Brushes.HIGH_FLOOR;
                                break;
                            case 0xD:
                                this_bitmap = MapRender.Icons.Graphics1.DOOR;
                                draw_bitmap = true;
                                break;
                            case 0xE:
                                this_bitmap = MapRender.Icons.Graphics1.STAIRS_UP; //Fix this code when you have details on which way the stairs go.
                                draw_bitmap = true;
                                break;
                            case 0x10:
                                this_bitmap = MapRender.Icons.Graphics1.ARROW_UP_DOWN; //Same as 0xE
                                draw_bitmap = true;
                                break;
                            case 0x11:
                                this_bitmap = MapRender.Icons.Graphics1.ARROW_UP; //Same as 0xE
                                draw_bitmap = true;
                                break;
                            case 0x14:
                                this_bitmap = MapRender.Icons.Graphics1.GEOMAGNETIC_POLE;
                                draw_bitmap = true;
                                break;
                            default:
                                break;
                        }
                        e.Graphics.FillRectangle(this_brush, pos);
                        if (draw_bitmap)
                        {
                            e.Graphics.DrawImage(this_bitmap, pos.X, pos.Y);
                        }
                    }
                }
                for (int x = 0; x < globals.map_data.header.map_x + 1; x++) //Draw vertical lines. The line code and the box code have to be separated or else it renders awkwardly due to ordering.
                {
                    Pen line = MapRender.DrawingElements.Pens.THIN_LINE;
                    if ((x + 4) % 5 == 4)
                    {
                        line = MapRender.DrawingElements.Pens.THICK_LINE;
                    }
                    e.Graphics.DrawLine(line,
                        new PointF
                        (
                            MapRender.LEFT_EDGE + (x * (MapRender.BOX_WIDTH + MapRender.LINE_THICKNESS)) - 1, //Not entirely sure why I need - 1... quirk of the Pen system?
                            MapRender.TOP_EDGE - 1
                        ),
                        new PointF
                        (
                            MapRender.LEFT_EDGE + (x * (MapRender.BOX_WIDTH + MapRender.LINE_THICKNESS)) - 1,
                            MapRender.TOP_EDGE + (globals.map_data.header.map_y * (MapRender.BOX_HEIGHT + MapRender.LINE_THICKNESS)) - 1
                        )
                    );
                }
                for (int y = 0; y < globals.map_data.header.map_y + 1; y++) //Draw horizontal lines.
                {
                    Pen line = MapRender.DrawingElements.Pens.THIN_LINE;
                    line = MapRender.DrawingElements.Pens.THIN_LINE;
                    if ((y + 4) % 5 == 4)
                    {
                        line = MapRender.DrawingElements.Pens.THICK_LINE;
                    }
                    e.Graphics.DrawLine(line,
                        new PointF
                        (
                            MapRender.LEFT_EDGE - 1,
                            MapRender.TOP_EDGE + (y * (MapRender.BOX_HEIGHT + MapRender.LINE_THICKNESS)) - 1
                        ),
                        new PointF
                        (
                            MapRender.LEFT_EDGE + (globals.map_data.header.map_x * (MapRender.BOX_WIDTH + MapRender.LINE_THICKNESS)) - 1,
                            MapRender.TOP_EDGE + (y * (MapRender.BOX_HEIGHT + MapRender.LINE_THICKNESS)) - 1
                        )
                    );
                }
                if (globals.selected_box.Count > 0)
                {
                    for (int x = 0; x < globals.selected_box.Count; x++) //There is a way to make this nicer for boxes that share an edge but I am not spending the time doing that.
                    {
                        Rectangle pos = new Rectangle
                        (
                            MapRender.LEFT_EDGE + (globals.selected_box_x[x] * (MapRender.BOX_WIDTH + MapRender.LINE_THICKNESS)),
                            MapRender.TOP_EDGE + (globals.selected_box_y[x] * (MapRender.BOX_HEIGHT + MapRender.LINE_THICKNESS)),
                            MapRender.BOX_WIDTH,
                            MapRender.BOX_HEIGHT
                        );
                        e.Graphics.DrawRectangle(MapRender.DrawingElements.Pens.SELECTION, pos);
                    }
                }
                timer.Stop();
                Debug.WriteLine("Drew screen in: " + timer.Elapsed);
            }
            else
            {
                Application.Exit(); //There is no way this is the correct place for this but I could not find anywhere else to put it where it worked. This causes the main form to flicker.
                //Find a better way to do this.
            }
        }
    }
}