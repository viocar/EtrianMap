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
        public MSBFile BuildInitialMapData(byte[] sys, byte[] gfx)
        {
            Debug.WriteLine("here1");
            MSBFile file = new MSBFile();
            MSBHeader header = new MSBHeader();
            header.map_x = BitConverter.ToInt32(sys, 0x8);
            header.map_y = BitConverter.ToInt32(sys, 0xC);
            header.behaviour_count = BitConverter.ToInt32(sys, 0x10);
            header.behaviour_pointer = BitConverter.ToInt32(sys, 0x14);
            header.tile_type_count = BitConverter.ToInt32(sys, 0x18);
            header.tile_type_pointer = BitConverter.ToInt32(sys, 0x1C);
            header.encounter_pointer = BitConverter.ToInt32(sys, 0x20);
            file.header = header;
            for (int x = 0; x < header.behaviour_count; x++) //A list of lists because we generally want the tile type entries to be organized with each other
            {
                int ptr = BitConverter.ToInt32(sys, header.behaviour_pointer + (x * 0x4));
                for (int y = 0; y < (header.map_x * header.map_y); y++)
                {
                    MSBCellTileType tile = new MSBCellTileType();
                    tile.type = sys[ptr + (y * 2)];
                    tile.id = sys[ptr + ((y * 2) + 1)];
                    file.cell_tile_types.Add(tile);
                }
            }
            for (int x = 0; x < header.tile_type_count; x++)
            {
                int ptr = header.tile_type_pointer;
                MSBTileTypeInfo tile = new MSBTileTypeInfo();
                tile.index = BitConverter.ToUInt32(sys, ptr + (x * 0x14));
                tile.entries = BitConverter.ToUInt32(sys, ptr + 4 + (x * 0x14));
                tile.entry_ptr = BitConverter.ToUInt32(sys, ptr + 8 + (x * 0x14));
                tile.data_ptr = BitConverter.ToUInt32(sys, ptr + 0xC + (x * 0x14));
                tile.data_length = BitConverter.ToUInt32(sys, ptr + 0x10 + (x * 0x14));
                file.tile_data.Add(tile);
            }
            foreach (MSBTileTypeInfo tile in file.tile_data)
            {
                switch (tile.index)
                {
                    case uint n when (n < 0xD):
                        break;
                    case 0xD:
                        break;
                    case 0xE:
                        for (int x = 0; x < tile.entries; x++)
                        {
                            MSBStaircase staircase = new MSBStaircase();
                            staircase.dest_floor = sys[tile.data_ptr + (x * tile.data_length)];
                            staircase.dest_x = sys[tile.data_ptr + (x * tile.data_length) + 1];
                            staircase.dest_y = sys[tile.data_ptr + (x * tile.data_length) + 2];
                            staircase.dest_facing = sys[tile.data_ptr + (x * tile.data_length) + 3];
                            staircase.sfx = sys[tile.data_ptr + (x * tile.data_length) + 4];
                            staircase.interact_message = sys[tile.data_ptr + (x * tile.data_length) + 5];
                            staircase.unknown_1 = sys[tile.data_ptr + (x * tile.data_length) + 6];
                            staircase.unknown_2 = sys[tile.data_ptr + (x * tile.data_length) + 7];
                            file.staircases.Add(staircase);
                            Debug.WriteLine(staircase.dest_floor);
                            Debug.WriteLine(staircase.dest_x);
                            Debug.WriteLine(staircase.dest_y);
                            Debug.WriteLine(staircase.sfx);
                        }
                        break;
                    case 0xF:
                        for (int x = 0; x < tile.entries; x++)
                        {
                            MSBChest chest = new MSBChest();
                            chest.is_item = sys[tile.data_ptr + (x * tile.data_length)];
                            chest.unknown_1 = sys[tile.data_ptr + (x * tile.data_length) + 1];
                            chest.unknown_2 = sys[tile.data_ptr + (x * tile.data_length) + 2];
                            chest.unknown_3 = sys[tile.data_ptr + (x * tile.data_length) + 3];
                            chest.value = BitConverter.ToUInt32(sys, (int)(tile.data_ptr + (x * tile.data_length) + 4)); //This is a long unless I cast it... why?
                            file.chests.Add(chest);
                        }
                        break;
                    case 0x10:
                        for (int x = 0; x < tile.entries; x++)
                        {
                            MSBTwoWayPassage two_way = new MSBTwoWayPassage();
                            two_way.unknown_1 = sys[tile.data_ptr + (x * tile.data_length)];
                            two_way.unknown_2 = sys[tile.data_ptr + (x * tile.data_length) + 1];
                            two_way.unknown_3 = sys[tile.data_ptr + (x * tile.data_length) + 2];
                            two_way.unknown_4 = sys[tile.data_ptr + (x * tile.data_length) + 3];
                            file.two_way_passages.Add(two_way);
                        }
                        break;
                }
            }
            for (int x = 0; x < header.map_x * header.map_y; x++)
            {
                int ptr = header.encounter_pointer;
                MSBCellEncounter enc = new MSBCellEncounter();
                enc.encounter_id = BitConverter.ToUInt16(sys, ptr + (x * 8));
                enc.danger = BitConverter.ToUInt16(sys, ptr + 2 + (x * 8));
                enc.unknown_1 = BitConverter.ToUInt16(sys, ptr + 4 + (x * 8));
                enc.unknown_2 = BitConverter.ToUInt16(sys, ptr + 6 + (x * 8));
                file.encounters.Add(enc);
            }
            return file;
        }
        private void SaveFile()
        {
            //We need to first build the 
        }
    }
}