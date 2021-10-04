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
using OriginTablets.Types;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace EtrianMap
{
    public class Globals
    {
        public int open_map = -1;
        public List<int> selected_box = new List<int>();
        public List<int> selected_box_x = new List<int>();
        public List<int> selected_box_y = new List<int>();
        public Rectangle map_area = new Rectangle(); //Cannot be determined until the map is loaded.
        public List<byte[]> binaries = new List<byte[]>();
        public List<Table> tables = new List<Table>();
        public List<MBM> mbms = new List<MBM>();
        public List<MapDatCollection> mapdat_list = new List<MapDatCollection>(); 
        public string open_path = ""; //Initialized when a file is loaded.
        public MSBFile sys_data { get; set; }
        public MGBFile gfx_data { get; set; }
    }
    public class MapDatCollection
    {
        public string base_path { get; set; }
        public string sys_filename { get; set; }
        public string gfx_filename { get; set; }
        public byte[] sys_file { get; set; }
        public byte[] gfx_file { get; set; }
    }
    
    //MSB
    public class MSBFile
    {
        public MSBHeader header = new MSBHeader();
        public List<MSBTileContainer> containers = new List<MSBTileContainer>();
        public List<List<MSBBehaviours>> behaviour_tiles = new List<List<MSBBehaviours>>(); //0x2C through the end
        public List<MSBCellEncounter> encounters = new List<MSBCellEncounter>();
        public List<MSBTileTypeInfo> tile_data = new List<MSBTileTypeInfo>();
        public List<MSBTileTypeObject> tile_objects = new List<MSBTileTypeObject>();
        public List<MSBStaircase> staircases = new List<MSBStaircase>();
        public List<MSBChest> chests = new List<MSBChest>();
        public List<MSBTwoWayPassage> two_way_passages = new List<MSBTwoWayPassage>();
        public List<MSBOneWayPassage> one_way_passages = new List<MSBOneWayPassage>();
        public List<MSBDoodad> doodads = new List<MSBDoodad>();
        public List<MSBSnake> snakes = new List<MSBSnake>();
        public List<MSBScript> scripted_events = new List<MSBScript>();
        public List<MSBRisingPlatform> rising_platforms = new List<MSBRisingPlatform>();
        public byte[] garbage = new byte[0x68];
    }
    public class MSBHeader
    {
        public const int MAGIC = 0x534D4744; //0x0, "DGMS"
        public const int _0x4 = 1;
        public int map_x { get; set; } //0x8
        public int map_y { get; set; } //0xC
        public int behaviour_count { get; set; } //0x10
        public int behaviour_pointer { get; set; } //0x14
        public int tile_type_count { get; set; } //0x18
        public int tile_type_pointer { get; set; } //0x1C
        public int encounter_pointer { get; set; } //0x20
        public const int _0x24 = 0; //Never used
        public const int _0x28 = 0; //Never used
    }
    public class MSBTileContainer //This is a list that tracks whether something is on each tile.
    {
        public List<bool> behaviour { get; set; } = new List<bool>(); //Since each behaviour can have only one entry per tile, we're using bools for them.
        public byte doors { get; set; } = 0; //For tile objects, we can have more than one. Most of these will never be more than one, but we're going to use a byte for consistency.
        public byte stairs { get; set; } = 0;
        public byte chests { get; set; } = 0;
        public byte two_ways { get; set; } = 0;
        public byte one_ways { get; set; } = 0;
        public byte doodads { get; set; } = 0;
        public byte pits { get; set; } = 0;
        public byte poles { get; set; } = 0;
        public byte unknown { get; set; } = 0;
        public byte currents { get; set; } = 0;
        public byte snakes { get; set; } = 0;
        public byte scripts { get; set; } = 0; //Theoretically, you could have more than 256 scripts on a tile, which would break this code. That's not a reasonable concern, though...
        public byte risers { get; set; } = 0;
        //Encounters are not in here because they are pulled directly from the encounter data. You can't really do that with the other types without it getting weird.
    }
    public class MSBCellEncounter //20
    {
        public ushort encounter_id { get; set; }
        public ushort danger { get; set; }
        public ushort unknown_1 { get; set; } //Used very rarely and only ever has a value of 1 if used.
        public ushort unknown_2 { get; set; } //Never used.
    }
    public class MSBBehaviours
    {
        public byte type { get; set; }
        public byte id { get; set; }
    }
    public class MSBTileTypeInfo
    {
        public uint index { get; set; }
        public uint entries { get; set; }
        public uint entry_ptr { get; set; }
        public uint data_ptr { get; set; }
        public uint data_length { get; set; }
    }
    public class MSBTileTypeObject
    {
        public byte type { get; set; }
        public byte id { get; set; }
        public byte graphic { get; set; }
        public byte unknown_1 { get; set; }
        public byte unknown_2 { get; set; }
        public byte map_x { get; set; }
        public byte map_y { get; set; }
        public byte unknown_3 { get; set; }
        public byte activation_direction_1 { get; set; } //Figure out which of these is which direction.
        public byte activation_direction_2 { get; set; }
        public byte activation_direction_3 { get; set; }
        public byte activation_direction_4 { get; set; }
        public const ulong UNUSED_1 = 0; //These values appear to never be used, so I'm just gonna pack them as longs 
        public const ulong UNUSED_2 = 0; //In EO5, this was 0xC long and could sometimes have a script value. This functionality seems to have changed in EON?
    }
    public class MSBStaircase
    {
        public byte dest_floor { get; set; }
        public byte dest_x { get; set; }
        public byte dest_y { get; set; }
        public byte dest_facing { get; set; }
        public byte sfx { get; set; }
        public byte interact_message { get; set; }
        public byte unknown_1 { get; set; }
        public byte unknown_2 { get; set; }
    }
    public class MSBChest
    {
        public byte is_item { get; set; } //Technically, this is a bool, but setting it as byte makes it easier for the data handlers. This might actually be an int...
        public byte unknown_1 { get; set; }
        public byte unknown_2 { get; set; }
        public byte unknown_3 { get; set; }
        public uint value { get; set; }
    }
    public class MSBTwoWayPassage //These values are never used in EON, but they are used in EO5's 6th stratum. I can't tell what for.
    {
        public byte unknown_1 { get; set; }
        public byte unknown_2 { get; set; }
        public byte unknown_3 { get; set; }
        public byte unknown_4 { get; set; }
    }
    public class MSBOneWayPassage
    {
        public byte unknown_1 { get; set; }
        public byte unknown_2 { get; set; }
        public byte unknown_3 { get; set; }
        public byte unknown_4 { get; set; }
    }
    public class MSBDoodad
    {
        public byte id_1 { get; set; }
        public byte unknown_1_1 { get; set; }
        public byte unknown_2_1 { get; set; }
        public byte unknown_3_1 { get; set; }
        public byte id_2 { get; set; }
        public byte unknown_1_2 { get; set; }
        public byte unknown_2_2 { get; set; }
        public byte unknown_3_2 { get; set; }
        public byte id_3 { get; set; }
        public byte unknown_1_3 { get; set; }
        public byte unknown_2_3 { get; set; }
        public byte unknown_3_3 { get; set; }
        public byte id_4 { get; set; }
        public byte unknown_1_4 { get; set; }
        public byte unknown_2_4 { get; set; }
        public byte unknown_3_4 { get; set; }
    }
    public class MSBPit
    {
        //Pits are never used in the base game OR in EO5, so I'm not sure what data is expected
    }
    public class MSBSnake
    {
        public byte id { get; set; }
        public byte unknown_1 { get; set; }
        public byte position { get; set; } //This is which position within the snake it is
        public byte unknown_2 { get; set; }
    }
    public class MSBScript //The format for this is different in EO5
    {
        public ushort flag_1 { get; set; }
        public ushort flag_2 { get; set; }
        public ushort flag_3 { get; set; }
        public ushort required_flag { get; set; }
        public byte unknown_1 { get; set; }
        public byte unknown_2 { get; set; }
        public byte unknown_3 { get; set; }
        public byte unknown_4 { get; set; }
        public byte unknown_5 { get; set; }
        public byte unknown_6 { get; set; }
        public byte unknown_7 { get; set; }
        public byte unknown_8 { get; set; }
        public byte prompt { get; set; }
        public byte unknown_9 { get; set; }
        public ushort unknown_10 { get; set; } //This is probably the proc() used

        private string p_script_name;
        public string script_name
        {
            get
            {
                return p_script_name;
            }
            set
            {
                if (value.Length > 0x18)
                {
                    p_script_name = value.Substring(0, 0x18);
                }
                else
                {
                    p_script_name = value;
                }
            }
        }
    }
    public class MSBRisingPlatform
    {
        public byte id { get; set; }
        public byte unknown_1 { get; set; }
        public byte unknown_2 { get; set; }
        public byte unknown_3 { get; set;  }
    }
    //MGB
    public class MGBFile
    {
        public MGBHeader header = new MGBHeader();
        public List<List<MGBTile>> layer_tiles = new List<List<MGBTile>>();
        public List<MGBFileIndex> indices = new List<MGBFileIndex>();
    }

    public class MGBHeader
    {
        public const int MAGIC = 0x474D4744; //0x0, "DGMG"
        public const int _0x4 = 1;
        public int map_x { get; set; } //0x8
        public int map_y { get; set; } //0xC
        public int layer_count { get; set; } //0x10
        public int layer_pointer { get; set; } //0x14, always 0x70 for EON
        public int filename_count { get; set; } //0x18
        public int filename_pointer { get; set; } //0x1C
    }

    public class MGBTile
    {
        public byte id { get; set; } = 0;
        public byte rotation { get; set; } = 0;
        public byte unknown_1 { get; set; } = 0;
        public byte unknown_2 { get; set; } = 0;
    }
    public class MGBFileIndex
    {
        public uint index { get; set; } = 0;
        private string p_file_1;
        public string file_1 
        {
            get
            {
                return p_file_1;
            }
            set
            {
                if (value.Length > 0x20)
                {
                    p_file_1 = value.Substring(0, 0x20);
                }
                else
                {
                    p_file_1 = value;
                }
            }
        }
        private string p_file_2;
        public string file_2
        {
            get
            {
                return p_file_2;
            }
            set
            {
                if (value.Length > 0x20)
                {
                    p_file_2 = value.Substring(0, 0x20);
                }
                else
                {
                    p_file_2 = value;
                }
            }
        }
        private string p_file_3;
        public string file_3
        {
            get
            {
                return p_file_3;
            }
            set
            {
                if (value.Length > 0x20)
                {
                    p_file_3 = value.Substring(0, 0x20);
                }
                else
                {
                    p_file_3 = value;
                }
            }
        }
        private string p_file_4;
        public string file_4
        {
            get
            {
                return p_file_4;
            }
            set
            {
                if (value.Length > 0x20)
                {
                    p_file_4 = value.Substring(0, 0x20);
                }
                else
                {
                    p_file_4 = value;
                }
            }
        }
        public byte file_1_rotation { get; set; } = 0;
        public byte file_2_rotation { get; set; } = 0;
        public byte file_3_rotation { get; set; } = 0;
        public byte file_4_rotation { get; set; } = 0;
        public byte unknown_1 { get; set; } = 0;
        public byte unknown_2 { get; set; } = 0;
        public byte unknown_3 { get; set; } = 0;
        public byte unknown_4 { get; set; } = 0;
        public byte unknown_5 { get; set; } = 0;
        public byte unknown_6 { get; set; } = 0;
        public byte unknown_7 { get; set; } = 0;
        public byte unknown_8 { get; set; } = 0;
        public byte unknown_9 { get; set; } = 0;
        public byte unknown_10 { get; set; } = 0;
        public byte unknown_11 { get; set; } = 0;
        public byte unknown_12 { get; set; } = 0;
    }
}