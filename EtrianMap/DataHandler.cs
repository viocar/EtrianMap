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
using OriginTablets.Types;

namespace EtrianMap
{
    public partial class EtrianMap : Form
    {
        private void SaveFile()
        {

        }
        private void BuildInitialMapData(byte[] sys, byte[] gfx)
        {
            MSBHeader header = new MSBHeader();
            header.map_x = BitConverter.ToInt32(sys, 0x8);
            header.map_y = BitConverter.ToInt32(sys, 0xC);
            header.behaviour_count = BitConverter.ToInt32(sys, 0x10);
            header.behaviour_pointer = BitConverter.ToInt32(sys, 0x14);
            header.tile_type_count = BitConverter.ToInt32(sys, 0x18);
            header.tile_type_pointer = BitConverter.ToInt32(sys, 0x1C);
            header.encounter_pointer = BitConverter.ToInt32(sys, 0x20);
            for (int x = 0; x < header.behaviour_count; x++)
            {
                int ptr = BitConverter.ToInt32(sys, header.behaviour_pointer + (x * 0x4));
                List<MSBTileType> tile_type_entry = new List<MSBTileType>();
                for (int y = 0; y < (header.map_x * header.map_y); y++)
                {
                    MSBTileType tile = new MSBTileType();
                    tile.type = BitConverter.ToUInt16(sys, ptr + (y * 4));
                    tile.id = BitConverter.ToUInt16(sys, ptr + ((y * 4) + 2));
                    tile_type_entry.Add(tile);
                }
                header.tile_types.Add(tile_type_entry);
            }
        }
    }
}