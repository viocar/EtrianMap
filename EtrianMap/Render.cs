using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D; //For LinearGradientBrush
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
            public const int TOP_EDGE = 39; //15 + menuStrip's height of 24
            public const int LEFT_EDGE = 15;
            public const int BOX_WIDTH = 19;
            public const int BOX_HEIGHT = 19;
            public const int LINE_THICKNESS = 1;
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
                public static Color SELECTION_BOX = Color.FromArgb(127, 0, 255); //Not an EO colour.
                public static Color HIGHLIGHT_BOX = Color.FromArgb(255, 127, 0); //Not an EO colour.             
                public static byte[,] DANGER_RGB = //First palette. Didn't look good.
                { 
                    { 127, 127, 127 }, { 000, 128, 255 }, { 054, 155, 201 }, { 090, 173, 166 }, { 116, 185, 139 }, { 155, 205, 100 }, 
                    { 207, 231, 049 }, { 237, 246, 018 }, { 255, 255, 000 }, { 255, 232, 000 }, { 255, 207, 080 }, { 255, 185, 000 },
                    { 255, 139, 000 }, { 255, 112, 000 }, { 255, 074, 000 }, { 255, 049, 000 }, { 255, 000, 000 }, { 230, 000, 000 },
                    { 205, 000, 000 }, { 166, 000, 000 }, { 118, 000, 000 }, { 076, 000, 000,}, { 037, 000, 000 }, { 000, 000, 000,}
                };
                public static byte[,] DANGER_RGB2 = //Second palette. Also didn't look good.
                { 
                    { 000, 128, 255 }, { 046, 128, 255 }, { 094, 128, 255 }, { 140, 128, 255 }, { 182, 128, 255 }, { 225, 128, 255 },
                    { 255, 128, 255 }, { 255, 128, 218 }, { 255, 128, 176 }, { 255, 128, 134 }, { 255, 128, 090 }, { 255, 128, 048 },
                    { 255, 128, 008 }, { 255, 112, 000 }, { 255, 074, 000 }, { 255, 049, 000 }, { 255, 000, 000 }, { 230, 000, 000 },
                    { 205, 000, 000 }, { 166, 000, 000 }, { 118, 000, 000 }, { 076, 000, 000,}, { 037, 000, 000 }, { 000, 000, 000,}
                };
                public static byte[,] DANGER_RGB3 = //Third palette. Still doesn't look good, but it'll do.
                {
                    { 127, 127, 127 }, { 000, 255, 000 }, { 000, 255, 145 }, { 000, 225, 225 }, { 000, 154, 222 }, { 000, 068, 228 },
                    { 114, 093, 220 }, { 204, 115, 242 }, { 255, 137, 247 }, { 255, 180, 203 }, { 255, 175, 155 }, { 255, 170, 127 },
                    { 255, 162, 099 }, { 255, 141, 068 }, { 255, 110, 036 }, { 245, 091, 000 }, { 228, 073, 000 }, { 201, 018, 000 },
                    { 162, 000, 000 }, { 123, 000, 000 }, { 050, 000, 000 }, { 000, 000, 000 }
                };
                public static byte[,] GROUP_RGB =
                {
                    { 255, 128, 000 }, { 000, 128, 255 }, { 000, 255, 000 }, { 192, 128, 192 }, { 096, 096, 096 }, { 192, 192, 192 }, 
                    { 128, 000, 255 }, { 192, 255, 192 }, { 255, 192, 192 }, { 192, 192, 255 }, { 192, 192, 000 }, { 000, 192, 192 } //It shouldn't need more than this but there'll be a fallback.
                };
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
                    public static Pen HIGHLIGHT = new Pen(Colours.HIGHLIGHT_BOX, LINE_THICKNESS + 1); //Thinner than the selection box so you can see highlights and selections at the same time.
                }
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (globals.open_map > - 1)
            {
                System.Diagnostics.Stopwatch timer = new Stopwatch();
                timer.Start();
                for (int x = 0; x < globals.sys_data.header.map_x; x++) //Draw boxes.
                {
                    for (int y = 0; y < globals.sys_data.header.map_y; y++)
                    {
                        for (int z = 0; z < globals.sys_data.header.behaviour_count; z++)
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
                            switch (globals.sys_data.behaviour_tiles[z][x + (y * globals.sys_data.header.map_x)].type) //This really should not render just 0...
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
                                case 0x5:
                                    this_brush = MapRender.DrawingElements.Brushes.FLOOR;
                                    this_bitmap = MapRender.Icons.Graphics1.ARROW_UP_YELLOW;
                                    draw_bitmap = true;
                                    break;
                                case 0x6:
                                    this_brush = MapRender.DrawingElements.Brushes.FLOOR;
                                    this_bitmap = MapRender.Icons.Graphics1.ARROW_DOWN_YELLOW;
                                    draw_bitmap = true;
                                    break;
                                case 0x7:
                                    this_brush = MapRender.DrawingElements.Brushes.FLOOR;
                                    this_bitmap = MapRender.Icons.Graphics1.ARROW_LEFT_YELLOW;
                                    draw_bitmap = true;
                                    break;
                                case 0x8:
                                    this_brush = MapRender.DrawingElements.Brushes.FLOOR;
                                    this_bitmap = MapRender.Icons.Graphics1.ARROW_RIGHT_YELLOW;
                                    draw_bitmap = true;
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
                                    int tile_to_left = globals.sys_data.behaviour_tiles[0][x - 1 + (y * globals.sys_data.header.map_x)].type; //Check the cell to the left to see if it's walkable. This is a bit of a hack to get the orientation, but it seems solid.
                                    if (tile_to_left == 0x1 || tile_to_left == 0x2 || tile_to_left == 0x3 || tile_to_left == 0x9 || tile_to_left == 0xC)
                                    {
                                        this_bitmap = MapRender.Icons.Graphics1.ARROW_LEFT_RIGHT;
                                    }
                                    draw_bitmap = true;
                                    break;
                                case 0x11:
                                    this_bitmap = MapRender.Icons.Graphics1.ARROW_UP; //Same as 0xE
                                    draw_bitmap = true;
                                    break;
                                case 0x12:
                                    this_bitmap = MapRender.Icons.Graphics1.EXCLAMATION_POINT; //Temporary
                                    draw_bitmap = true;
                                    break;
                                case 0x14:
                                    this_bitmap = MapRender.Icons.Graphics1.GEOMAGNETIC_POLE;
                                    draw_bitmap = true;
                                    break;
                                case 0x16: //Figure out how the data for this one works.
                                    break;
                                case 0x17: //And this...
                                    break;
                                case 0x18: //Scripted events don't use a tile type.
                                    break;
                                default:
                                    break;
                            }
                            if (cb_Type.SelectedIndex == (int)SYS_SELECTIONS.Encounters) //This will override the this_brush set in the previous switch statement.
                            {
                                if (cb_Subtype.SelectedIndex == 0) //Unfortunately, I need to duplicate the switch here to handle high ground.
                                {
                                    int encounter = globals.sys_data.encounters[x + (y * globals.sys_data.header.map_x)].encounter_id;
                                    if (globals.encounts.ContainsKey(encounter))
                                    {
                                        if (globals.encounts[encounter] < MapRender.Colours.GROUP_RGB.Length / 3) //Floors really shouldn't have more than 12, so this fallback should rarely be tripped.
                                        {
                                            switch (globals.sys_data.behaviour_tiles[0][x + (y * globals.sys_data.header.map_x)].type) //Take layer 0's tile only.
                                            {
                                                case 0x1:
                                                case 0x2:
                                                case 0x3:
                                                case 0x4:
                                                case 0x5:
                                                case 0x6:
                                                case 0x7:
                                                case 0x8:
                                                case 0x9:
                                                case 0x12:
                                                case 0x16:
                                                case 0x18:
                                                    this_brush = new SolidBrush(Color.FromArgb
                                                    (
                                                        MapRender.Colours.GROUP_RGB[globals.encounts[encounter], 0],
                                                        MapRender.Colours.GROUP_RGB[globals.encounts[encounter], 1],
                                                        MapRender.Colours.GROUP_RGB[globals.encounts[encounter], 2]
                                                    ));
                                                    break;
                                                case 0xC: //Lighten the palette slightly for high ground tiles.
                                                    this_brush = new SolidBrush(Color.FromArgb //Ternaries, man...
                                                    (
                                                        MapRender.Colours.GROUP_RGB[globals.encounts[encounter], 0] + 64 <= 255 ? MapRender.Colours.GROUP_RGB[globals.encounts[encounter], 0] + 64 : 255,
                                                        MapRender.Colours.GROUP_RGB[globals.encounts[encounter], 1] + 64 <= 255 ? MapRender.Colours.GROUP_RGB[globals.encounts[encounter], 1] + 64 : 255,
                                                        MapRender.Colours.GROUP_RGB[globals.encounts[encounter], 2] + 64 <= 255 ? MapRender.Colours.GROUP_RGB[globals.encounts[encounter], 2] + 64 : 255
                                                    ));
                                                    break;
                                            }
                                        }
                                    }
                                }
                                else if (cb_Subtype.SelectedIndex == 1)
                                {
                                    int danger = globals.sys_data.encounters[x + (y * globals.sys_data.header.map_x)].danger;
                                    if (danger <= 20)
                                    {
                                        switch (globals.sys_data.behaviour_tiles[0][x + (y * globals.sys_data.header.map_x)].type) //Take layer 0's tile only.
                                        {
                                            case 0x1:
                                            case 0x2:
                                            case 0x3:
                                            case 0x4:
                                            case 0x5:
                                            case 0x6:
                                            case 0x7:
                                            case 0x8:
                                            case 0x9:
                                            case 0xC:
                                            case 0x12:
                                            case 0x16:
                                            case 0x18:
                                                this_brush = new SolidBrush(Color.FromArgb
                                                (
                                                    MapRender.Colours.DANGER_RGB3[danger, 0],
                                                    MapRender.Colours.DANGER_RGB3[danger, 1],
                                                    MapRender.Colours.DANGER_RGB3[danger, 2]
                                                ));
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        this_brush = new SolidBrush(Color.Black);
                                    }
                                }
                            }
                            if (z == 0 || (z > 0 && this_brush != MapRender.DrawingElements.Brushes.BACKGROUND)) //We want background tiles on layer 0 only.
                            {
                                e.Graphics.FillRectangle(this_brush, pos);
                            }
                            if (draw_bitmap)
                            {
                                e.Graphics.DrawImage(this_bitmap, pos.X, pos.Y);
                            }
                        }
                    }
                }
                for (int x = 0; x < globals.sys_data.header.map_x + 1; x++) //Draw vertical lines. The line code and the box code have to be separated or else it renders awkwardly due to ordering.
                {
                    Pen line = MapRender.DrawingElements.Pens.THIN_LINE;
                    if ((x + 4) % 5 == 4)
                    {
                        line = MapRender.DrawingElements.Pens.THICK_LINE;
                    }
                    e.Graphics.DrawLine(line,
                        new PointF
                        (
                            MapRender.LEFT_EDGE + (x * (MapRender.BOX_WIDTH + MapRender.LINE_THICKNESS)) - 1, //Not entirely sure why I need - 1...
                            MapRender.TOP_EDGE - 1
                        ),
                        new PointF
                        (
                            MapRender.LEFT_EDGE + (x * (MapRender.BOX_WIDTH + MapRender.LINE_THICKNESS)) - 1,
                            MapRender.TOP_EDGE + (globals.sys_data.header.map_y * (MapRender.BOX_HEIGHT + MapRender.LINE_THICKNESS)) - 1
                        )
                    );
                }
                for (int y = 0; y < globals.sys_data.header.map_y + 1; y++) //Draw horizontal lines.
                {
                    Pen line = MapRender.DrawingElements.Pens.THIN_LINE;
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
                            MapRender.LEFT_EDGE + (globals.sys_data.header.map_x * (MapRender.BOX_WIDTH + MapRender.LINE_THICKNESS)) - 1,
                            MapRender.TOP_EDGE + (y * (MapRender.BOX_HEIGHT + MapRender.LINE_THICKNESS)) - 1
                        )
                    );
                }
                if (globals.highlighted_box.Count > 0)
                {
                    for (int x = 0; x < globals.highlighted_box.Count; x++)
                    {
                        Rectangle pos = new Rectangle
                        (
                            MapRender.LEFT_EDGE + (globals.highlighted_box_x[x] * (MapRender.BOX_WIDTH + MapRender.LINE_THICKNESS)),
                            MapRender.TOP_EDGE + (globals.highlighted_box_y[x] * (MapRender.BOX_HEIGHT + MapRender.LINE_THICKNESS)),
                            MapRender.BOX_WIDTH,
                            MapRender.BOX_HEIGHT
                        );
                        e.Graphics.DrawRectangle(MapRender.DrawingElements.Pens.HIGHLIGHT, pos);
                    }
                }
                if (globals.selected_box.Count > 0)
                {
                    for (int x = 0; x < globals.selected_box.Count; x++)
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
                if (globals.highlighted_box.Count > 0 && globals.selected_box.Count > 0) //This just kinda has to render on top of the other boxes.
                {
                    for (int x = 0; x < globals.highlighted_box.Count; x++) //There is a way to make this nicer for boxes that share an edge but I am not spending the time doing that.
                    {
                        if (globals.highlighted_box.Count > globals.selected_box.Count) //Don't really feel great about this code here. It's a lot of work to get highlighted + selected boxes playing nicely.
                        {
                            for (int y = 0; y < globals.selected_box.Count; y++)
                            {
                                if ((globals.highlighted_box_x[x] == globals.selected_box_x[y]) && (globals.highlighted_box_y[x] == globals.selected_box_y[y]))
                                {
                                    Rectangle pos = new Rectangle
                                    (
                                    MapRender.LEFT_EDGE + (globals.highlighted_box_x[x] * (MapRender.BOX_WIDTH + MapRender.LINE_THICKNESS)),
                                        MapRender.TOP_EDGE + (globals.highlighted_box_y[x] * (MapRender.BOX_HEIGHT + MapRender.LINE_THICKNESS)),
                                        MapRender.BOX_WIDTH,
                                        MapRender.BOX_HEIGHT
                                    );
                                    //Making a gradient is a pain in the ass...
                                    Rectangle brush_rect = new Rectangle
                                    (
                                        MapRender.LEFT_EDGE + (globals.highlighted_box_x[x] * (MapRender.BOX_WIDTH + MapRender.LINE_THICKNESS) - 1),
                                        MapRender.TOP_EDGE + (globals.highlighted_box_y[x] * (MapRender.BOX_HEIGHT + MapRender.LINE_THICKNESS)),
                                        MapRender.BOX_WIDTH + 3,
                                        MapRender.BOX_HEIGHT
                                    );
                                    using (LinearGradientBrush brush = new LinearGradientBrush(brush_rect, MapRender.Colours.HIGHLIGHT_BOX, MapRender.Colours.SELECTION_BOX, LinearGradientMode.Horizontal))
                                    {
                                        using (Pen pen = new Pen(brush, MapRender.LINE_THICKNESS + 1))
                                        {
                                            e.Graphics.DrawRectangle(pen, pos);
                                        }
                                    }
                                }
                            }
                        }
                        else if (globals.selected_box.Count >= globals.highlighted_box.Count)
                        {
                            for (int y = 0; y < globals.highlighted_box.Count; y++)
                            {
                                if ((globals.selected_box_x[x] == globals.highlighted_box_x[y]) && (globals.selected_box_y[x] == globals.highlighted_box_y[y]))
                                {
                                    Rectangle pos = new Rectangle
                                    (
                                        MapRender.LEFT_EDGE + (globals.selected_box_x[x] * (MapRender.BOX_WIDTH + MapRender.LINE_THICKNESS)),
                                        MapRender.TOP_EDGE + (globals.selected_box_y[x] * (MapRender.BOX_HEIGHT + MapRender.LINE_THICKNESS)),
                                        MapRender.BOX_WIDTH,
                                        MapRender.BOX_HEIGHT
                                    );
                                    Rectangle brush_rect = new Rectangle
                                    (
                                        MapRender.LEFT_EDGE + (globals.selected_box_x[x] * (MapRender.BOX_WIDTH + MapRender.LINE_THICKNESS) - 1),
                                        MapRender.TOP_EDGE + (globals.selected_box_y[x] * (MapRender.BOX_HEIGHT + MapRender.LINE_THICKNESS)),
                                        MapRender.BOX_WIDTH + 3,
                                        MapRender.BOX_HEIGHT
                                    );
                                    using (LinearGradientBrush brush = new LinearGradientBrush(brush_rect, MapRender.Colours.HIGHLIGHT_BOX, MapRender.Colours.SELECTION_BOX, LinearGradientMode.Horizontal))
                                    {
                                        using (Pen pen = new Pen(brush, MapRender.LINE_THICKNESS + 1))
                                        {
                                            e.Graphics.DrawRectangle(pen, pos);
                                        }
                                    }
                                }
                            }
                        }
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