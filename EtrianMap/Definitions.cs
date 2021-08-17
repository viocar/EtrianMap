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
    public class MapDatCollection
    {
        public string sys_filename { get; set; }
        public string gfx_filename { get; set; }
        public byte[] sys_file { get; set; }
        public byte[] gfx_file { get; set; }
    }
    public class Flags
    {
        public bool sample_renderer_enabled = false; //Render the sample map.
        public int open_map = -1;
    }
    public class MSBHeader
    {
        //Used in all maps. Rename when you understand what these are better.
        public const string magic = "DGMS"; //0x0
        public const int _0x4 = 1;
        public int map_x { get; set; } //0x8
        public int map_y { get; set; } //0xC
        public int behaviour_count { get; set; } //0x10
        public int behaviour_pointer { get; set; } //0x14
        public int tile_type_count { get; set; } //0x18
        public int tile_type_pointer { get; set; } //0x1C
        public int encounter_pointer { get; set; } //0x20
        public const int _0x24 = 0;
        public const int _0x28 = 0;
        public List<List<MSBTileType>> tile_types = new List<List<MSBTileType>>(); //0x2C through the end
    }
    public class MSBEncounter //20
    {
        public ushort encounter_id { get; set; }
        public ushort danger { get; set; }
        public ushort unknown_1 { get; set; } //Used very rarely and only ever has a value of 1 if used.
        public ushort unknown_2 { get; set; } //Never used.
    }
    public class MSBTileType
    {
        public ushort type { get; set; }
        public ushort id { get; set; }
    }
    public class MGBPointers
    {
        public int p_70 { get; set; }
        public int p_74 { get; set; }
        public int p_78 { get; set; }
        public int p_7C { get; set; }
        public int p_80 { get; set; }
        public int p_84 { get; set; }
        public int p_88 { get; set; }
        //Used in Nexus only.
        public int p_8C { get; set; }
        public int p_90 { get; set; }
        public int p_94 { get; set; }
    }
    public class MGBLayer
    {
        public byte id { get; set; } = 0;
        public byte rotation { get; set; } = 0;
        public byte unk_1 { get; set; } = 0;
        public byte unk_2 { get; set; } = 0;
    }
    public class MGBFileIndex
    {
        public uint index { get; set; } = 0;
        public string file_1 { get; set; } = ""; //These should probably check for ASCII-compatibility
        public string file_2 { get; set; } = ""; //These should probably check for ASCII-compatibility
        public string file_3 { get; set; } = ""; //These should probably check for ASCII-compatibility
        public string file_4 { get; set; } = ""; //These should probably check for ASCII-compatibility
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