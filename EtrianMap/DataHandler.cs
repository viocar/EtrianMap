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
using System.IO;

namespace EtrianMap
{
    public partial class EtrianMap : Form
    {
        public MSBFile BuildInitialMapData()
        {
            //Initial variable setup for easy access.
            byte[] sys = globals.mapdat_list[globals.open_map].sys_file;
            byte[] gfx = globals.mapdat_list[globals.open_map].gfx_file;
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

            //Get all the behaviours.
            for (int x = 0; x < header.behaviour_count; x++) 
            {
                List<MSBBehaviours> section = new List<MSBBehaviours>();
                int ptr = BitConverter.ToInt32(sys, header.behaviour_pointer + (x * 0x4));
                for (int y = 0; y < (header.map_x * header.map_y); y++)
                {
                    MSBBehaviours tile = new MSBBehaviours();
                    tile.type = sys[ptr + (y * 2)];
                    tile.id = sys[ptr + ((y * 2) + 1)];
                    section.Add(tile);
                }
                file.behaviour_tiles.Add(section);
            }

            //Get all the object data.
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

            //Get all the tile data. This is fairly involved.
            foreach (MSBTileTypeInfo tile in file.tile_data)
            {
                switch (tile.index)
                {
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
                    case 0x11:
                        for (int x = 0; x < tile.entries; x++)
                        {
                            MSBOneWayPassage one_way = new MSBOneWayPassage();
                            one_way.unknown_1 = sys[tile.data_ptr + (x * tile.data_length)];
                            one_way.unknown_2 = sys[tile.data_ptr + (x * tile.data_length) + 1];
                            one_way.unknown_3 = sys[tile.data_ptr + (x * tile.data_length) + 2];
                            one_way.unknown_4 = sys[tile.data_ptr + (x * tile.data_length) + 3];
                            file.one_way_passages.Add(one_way);
                        }
                        break;
                    case 0x12: //This is messy 
                        for (int x = 0; x < tile.entries; x++)
                        {
                            MSBDoodad doodad = new MSBDoodad();
                            doodad.id_1 = sys[tile.data_ptr + (x * tile.data_length)];
                            doodad.unknown_1_1 = sys[tile.data_ptr + (x * tile.data_length) + 1];
                            doodad.unknown_2_1 = sys[tile.data_ptr + (x * tile.data_length) + 2];
                            doodad.unknown_3_1 = sys[tile.data_ptr + (x * tile.data_length) + 3];
                            doodad.id_2 = sys[tile.data_ptr + (x * tile.data_length) + 4];
                            doodad.unknown_1_2 = sys[tile.data_ptr + (x * tile.data_length) + 5];
                            doodad.unknown_2_2 = sys[tile.data_ptr + (x * tile.data_length) + 6];
                            doodad.unknown_3_2 = sys[tile.data_ptr + (x * tile.data_length) + 7];
                            doodad.id_3 = sys[tile.data_ptr + (x * tile.data_length) + 8];
                            doodad.unknown_1_3 = sys[tile.data_ptr + (x * tile.data_length) + 9];
                            doodad.unknown_2_3 = sys[tile.data_ptr + (x * tile.data_length) + 0xA];
                            doodad.unknown_3_3 = sys[tile.data_ptr + (x * tile.data_length) + 0xB];
                            doodad.id_4 = sys[tile.data_ptr + (x * tile.data_length) + 0xC];
                            doodad.unknown_1_4 = sys[tile.data_ptr + (x * tile.data_length) + 0xD];
                            doodad.unknown_2_4 = sys[tile.data_ptr + (x * tile.data_length) + 0xE];
                            doodad.unknown_3_4 = sys[tile.data_ptr + (x * tile.data_length) + 0xF];
                            file.doodads.Add(doodad);
                        }
                        break;
                    case 0x17:
                        for (int x = 0; x < tile.entries; x++)
                        {
                            MSBSnake snake = new MSBSnake();
                            snake.id = sys[tile.data_ptr + (x * tile.data_length)];
                            snake.unknown_1 = sys[tile.data_ptr + (x * tile.data_length) + 1];
                            snake.position = sys[tile.data_ptr + (x * tile.data_length) + 2];
                            snake.unknown_2 = sys[tile.data_ptr + (x * tile.data_length) + 3];
                            file.snakes.Add(snake);
                        }
                        break;
                    case 0x18:
                        for (int x = 0; x < tile.entries; x++)
                        {
                            MSBScript script = new MSBScript();
                            script.flag_1 = BitConverter.ToUInt16(sys, (int)(tile.data_ptr + (x * tile.data_length)));
                            script.flag_2 = BitConverter.ToUInt16(sys, (int)(tile.data_ptr + (x * tile.data_length) + 2));
                            script.flag_3 = BitConverter.ToUInt16(sys, (int)(tile.data_ptr + (x * tile.data_length) + 4));
                            script.required_flag = BitConverter.ToUInt16(sys, (int)(tile.data_ptr + (x * tile.data_length) + 6));
                            script.unknown_1 = sys[tile.data_ptr + (x * tile.data_length) + 8];
                            script.unknown_2 = sys[tile.data_ptr + (x * tile.data_length) + 9];
                            script.unknown_3 = sys[tile.data_ptr + (x * tile.data_length) + 0xA];
                            script.unknown_4 = sys[tile.data_ptr + (x * tile.data_length) + 0xB];
                            script.unknown_5 = sys[tile.data_ptr + (x * tile.data_length) + 0xC];
                            script.unknown_6 = sys[tile.data_ptr + (x * tile.data_length) + 0xD];
                            script.unknown_7 = sys[tile.data_ptr + (x * tile.data_length) + 0xE];
                            script.unknown_8 = sys[tile.data_ptr + (x * tile.data_length) + 0xF];
                            script.prompt = sys[tile.data_ptr + (x * tile.data_length) + 0x10];
                            script.unknown_9 = sys[tile.data_ptr + (x * tile.data_length) + 0x11];
                            script.unknown_10 = BitConverter.ToUInt16(sys, (int)(tile.data_ptr + (x * tile.data_length) + 0x12));
                            byte[] str = new byte[0x18];
                            for (int y = 0; y < 0x18; y++)
                            {
                                str[y] = sys[tile.data_ptr + (x * tile.data_length) + 0x14 + y];
                            }
                            script.script_name = Encoding.ASCII.GetString(str, 0, str.Length);
                        }
                        break;
                    default:
                        break;
                }
            }
            Array.Copy(sys, 0x5464, file.garbage, 0, 0x68); //This is probably completely unnecessary for functionality, but I'm copying it for the sake of identical write-back.
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
            //Initialize variables for easy use.
            MSBFile map = globals.map_data;
            string sys_path = globals.mapdat_list[globals.open_map].sys_filename;
            string gfx_path = globals.mapdat_list[globals.open_map].gfx_filename;
            string dungeonname = globals.open_path + "\\InterfaceFile\\dungeonname.mbm";
            string encount = globals.open_path + "\\Dungeon\\encount.tbl";
            string encount_group = globals.open_path + "\\Dungeon\\encount_group.tbl";
            string floor = globals.open_path + "\\Dungeon\\floor.tbl";
            string mapdat = globals.open_path + "\\MapDat\\";
            if (File.Exists(encount) && File.Exists(encount_group) && File.Exists(floor) && File.Exists(dungeonname) && File.Exists(mapdat + "\\sys01.msb") && File.Exists(mapdat + "\\gfx01.mgb"))
            {
                int behaviours = map.header.behaviour_count;
                int tile_type_info_count = map.header.tile_type_count;
                List<byte[]> cell_tile_types = new List<byte[]>();

                //We need to know how big the file will be. While most things are constant, there is some variability.
                int header_size = 0x2C; //Technically, this should be const, but I'm going to leave it as-is for consistency.
                int garbage_size = 0x68; //Same as header size...
                int behaviour_ptr_size = behaviours * 0x4; //Number of behaviour pointers
                int total_behaviour_size = behaviours * map.header.map_x * map.header.map_y * 0x2; //Number of tile behaviours in the file
                int tile_type_count_size = map.header.tile_type_count * 0x14; //Number of tile types for the tile type data section
                int tile_type_data_size = 0;
                foreach (MSBTileTypeInfo tile in map.tile_data) //Iterate through the tile type info to figure out how much space we need to allocate for those
                {
                    tile_type_data_size = tile_type_data_size + (int)(tile.entries * 0x1C); //Object entry
                    tile_type_data_size = tile_type_data_size + (int)(tile.entries * tile.data_length); //Data entry
                }
                int encounter_size = map.header.map_x * map.header.map_y * 0x8; //Number of tile encounters in the file
                int size = behaviour_ptr_size + total_behaviour_size + tile_type_count_size + tile_type_data_size + encounter_size + header_size + garbage_size; //Add all sizes together.
                byte[] save_byte = new byte[size];
                Debug.WriteLine(Convert.ToString(size, 16));

                //Create arrays for data. We could just write it all directly into save_byte, but this intermediate step makes it easier with different data sizes.
                int[] header = new int[header_size / 4];
                int[] behaviour_pointers = new int[behaviours];
                byte[] behaviour_bytes = new byte[total_behaviour_size]; //All the behaviours can be listed sequentially in here.
                byte[] tile_type_data_bytes = new byte[tile_type_count_size];
                byte[] staircase_bytes = new byte[map.staircases.Count * 8];
                byte[] chest_bytes = new byte[map.chests.Count * 8];
                byte[] two_way_bytes = new byte[map.two_way_passages.Count * 4];
                byte[] one_way_bytes = new byte[map.one_way_passages.Count * 4];
                byte[] doodad_bytes = new byte[map.doodads.Count * 4];
                byte[] snake_bytes = new byte[map.snakes.Count * 4];
                byte[] scripted_event_bytes = new byte[map.scripted_events.Count * 4];
                byte[] rising_platform_bytes = new byte[map.rising_platforms.Count * 4];

                //Populate the header.
                header[0] = MSBHeader.MAGIC; //I can't do map.header.MAGIC due to a quirk in C#
                header[1] = MSBHeader._0x4;
                header[2] = map.header.map_x;
                header[3] = map.header.map_y;
                header[4] = map.header.behaviour_count;
                header[5] = map.header.behaviour_pointer;
                header[6] = map.header.tile_type_count;
                header[7] = map.header.tile_type_pointer;
                header[8] = map.header.encounter_pointer;
                header[9] = MSBHeader._0x24;
                header[10] = MSBHeader._0x28;


                //Create the pointer list for each behaviour.
                for (int x = 0; x < behaviours; x++)
                {
                    behaviour_pointers[x] = header_size + (behaviours * 0x4) + (x * map.header.map_x * map.header.map_y * 2);
                }

                //Pack the data for each behaviour.
                for (int x = 0; x < behaviours; x++)
                {
                    for (int y = 0; y < map.header.map_x * map.header.map_y; y++)
                    {
                        behaviour_bytes[(x * map.header.map_x * map.header.map_y * 2) + (y * 2)] = map.behaviour_tiles[x][y].type;
                        behaviour_bytes[(x * map.header.map_x * map.header.map_y * 2) + (y * 2) + 1] = map.behaviour_tiles[x][y].id;
                    }
                }

                //Pack each data type. 
                //Staircases
                for (int x = 0; x < map.staircases.Count; x++)
                {
                    staircase_bytes[x * 0x8] = map.staircases[x].dest_floor;
                    staircase_bytes[x * 0x8 + 1] = map.staircases[x].dest_x;
                    staircase_bytes[x * 0x8 + 2] = map.staircases[x].dest_y;
                    staircase_bytes[x * 0x8 + 3] = map.staircases[x].dest_facing;
                    staircase_bytes[x * 0x8 + 4] = map.staircases[x].sfx;
                    staircase_bytes[x * 0x8 + 5] = map.staircases[x].interact_message;
                    staircase_bytes[x * 0x8 + 6] = map.staircases[x].unknown_1;
                    staircase_bytes[x * 0x8 + 7] = map.staircases[x].unknown_2;
                }

                //Chests
                for (int x = 0; x < map.chests.Count; x++)
                {
                    chest_bytes[x * 0x8] = map.chests[x].is_item;
                    chest_bytes[x * 0x8 + 1] = map.chests[x].unknown_1;
                    chest_bytes[x * 0x8 + 2] = map.chests[x].unknown_2;
                    chest_bytes[x * 0x8 + 3] = map.chests[x].unknown_3;
                    BitConverter.GetBytes(map.chests[x].value).CopyTo(chest_bytes, x * 0x8 + 4);
                }

                //Two-way passages
                for (int x = 0; x < map.two_way_passages.Count; x++)
                {
                    two_way_bytes[x * 0x4] = map.two_way_passages[x].unknown_1;
                    two_way_bytes[x * 0x4 + 1] = map.two_way_passages[x].unknown_2;
                    two_way_bytes[x * 0x4 + 2] = map.two_way_passages[x].unknown_3;
                    two_way_bytes[x * 0x4 + 3] = map.two_way_passages[x].unknown_4;
                }

                //One-way passages
                for (int x = 0; x < map.one_way_passages.Count; x++)
                {
                    one_way_bytes[x * 0x4] = map.one_way_passages[x].unknown_1;
                    one_way_bytes[x * 0x4 + 1] = map.one_way_passages[x].unknown_2;
                    one_way_bytes[x * 0x4 + 2] = map.one_way_passages[x].unknown_3;
                    one_way_bytes[x * 0x4 + 3] = map.one_way_passages[x].unknown_4;
                }

                //Doodads
                for (int x = 0; x < map.doodads.Count; x++)
                {
                    doodad_bytes[x * 0x10] = map.doodads[x].id_1;
                    doodad_bytes[x * 0x10 + 1] = map.doodads[x].unknown_1_1;
                    doodad_bytes[x * 0x10 + 2] = map.doodads[x].unknown_2_1;
                    doodad_bytes[x * 0x10 + 3] = map.doodads[x].unknown_3_1;
                    doodad_bytes[x * 0x10 + 4] = map.doodads[x].id_2;
                    doodad_bytes[x * 0x10 + 5] = map.doodads[x].unknown_1_2;
                    doodad_bytes[x * 0x10 + 6] = map.doodads[x].unknown_2_2;
                    doodad_bytes[x * 0x10 + 7] = map.doodads[x].unknown_3_2;
                    doodad_bytes[x * 0x10 + 8] = map.doodads[x].id_3;
                    doodad_bytes[x * 0x10 + 9] = map.doodads[x].unknown_1_3;
                    doodad_bytes[x * 0x10 + 0xA] = map.doodads[x].unknown_2_3;
                    doodad_bytes[x * 0x10 + 0xB] = map.doodads[x].unknown_3_3;
                    doodad_bytes[x * 0x10 + 0xC] = map.doodads[x].id_4;
                    doodad_bytes[x * 0x10 + 0xD] = map.doodads[x].unknown_1_4;
                    doodad_bytes[x * 0x10 + 0xE] = map.doodads[x].unknown_2_4;
                    doodad_bytes[x * 0x10 + 0xF] = map.doodads[x].unknown_3_4;
                }

                //Snakes
                for (int x = 0; x < map.snakes.Count; x++)
                {
                    snake_bytes[x * 0x4] = map.snakes[x].id;
                    snake_bytes[x * 0x4 + 1] = map.snakes[x].unknown_1;
                    snake_bytes[x * 0x4 + 2] = map.snakes[x].position;
                    snake_bytes[x * 0x4 + 3] = map.snakes[x].unknown_2;
                }

                //Scripted events
                for (int x = 0; x < map.scripted_events.Count; x++)
                {
                    BitConverter.GetBytes(map.scripted_events[x].flag_1).CopyTo(scripted_event_bytes, x * 0x2C);
                    BitConverter.GetBytes(map.scripted_events[x].flag_2).CopyTo(scripted_event_bytes, x * 0x2C + 2);
                    BitConverter.GetBytes(map.scripted_events[x].flag_3).CopyTo(scripted_event_bytes, x * 0x2C + 4);
                    BitConverter.GetBytes(map.scripted_events[x].required_flag).CopyTo(scripted_event_bytes, x * 0x2C + 6);
                    scripted_event_bytes[x + 0x2C + 8] = map.scripted_events[x].unknown_1;
                    scripted_event_bytes[x + 0x2C + 9] = map.scripted_events[x].unknown_2;
                    scripted_event_bytes[x + 0x2C + 0xA] = map.scripted_events[x].unknown_3;
                    scripted_event_bytes[x + 0x2C + 0xB] = map.scripted_events[x].unknown_4;
                    scripted_event_bytes[x + 0x2C + 0xC] = map.scripted_events[x].unknown_5;
                    scripted_event_bytes[x + 0x2C + 0xD] = map.scripted_events[x].unknown_6;
                    scripted_event_bytes[x + 0x2C + 0xE] = map.scripted_events[x].unknown_7;
                    scripted_event_bytes[x + 0x2C + 0xF] = map.scripted_events[x].unknown_8;
                    scripted_event_bytes[x + 0x2C + 0x10] = map.scripted_events[x].prompt;
                    scripted_event_bytes[x + 0x2C + 0x11] = map.scripted_events[x].unknown_9;
                    BitConverter.GetBytes(map.scripted_events[x].unknown_10).CopyTo(scripted_event_bytes, x * 0x2C + 12);
                    Encoding.ASCII.GetBytes(map.scripted_events[x].script_name).CopyTo(scripted_event_bytes, x * 0x2C + 14);
                }

                //Rising platforms
                for (int x = 0; x < map.rising_platforms.Count; x++)
                {
                    rising_platform_bytes[x + 0x4] = map.rising_platforms[x].id;
                    rising_platform_bytes[x + 0x4 + 1] = map.rising_platforms[x].unknown_1;
                    rising_platform_bytes[x + 0x4 + 2] = map.rising_platforms[x].unknown_2;
                    rising_platform_bytes[x + 0x4 + 3] = map.rising_platforms[x].unknown_3;
                }

                //Create the tile data header. This needs to be done after all the data is written so we know where our pointers go.
                for (int x = 0; x < tile_type_info_count; x++)
                {
                    BitConverter.GetBytes(map.tile_data[x].index).CopyTo(tile_type_data_bytes, x * 0x14);
                    BitConverter.GetBytes(map.tile_data[x].entries).CopyTo(tile_type_data_bytes, x * 0x14 + 4);
                    BitConverter.GetBytes(map.tile_data[x].entry_ptr).CopyTo(tile_type_data_bytes, x * 0x14 + 8);
                    BitConverter.GetBytes(map.tile_data[x].data_ptr).CopyTo(tile_type_data_bytes, x * 0x14 + 0xC);
                    BitConverter.GetBytes(map.tile_data[x].data_length).CopyTo(tile_type_data_bytes, x * 0x14 + 0x10);
                }
            }
        }
    }
}