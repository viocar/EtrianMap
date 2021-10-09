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
            MSBFile file = new MSBFile();

            //Create the header.
            MSBHeader header = new MSBHeader
            {
                map_x = BitConverter.ToInt32(sys, 0x8),
                map_y = BitConverter.ToInt32(sys, 0xC),
                behaviour_count = BitConverter.ToInt32(sys, 0x10),
                behaviour_pointer = BitConverter.ToInt32(sys, 0x14),
                tile_type_count = BitConverter.ToInt32(sys, 0x18),
                tile_type_pointer = BitConverter.ToInt32(sys, 0x1C),
                encounter_pointer = BitConverter.ToInt32(sys, 0x20)
            };
            file.header = header;

            //Now that we have the data in the header, we're going to create an entry for each tile so that we can track what is on which tile.
            for (int x = 0; x < header.map_x * header.map_y; x++)
            {
                MSBTileContainer container = new MSBTileContainer();
                for (int y = 0; y < header.behaviour_count; y++)
                {
                    bool tile = false;
                    container.behaviour.Add(tile);
                }
                file.containers.Add(container);
            }

            //Get all the behaviours.
            for (int x = 0; x < header.behaviour_count; x++) 
            {
                List<MSBBehaviours> section = new List<MSBBehaviours>();
                int ptr = BitConverter.ToInt32(sys, header.behaviour_pointer + (x * 0x4));
                for (int y = 0; y < (header.map_x * header.map_y); y++)
                {
                    MSBBehaviours tile = new MSBBehaviours
                    {
                        type = sys[ptr + (y * 2)],
                        id = sys[ptr + (y * 2) + 1]
                    };
                    section.Add(tile);
                    if (tile.type != 0 && tile.type != 0xA)
                    {
                        file.containers[y].behaviour[x] = true;
                    }
                }
                file.behaviour_tiles.Add(section);
            }

            //Get all the tile type info data.
            for (int x = 0; x < header.tile_type_count; x++)
            {
                int ptr = header.tile_type_pointer;
                MSBTileTypeInfo tile = new MSBTileTypeInfo
                {
                    index = BitConverter.ToUInt32(sys, ptr + (x * 0x14)),
                    entries = BitConverter.ToUInt32(sys, ptr + 4 + (x * 0x14)),
                    entry_ptr = BitConverter.ToUInt32(sys, ptr + 8 + (x * 0x14)),
                    data_ptr = BitConverter.ToUInt32(sys, ptr + 0xC + (x * 0x14)),
                    data_length = BitConverter.ToUInt32(sys, ptr + 0x10 + (x * 0x14))
                };
                file.tile_data.Add(tile);
            }

            //Get the tile type objects.
            foreach (MSBTileTypeInfo tile in file.tile_data) //This and the next section could be merged, but I'm keeping them separate for readability.
            {
                if (tile.entries > 0)
                {
                    for (int y = 0; y < tile.entries; y++)
                    {
                        MSBTileTypeObject obj = new MSBTileTypeObject
                        {
                            type = sys[tile.entry_ptr + y * 0x1C],
                            id = sys[tile.entry_ptr + y * 0x1C + 1],
                            graphic = sys[tile.entry_ptr + y * 0x1C + 2],
                            unknown_1 = sys[tile.entry_ptr + y * 0x1C + 3],
                            unknown_2 = sys[tile.entry_ptr + y * 0x1C + 4],
                            map_x = sys[tile.entry_ptr + y * 0x1C + 5],
                            map_y = sys[tile.entry_ptr + y * 0x1C + 6],
                            unknown_3 = sys[tile.entry_ptr + y * 0x1C + 7],
                            activation_direction_1 = sys[tile.entry_ptr + y * 0x1C + 8],
                            activation_direction_2 = sys[tile.entry_ptr + y * 0x1C + 9],
                            activation_direction_3 = sys[tile.entry_ptr + y * 0x1C + 0xA],
                            activation_direction_4 = sys[tile.entry_ptr + y * 0x1C + 0xB]
                        };
                        //This is 0x1C long! C# is just weird about consts in classes. Be careful with the indexing code later on.
                        file.tile_objects.Add(obj);
                        switch (obj.type)
                        {
                            case 0xD:
                                file.containers[obj.map_x + obj.map_y * header.map_y].doors = file.containers[obj.map_x + obj.map_y * header.map_y].doors++;
                                break;
                            case 0xE:
                                file.containers[obj.map_x + obj.map_y * header.map_y].stairs = file.containers[obj.map_x + obj.map_y * header.map_y].stairs++;
                                break;
                            case 0xF:
                                file.containers[obj.map_x + obj.map_y * header.map_y].chests = file.containers[obj.map_x + obj.map_y * header.map_y].chests++;
                                break;
                            case 0x10:
                                file.containers[obj.map_x + obj.map_y * header.map_y].two_ways= file.containers[obj.map_x + obj.map_y * header.map_y].two_ways++;
                                break;
                            case 0x11:
                                file.containers[obj.map_x + obj.map_y * header.map_y].one_ways = file.containers[obj.map_x + obj.map_y * header.map_y].one_ways++;
                                break;
                            case 0x12:
                                file.containers[obj.map_x + obj.map_y * header.map_y].doodads = file.containers[obj.map_x + obj.map_y * header.map_y].doodads++;
                                break;
                            case 0x13:
                                file.containers[obj.map_x + obj.map_y * header.map_y].pits = file.containers[obj.map_x + obj.map_y * header.map_y].pits++;
                                break;
                            case 0x14:
                                file.containers[obj.map_x + obj.map_y * header.map_y].poles = file.containers[obj.map_x + obj.map_y * header.map_y].poles++;
                                break;
                            case 0x15:
                                file.containers[obj.map_x + obj.map_y * header.map_y].unknown = file.containers[obj.map_x + obj.map_y * header.map_y].unknown++;
                                break;
                            case 0x16:
                                file.containers[obj.map_x + obj.map_y * header.map_y].currents = file.containers[obj.map_x + obj.map_y * header.map_y].currents++;
                                break;
                            case 0x17:
                                file.containers[obj.map_x + obj.map_y * header.map_y].snakes = file.containers[obj.map_x + obj.map_y * header.map_y].snakes++;
                                break;
                            case 0x18:
                                file.containers[obj.map_x + obj.map_y * header.map_y].scripts = file.containers[obj.map_x + obj.map_y * header.map_y].scripts++;
                                break;
                            case 0x19:
                                file.containers[obj.map_x + obj.map_y * header.map_y].risers = file.containers[obj.map_x + obj.map_y * header.map_y].risers++;
                                break;
                        }
                    }
                }
            }

            //Get all the tile data. This is fairly involved.
            foreach (MSBTileTypeInfo tile in file.tile_data)
            {
                switch (tile.index)
                {
                    case 0xE:
                        for (int x = 0; x < tile.entries; x++)
                        {
                            MSBStaircase staircase = new MSBStaircase
                            {
                                dest_floor = sys[tile.data_ptr + (x * tile.data_length)],
                                dest_x = sys[tile.data_ptr + (x * tile.data_length) + 1],
                                dest_y = sys[tile.data_ptr + (x * tile.data_length) + 2],
                                dest_facing = sys[tile.data_ptr + (x * tile.data_length) + 3],
                                sfx = sys[tile.data_ptr + (x * tile.data_length) + 4],
                                interact_message = sys[tile.data_ptr + (x * tile.data_length) + 5],
                                unknown_1 = sys[tile.data_ptr + (x * tile.data_length) + 6],
                                unknown_2 = sys[tile.data_ptr + (x * tile.data_length) + 7]
                            };
                            file.staircases.Add(staircase);
                        }
                        break;
                    case 0xF:
                        for (int x = 0; x < tile.entries; x++)
                        {
                            MSBChest chest = new MSBChest
                            {
                                is_item = sys[tile.data_ptr + (x * tile.data_length)],
                                unknown_1 = sys[tile.data_ptr + (x * tile.data_length) + 1],
                                unknown_2 = sys[tile.data_ptr + (x * tile.data_length) + 2],
                                unknown_3 = sys[tile.data_ptr + (x * tile.data_length) + 3],
                                value = BitConverter.ToUInt32(sys, (int)(tile.data_ptr + (x * tile.data_length) + 4)) //This is a long unless I cast it... why?
                            };
                            file.chests.Add(chest);
                        }
                        break;
                    case 0x10:
                        for (int x = 0; x < tile.entries; x++)
                        {
                            MSBTwoWayPassage two_way = new MSBTwoWayPassage
                            {
                                unknown_1 = sys[tile.data_ptr + (x * tile.data_length)],
                                unknown_2 = sys[tile.data_ptr + (x * tile.data_length) + 1],
                                unknown_3 = sys[tile.data_ptr + (x * tile.data_length) + 2],
                                unknown_4 = sys[tile.data_ptr + (x * tile.data_length) + 3]
                            };
                            file.two_way_passages.Add(two_way);
                        }
                        break;
                    case 0x11:
                        for (int x = 0; x < tile.entries; x++)
                        {
                            MSBOneWayPassage one_way = new MSBOneWayPassage
                            {
                                unknown_1 = sys[tile.data_ptr + (x * tile.data_length)],
                                unknown_2 = sys[tile.data_ptr + (x * tile.data_length) + 1],
                                unknown_3 = sys[tile.data_ptr + (x * tile.data_length) + 2],
                                unknown_4 = sys[tile.data_ptr + (x * tile.data_length) + 3]
                            };
                            file.one_way_passages.Add(one_way);
                        }
                        break;
                    case 0x12: //This is messy 
                        for (int x = 0; x < tile.entries; x++)
                        {
                            MSBDoodad doodad = new MSBDoodad
                            {
                                id_1 = sys[tile.data_ptr + (x * tile.data_length)],
                                unknown_1_1 = sys[tile.data_ptr + (x * tile.data_length) + 1],
                                unknown_2_1 = sys[tile.data_ptr + (x * tile.data_length) + 2],
                                unknown_3_1 = sys[tile.data_ptr + (x * tile.data_length) + 3],
                                id_2 = sys[tile.data_ptr + (x * tile.data_length) + 4],
                                unknown_1_2 = sys[tile.data_ptr + (x * tile.data_length) + 5],
                                unknown_2_2 = sys[tile.data_ptr + (x * tile.data_length) + 6],
                                unknown_3_2 = sys[tile.data_ptr + (x * tile.data_length) + 7],
                                id_3 = sys[tile.data_ptr + (x * tile.data_length) + 8],
                                unknown_1_3 = sys[tile.data_ptr + (x * tile.data_length) + 9],
                                unknown_2_3 = sys[tile.data_ptr + (x * tile.data_length) + 0xA],
                                unknown_3_3 = sys[tile.data_ptr + (x * tile.data_length) + 0xB],
                                id_4 = sys[tile.data_ptr + (x * tile.data_length) + 0xC],
                                unknown_1_4 = sys[tile.data_ptr + (x * tile.data_length) + 0xD],
                                unknown_2_4 = sys[tile.data_ptr + (x * tile.data_length) + 0xE],
                                unknown_3_4 = sys[tile.data_ptr + (x * tile.data_length) + 0xF]
                            };
                            file.doodads.Add(doodad);
                        }
                        break;
                    case 0x17:
                        for (int x = 0; x < tile.entries; x++)
                        {
                            MSBSnake snake = new MSBSnake
                            {
                                id = sys[tile.data_ptr + (x * tile.data_length)],
                                unknown_1 = sys[tile.data_ptr + (x * tile.data_length) + 1],
                                position = sys[tile.data_ptr + (x * tile.data_length) + 2],
                                unknown_2 = sys[tile.data_ptr + (x * tile.data_length) + 3]
                            };
                            file.snakes.Add(snake);
                        }
                        break;
                    case 0x18:
                        for (int x = 0; x < tile.entries; x++)
                        {
                            MSBScript script = new MSBScript
                            {
                                flag_1 = BitConverter.ToUInt16(sys, (int)(tile.data_ptr + (x * tile.data_length))),
                                flag_2 = BitConverter.ToUInt16(sys, (int)(tile.data_ptr + (x * tile.data_length) + 2)),
                                flag_3 = BitConverter.ToUInt16(sys, (int)(tile.data_ptr + (x * tile.data_length) + 4)),
                                required_flag = BitConverter.ToUInt16(sys, (int)(tile.data_ptr + (x * tile.data_length) + 6)),
                                unknown_1 = sys[tile.data_ptr + (x * tile.data_length) + 8],
                                unknown_2 = sys[tile.data_ptr + (x * tile.data_length) + 9],
                                unknown_3 = sys[tile.data_ptr + (x * tile.data_length) + 0xA],
                                unknown_4 = sys[tile.data_ptr + (x * tile.data_length) + 0xB],
                                unknown_5 = sys[tile.data_ptr + (x * tile.data_length) + 0xC],
                                unknown_6 = sys[tile.data_ptr + (x * tile.data_length) + 0xD],
                                unknown_7 = sys[tile.data_ptr + (x * tile.data_length) + 0xE],
                                unknown_8 = sys[tile.data_ptr + (x * tile.data_length) + 0xF],
                                prompt = sys[tile.data_ptr + (x * tile.data_length) + 0x10],
                                unknown_9 = sys[tile.data_ptr + (x * tile.data_length) + 0x11],
                                unknown_10 = BitConverter.ToUInt16(sys, (int)(tile.data_ptr + (x * tile.data_length) + 0x12))
                            };
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
                MSBCellEncounter enc = new MSBCellEncounter
                {
                    encounter_id = BitConverter.ToUInt16(sys, ptr + (x * 8)),
                    danger = BitConverter.ToUInt16(sys, ptr + 2 + (x * 8)),
                    unknown_1 = BitConverter.ToUInt16(sys, ptr + 4 + (x * 8)),
                    unknown_2 = BitConverter.ToUInt16(sys, ptr + 6 + (x * 8))
                };
                file.encounters.Add(enc);
            }
            return file;
        }
        private MGBFile BuildInitialGfxData()
        {
            //Initial setup.
            byte[] gfx = globals.mapdat_list[globals.open_map].gfx_file;
            MGBFile file = new MGBFile();

            //Create header.
            MGBHeader header = new MGBHeader
            {
                map_x = BitConverter.ToInt32(gfx, 0x8),
                map_y = BitConverter.ToInt32(gfx, 0xC),
                layer_count = BitConverter.ToInt32(gfx, 0x10),
                layer_pointer = BitConverter.ToInt32(gfx, 0x14),
                filename_count = BitConverter.ToInt32(gfx, 0x18),
                filename_pointer = BitConverter.ToInt32(gfx, 0x1C)
            };
            file.header = header;

            //Populate all layers. 
            for (int x = 0; x < header.layer_count; x++)
            {
                List<MGBTile> layer = new List<MGBTile>();
                int ptr = BitConverter.ToInt32(gfx, header.layer_pointer + (x * 0x4));
                for (int y = 0; y < (header.map_x * header.map_y); y++)
                {
                    MGBTile tile = new MGBTile
                    {
                        id = gfx[ptr + (y * 4)],
                        rotation = gfx[ptr + ((y * 4) + 1)],
                        unknown_1 = gfx[ptr + ((y * 4) + 2)],
                        unknown_2 = gfx[ptr + ((y * 4) + 3)]
                    };
                    layer.Add(tile);
                }
                file.layer_tiles.Add(layer);
            }

            //Add the filename section.
            for (int x = 0; x < header.filename_count; x++)
            {
                byte[] str = new byte[0x80]; //We need to pack up these bytes first before we create the filename index.
                for (int y = 0; y < 4; y++)
                {
                    for (int z = 0; z < 0x20; z++)
                    {
                        str[(y * 0x20) + z] = gfx[header.filename_pointer + (x * 0x94) + 0x4 + z + (y * 0x20)];
                    }
                }
                MGBFileIndex index = new MGBFileIndex
                {
                    index = BitConverter.ToUInt32(gfx, header.filename_pointer + (x * 0x94)),
                    file_1 = Encoding.ASCII.GetString(str, 0, 0x20),
                    file_2 = Encoding.ASCII.GetString(str, 0x20, 0x20),
                    file_3 = Encoding.ASCII.GetString(str, 0x40, 0x20),
                    file_4 = Encoding.ASCII.GetString(str, 0x60, 0x20),
                    file_1_rotation = gfx[header.filename_pointer + (x * 0x94) + 0x84],
                    file_2_rotation = gfx[header.filename_pointer + (x * 0x94) + 0x85],
                    file_3_rotation = gfx[header.filename_pointer + (x * 0x94) + 0x86],
                    file_4_rotation = gfx[header.filename_pointer + (x * 0x94) + 0x87],
                    //Values 0x88 through 0x90 are explicitly set to 0 by the game code. They may be used once the map is in memory, but they have no relevance to the editor.
                    unknown_1 = gfx[header.filename_pointer + (x * 0x94) + 0x91],
                    unknown_2 = gfx[header.filename_pointer + (x * 0x94) + 0x92],
                    unknown_3 = gfx[header.filename_pointer + (x * 0x94) + 0x93]
                };
                file.indices.Add(index);
                globals.gfx_filenames.Add(index.file_1);
            }

            return file;
        }
        private void SaveFile()
        {
            //Initialize variables for easy use.
            MSBFile sys = globals.sys_data;
            MGBFile gfx = globals.gfx_data;
            string sys_path = globals.mapdat_list[globals.open_map].base_path + globals.mapdat_list[globals.open_map].sys_filename;
            string gfx_path = globals.mapdat_list[globals.open_map].base_path + globals.mapdat_list[globals.open_map].gfx_filename;
            string dungeonname = globals.open_path + "\\InterfaceFile\\dungeonname.mbm";
            string encount = globals.open_path + "\\Dungeon\\encount.tbl";
            string encount_group = globals.open_path + "\\Dungeon\\encount_group.tbl";
            string floor = globals.open_path + "\\Dungeon\\floor.tbl";
            string mapdat = globals.open_path + "\\MapDat\\";
            if (File.Exists(encount) && File.Exists(encount_group) && File.Exists(floor) && File.Exists(dungeonname) && File.Exists(mapdat + "\\sys01.msb") && File.Exists(mapdat + "\\gfx01.mgb"))
            {
                //Start with the MSB.
                int behaviours = sys.header.behaviour_count;
                int tile_type_info_count = sys.header.tile_type_count;

                //We need to know how big the file will be. While most things are constant, there is some variability.
                int sys_header_size = 0x2C; //Technically, this should be const, but I'm going to leave it as-is for consistency.
                int behaviour_ptr_size = behaviours * 0x4; //Number of behaviour pointers
                int total_behaviour_size = behaviours * sys.header.map_x * sys.header.map_y * 0x2; //Number of tile behaviours in the file
                int tile_type_count_size = sys.header.tile_type_count * 0x14; //Number of tile types for the tile type data section
                int tile_type_data_size = 0;
                foreach (MSBTileTypeInfo tile in sys.tile_data) //Iterate through the tile type info to figure out how much space we need to allocate for those
                {
                    tile_type_data_size += (int)(tile.entries * 0x1C); //Object entry
                    tile_type_data_size += (int)(tile.entries * tile.data_length); //Data entry
                }
                int garbage_size = 0x68; //Same as header size...
                int encounter_size = sys.header.map_x * sys.header.map_y * 0x8; //Number of tile encounters in the file
                int sys_size = behaviour_ptr_size + total_behaviour_size + tile_type_count_size + tile_type_data_size + encounter_size + sys_header_size + garbage_size; //Add all sizes together.
                byte[] sys_save_byte = new byte[sys_size];

                //Create arrays for data. We could just write it all directly into sys_save_byte, but this intermediate step makes it easier with different data sizes.
                int[] sys_header = new int[sys_header_size / 4];
                int[] behaviour_pointers = new int[behaviours];
                byte[] behaviour_bytes = new byte[total_behaviour_size]; //All the behaviours can be listed sequentially in here.
                byte[] tile_type_data_bytes = new byte[tile_type_count_size];
                Dictionary<int, List<byte[]>> tile_type_object_list = new Dictionary<int, List<byte[]>>();
                byte[] staircase_bytes = new byte[sys.staircases.Count * 8];
                byte[] chest_bytes = new byte[sys.chests.Count * 8];
                byte[] two_way_bytes = new byte[sys.two_way_passages.Count * 4];
                byte[] one_way_bytes = new byte[sys.one_way_passages.Count * 4];
                byte[] doodad_bytes = new byte[sys.doodads.Count * 0x10];
                byte[] snake_bytes = new byte[sys.snakes.Count * 4];
                byte[] scripted_event_bytes = new byte[sys.scripted_events.Count * 4];
                byte[] rising_platform_bytes = new byte[sys.rising_platforms.Count * 4];
                ushort[] encounter_bytes = new ushort[sys.encounters.Count * 4];

                //Populate the header.
                sys_header[0] = MSBHeader.MAGIC; //I can't do sys.header.MAGIC due to a quirk in C#
                sys_header[1] = MSBHeader._0x4;
                sys_header[2] = sys.header.map_x;
                sys_header[3] = sys.header.map_y;
                sys_header[4] = sys.header.behaviour_count;
                sys_header[5] = sys.header.behaviour_pointer;
                sys_header[6] = sys.header.tile_type_count;
                sys_header[7] = sys.header.tile_type_pointer;
                sys_header[8] = sys.header.encounter_pointer;
                sys_header[9] = MSBHeader._0x24;
                sys_header[10] = MSBHeader._0x28;


                //Create the pointer list for each behaviour.
                for (int x = 0; x < behaviours; x++)
                {
                    behaviour_pointers[x] = sys_header_size + (behaviours * 0x4) + (x * sys.header.map_x * sys.header.map_y * 2);
                }

                //Pack the data for each behaviour.
                for (int x = 0; x < behaviours; x++)
                {
                    for (int y = 0; y < sys.header.map_x * sys.header.map_y; y++)
                    {
                        behaviour_bytes[(x * sys.header.map_x * sys.header.map_y * 2) + (y * 2)] = sys.behaviour_tiles[x][y].type;
                        behaviour_bytes[(x * sys.header.map_x * sys.header.map_y * 2) + (y * 2) + 1] = sys.behaviour_tiles[x][y].id;
                    }
                }

                //Create the tile data header. This needs to be done after all the data is written so we know where our pointers go.
                int rolling_entry_ptr = sys_header_size + behaviour_ptr_size + total_behaviour_size + tile_type_count_size + garbage_size; //Should always be 0x54CC for EON
                for (int x = 0; x < tile_type_info_count; x++)
                {
                    BitConverter.GetBytes(sys.tile_data[x].index).CopyTo(tile_type_data_bytes, x * 0x14);
                    BitConverter.GetBytes(sys.tile_data[x].entries).CopyTo(tile_type_data_bytes, x * 0x14 + 4);
                    if (sys.tile_data[x].entries == 0)
                    {
                        BitConverter.GetBytes(0).CopyTo(tile_type_data_bytes, x * 0x14 + 8);
                        BitConverter.GetBytes(0).CopyTo(tile_type_data_bytes, x * 0x14 + 0xC);
                    }
                    else
                    {
                        BitConverter.GetBytes(rolling_entry_ptr).CopyTo(tile_type_data_bytes, x * 0x14 + 8);
                        rolling_entry_ptr = (int)(rolling_entry_ptr + sys.tile_data[x].entries * 0x1C); //Need to calculate where the next offset goes. This is for this tile type's data section.
                        BitConverter.GetBytes(rolling_entry_ptr).CopyTo(tile_type_data_bytes, x * 0x14 + 0xC);
                        rolling_entry_ptr = (int)(rolling_entry_ptr + sys.tile_data[x].entries * sys.tile_data[x].data_length); //Next, calculate how long the data section is. This is for the next iteration's object.
                    }
                    BitConverter.GetBytes(sys.tile_data[x].data_length).CopyTo(tile_type_data_bytes, x * 0x14 + 0x10);
                }

                //Pack all the tile type objects. These are consistent across all tile types, but we want to neatly separate them as we can't just cluster them all together like other data types.
                for (int x = 0; x < sys.tile_objects.Count; x++)
                {
                    byte type = sys.tile_objects[x].type;
                    if (!tile_type_object_list.ContainsKey(type))
                    {
                        List<byte[]> obj_list = new List<byte[]>();
                        tile_type_object_list.Add(type, obj_list);
                    }
                    byte[] tile_type_object_bytes = new byte[0x1C];
                    tile_type_object_bytes[0] = type;
                    tile_type_object_bytes[1] = sys.tile_objects[x].id;
                    tile_type_object_bytes[2] = sys.tile_objects[x].graphic;
                    tile_type_object_bytes[3] = sys.tile_objects[x].unknown_1;
                    tile_type_object_bytes[4] = sys.tile_objects[x].unknown_2;
                    tile_type_object_bytes[5] = sys.tile_objects[x].map_x;
                    tile_type_object_bytes[6] = sys.tile_objects[x].map_y;
                    tile_type_object_bytes[7] = sys.tile_objects[x].unknown_3;
                    tile_type_object_bytes[8] = sys.tile_objects[x].activation_direction_1;
                    tile_type_object_bytes[9] = sys.tile_objects[x].activation_direction_2;
                    tile_type_object_bytes[0xA] = sys.tile_objects[x].activation_direction_3;
                    tile_type_object_bytes[0xB] = sys.tile_objects[x].activation_direction_4;
                    BitConverter.GetBytes(MSBTileTypeObject.UNUSED_1).CopyTo(tile_type_object_bytes, 0xC);
                    BitConverter.GetBytes(MSBTileTypeObject.UNUSED_2).CopyTo(tile_type_object_bytes, 0x14);
                    tile_type_object_list[type].Add(tile_type_object_bytes);
                }

                //Pack each data type. 
                //Staircases
                for (int x = 0; x < sys.staircases.Count; x++)
                {
                    staircase_bytes[x * 0x8] = sys.staircases[x].dest_floor;
                    staircase_bytes[x * 0x8 + 1] = sys.staircases[x].dest_x;
                    staircase_bytes[x * 0x8 + 2] = sys.staircases[x].dest_y;
                    staircase_bytes[x * 0x8 + 3] = sys.staircases[x].dest_facing;
                    staircase_bytes[x * 0x8 + 4] = sys.staircases[x].sfx;
                    staircase_bytes[x * 0x8 + 5] = sys.staircases[x].interact_message;
                    staircase_bytes[x * 0x8 + 6] = sys.staircases[x].unknown_1;
                    staircase_bytes[x * 0x8 + 7] = sys.staircases[x].unknown_2;
                }

                //Chests
                for (int x = 0; x < sys.chests.Count; x++)
                {
                    chest_bytes[x * 0x8] = sys.chests[x].is_item;
                    chest_bytes[x * 0x8 + 1] = sys.chests[x].unknown_1;
                    chest_bytes[x * 0x8 + 2] = sys.chests[x].unknown_2;
                    chest_bytes[x * 0x8 + 3] = sys.chests[x].unknown_3;
                    BitConverter.GetBytes(sys.chests[x].value).CopyTo(chest_bytes, x * 0x8 + 4);
                }

                //Two-way passages
                for (int x = 0; x < sys.two_way_passages.Count; x++)
                {
                    two_way_bytes[x * 0x4] = sys.two_way_passages[x].unknown_1;
                    two_way_bytes[x * 0x4 + 1] = sys.two_way_passages[x].unknown_2;
                    two_way_bytes[x * 0x4 + 2] = sys.two_way_passages[x].unknown_3;
                    two_way_bytes[x * 0x4 + 3] = sys.two_way_passages[x].unknown_4;
                }

                //One-way passages
                for (int x = 0; x < sys.one_way_passages.Count; x++)
                {
                    one_way_bytes[x * 0x4] = sys.one_way_passages[x].unknown_1;
                    one_way_bytes[x * 0x4 + 1] = sys.one_way_passages[x].unknown_2;
                    one_way_bytes[x * 0x4 + 2] = sys.one_way_passages[x].unknown_3;
                    one_way_bytes[x * 0x4 + 3] = sys.one_way_passages[x].unknown_4;
                }

                //Doodads
                for (int x = 0; x < sys.doodads.Count; x++)
                {
                    doodad_bytes[x * 0x10] = sys.doodads[x].id_1;
                    doodad_bytes[x * 0x10 + 1] = sys.doodads[x].unknown_1_1;
                    doodad_bytes[x * 0x10 + 2] = sys.doodads[x].unknown_2_1;
                    doodad_bytes[x * 0x10 + 3] = sys.doodads[x].unknown_3_1;
                    doodad_bytes[x * 0x10 + 4] = sys.doodads[x].id_2;
                    doodad_bytes[x * 0x10 + 5] = sys.doodads[x].unknown_1_2;
                    doodad_bytes[x * 0x10 + 6] = sys.doodads[x].unknown_2_2;
                    doodad_bytes[x * 0x10 + 7] = sys.doodads[x].unknown_3_2;
                    doodad_bytes[x * 0x10 + 8] = sys.doodads[x].id_3;
                    doodad_bytes[x * 0x10 + 9] = sys.doodads[x].unknown_1_3;
                    doodad_bytes[x * 0x10 + 0xA] = sys.doodads[x].unknown_2_3;
                    doodad_bytes[x * 0x10 + 0xB] = sys.doodads[x].unknown_3_3;
                    doodad_bytes[x * 0x10 + 0xC] = sys.doodads[x].id_4;
                    doodad_bytes[x * 0x10 + 0xD] = sys.doodads[x].unknown_1_4;
                    doodad_bytes[x * 0x10 + 0xE] = sys.doodads[x].unknown_2_4;
                    doodad_bytes[x * 0x10 + 0xF] = sys.doodads[x].unknown_3_4;
                }

                //Snakes
                for (int x = 0; x < sys.snakes.Count; x++)
                {
                    snake_bytes[x * 0x4] = sys.snakes[x].id;
                    snake_bytes[x * 0x4 + 1] = sys.snakes[x].unknown_1;
                    snake_bytes[x * 0x4 + 2] = sys.snakes[x].position;
                    snake_bytes[x * 0x4 + 3] = sys.snakes[x].unknown_2;
                }

                //Scripted events
                for (int x = 0; x < sys.scripted_events.Count; x++)
                {
                    BitConverter.GetBytes(sys.scripted_events[x].flag_1).CopyTo(scripted_event_bytes, x * 0x2C);
                    BitConverter.GetBytes(sys.scripted_events[x].flag_2).CopyTo(scripted_event_bytes, x * 0x2C + 2);
                    BitConverter.GetBytes(sys.scripted_events[x].flag_3).CopyTo(scripted_event_bytes, x * 0x2C + 4);
                    BitConverter.GetBytes(sys.scripted_events[x].required_flag).CopyTo(scripted_event_bytes, x * 0x2C + 6);
                    scripted_event_bytes[x + 0x2C + 8] = sys.scripted_events[x].unknown_1;
                    scripted_event_bytes[x + 0x2C + 9] = sys.scripted_events[x].unknown_2;
                    scripted_event_bytes[x + 0x2C + 0xA] = sys.scripted_events[x].unknown_3;
                    scripted_event_bytes[x + 0x2C + 0xB] = sys.scripted_events[x].unknown_4;
                    scripted_event_bytes[x + 0x2C + 0xC] = sys.scripted_events[x].unknown_5;
                    scripted_event_bytes[x + 0x2C + 0xD] = sys.scripted_events[x].unknown_6;
                    scripted_event_bytes[x + 0x2C + 0xE] = sys.scripted_events[x].unknown_7;
                    scripted_event_bytes[x + 0x2C + 0xF] = sys.scripted_events[x].unknown_8;
                    scripted_event_bytes[x + 0x2C + 0x10] = sys.scripted_events[x].prompt;
                    scripted_event_bytes[x + 0x2C + 0x11] = sys.scripted_events[x].unknown_9;
                    BitConverter.GetBytes(sys.scripted_events[x].unknown_10).CopyTo(scripted_event_bytes, x * 0x2C + 12);
                    Encoding.ASCII.GetBytes(sys.scripted_events[x].script_name).CopyTo(scripted_event_bytes, x * 0x2C + 14);
                }

                //Rising platforms
                for (int x = 0; x < sys.rising_platforms.Count; x++)
                {
                    rising_platform_bytes[x + 0x4] = sys.rising_platforms[x].id;
                    rising_platform_bytes[x + 0x4 + 1] = sys.rising_platforms[x].unknown_1;
                    rising_platform_bytes[x + 0x4 + 2] = sys.rising_platforms[x].unknown_2;
                    rising_platform_bytes[x + 0x4 + 3] = sys.rising_platforms[x].unknown_3;
                }

                //Finally, we handle encounters. This is the last part of the file.
                for (int x = 0; x < sys.encounters.Count; x++)
                {
                    encounter_bytes[x * 0x4] = sys.encounters[x].encounter_id;
                    encounter_bytes[x * 0x4 + 1] = sys.encounters[x].danger;
                    encounter_bytes[x * 0x4 + 2] = sys.encounters[x].unknown_1;
                    encounter_bytes[x * 0x4 + 3] = sys.encounters[x].unknown_2;
                }

                //After getting all the bytes in order, we need to build it into a file. It is fairly straightforward to just pour it all in, but we have to keep track of the offset.
                int sys_ptr = 0;
                Buffer.BlockCopy(sys_header, 0, sys_save_byte, sys_ptr, sys_header.Length * 0x4);
                sys_ptr += sys_header.Length * 0x4;
                Buffer.BlockCopy(behaviour_pointers, 0, sys_save_byte, sys_ptr, behaviour_pointers.Length * 0x4);
                sys_ptr += behaviour_pointers.Length * 0x4;
                Buffer.BlockCopy(behaviour_bytes, 0, sys_save_byte, sys_ptr, behaviour_bytes.Length);
                sys_ptr += behaviour_bytes.Length;
                Buffer.BlockCopy(tile_type_data_bytes, 0, sys_save_byte, sys_ptr, tile_type_data_bytes.Length);
                sys_ptr += tile_type_data_bytes.Length;
                Buffer.BlockCopy(sys.garbage, 0, sys_save_byte, sys_ptr, sys.garbage.Length);
                sys_ptr += sys.garbage.Length;
                //We have to do a little more now with the lists to make sure everything is in the right order.
                for (int x = 0; x < sys.header.tile_type_count; x++)
                {
                    if (tile_type_object_list.ContainsKey(x))
                    {
                        for (int y = 0; y < tile_type_object_list[x].Count; y++)
                        {
                            Buffer.BlockCopy(tile_type_object_list[x][y], 0, sys_save_byte, sys_ptr, tile_type_object_list[x][y].Length);
                            sys_ptr += tile_type_object_list[x][y].Length;
                        }
                    }
                    switch (x)
                    {
                        case 0xE:
                            Buffer.BlockCopy(staircase_bytes, 0, sys_save_byte, sys_ptr, staircase_bytes.Length);
                            sys_ptr += staircase_bytes.Length;
                            break;
                        case 0xF:
                            Buffer.BlockCopy(chest_bytes, 0, sys_save_byte, sys_ptr, chest_bytes.Length);
                            sys_ptr += chest_bytes.Length;
                            break;
                        case 0x10:
                            Buffer.BlockCopy(two_way_bytes, 0, sys_save_byte, sys_ptr, two_way_bytes.Length);
                            sys_ptr += two_way_bytes.Length;
                            break;
                        case 0x11:
                            Buffer.BlockCopy(one_way_bytes, 0, sys_save_byte, sys_ptr, one_way_bytes.Length);
                            sys_ptr += one_way_bytes.Length;
                            break;
                        case 0x12:
                            Buffer.BlockCopy(doodad_bytes, 0, sys_save_byte, sys_ptr, doodad_bytes.Length);
                            sys_ptr += doodad_bytes.Length;
                            break;
                        case 0x17:
                            Buffer.BlockCopy(snake_bytes, 0, sys_save_byte, sys_ptr, snake_bytes.Length);
                            sys_ptr += snake_bytes.Length;
                            break;
                        case 0x18:
                            Buffer.BlockCopy(scripted_event_bytes, 0, sys_save_byte, sys_ptr, scripted_event_bytes.Length);
                            sys_ptr += scripted_event_bytes.Length;
                            break;
                        case 0x19:
                            Buffer.BlockCopy(rising_platform_bytes, 0, sys_save_byte, sys_ptr, rising_platform_bytes.Length);
                            sys_ptr += rising_platform_bytes.Length;
                            break;
                        default:
                            break;
                    }
                }
                //Finally, the encounter bytes.
                Buffer.BlockCopy(encounter_bytes, 0, sys_save_byte, sys_ptr, encounter_bytes.Length * 0x2);

                //Write to SYS file.
                Debug.WriteLine(sys_path);
                File.WriteAllBytes(sys_path + "test", sys_save_byte);
                //Debug.WriteLine("File writing disabled!");

                //Next, the MGB. A similar process.
                int layers = gfx.header.layer_count;
                int indices = gfx.header.filename_count;

                //Calculate filesize.
                int gfx_header_size = 0x70; //Most of this space is unused.
                int layer_ptr_size = layers * 0x4; //Number of layer pointers.
                int total_layer_size = layers * gfx.header.map_x * gfx.header.map_y * 0x4; //Number of tiles times the number of layers times four.
                int filename_size = indices * 0x94; //Number of tile file name indices in the file.
                int gfx_size = gfx_header_size + layer_ptr_size + total_layer_size + filename_size; //Add all sizes together.
                byte[] gfx_save_byte = new byte[gfx_size];
                Debug.WriteLine(Convert.ToString(layer_ptr_size, 16));
                Debug.WriteLine(Convert.ToString(total_layer_size, 16));
                Debug.WriteLine(Convert.ToString(filename_size, 16));
                Debug.WriteLine(Convert.ToString(gfx_size, 16));

                //Create the header array.
                int[] gfx_header = new int[gfx_header_size / 4];
                gfx_header[0] = MGBHeader.MAGIC;
                gfx_header[1] = MGBHeader._0x4;
                gfx_header[2] = gfx.header.map_x;
                gfx_header[3] = gfx.header.map_y;
                gfx_header[4] = gfx.header.layer_count;
                gfx_header[5] = gfx.header.layer_pointer;
                gfx_header[6] = gfx.header.filename_count;
                gfx_header[7] = gfx.header.filename_pointer;
                for (int x = 0; x < 0x14; x++) //These values are always 0.
                {
                    gfx_header[8 + x] = 0;
                }

                //Pointer array.
                int[] gfx_layer_pointer_bytes = new int[gfx.header.layer_count];
                for (int x = 0; x < gfx.header.layer_count; x++)
                {
                    gfx_layer_pointer_bytes[x] = gfx_header_size + (layers * 0x4) + (x * gfx.header.map_x * gfx.header.map_y * 4);
                }

                byte[] layer_bytes = new byte[layers * gfx.header.map_x * gfx.header.map_y * 4];
                //Copy the layer data in.
                for (int x = 0; x < layers; x++)
                {
                    for (int y = 0; y < gfx.header.map_x * gfx.header.map_y; y++)
                    {
                        layer_bytes[(x * gfx.header.map_x * gfx.header.map_y * 4) + (y * 4)] = gfx.layer_tiles[x][y].id;
                        layer_bytes[(x * gfx.header.map_x * gfx.header.map_y * 4) + (y * 4) + 1] = gfx.layer_tiles[x][y].rotation;
                        layer_bytes[(x * gfx.header.map_x * gfx.header.map_y * 4) + (y * 4) + 2] = gfx.layer_tiles[x][y].unknown_1;
                        layer_bytes[(x * gfx.header.map_x * gfx.header.map_y * 4) + (y * 4) + 3] = gfx.layer_tiles[x][y].unknown_2;
                    }
                }

                byte[] filename_bytes = new byte[indices * 0x94];
                //Copy in the filename data. Due to inconsistent leftovers in the base game files, we won't be able to make byte-accurate copies of the original data.
                for (int x = 0; x < indices; x++)
                {
                    BitConverter.GetBytes(gfx.indices[x].index).CopyTo(filename_bytes, x * 0x94);
                    Encoding.ASCII.GetBytes(gfx.indices[x].file_1.PadRight(0x20, '\x0')).CopyTo(filename_bytes, x * 0x94 + 4);
                    Encoding.ASCII.GetBytes(gfx.indices[x].file_2.PadRight(0x20, '\x0')).CopyTo(filename_bytes, x * 0x94 + 0x24);
                    Encoding.ASCII.GetBytes(gfx.indices[x].file_3.PadRight(0x20, '\x0')).CopyTo(filename_bytes, x * 0x94 + 0x44);
                    Encoding.ASCII.GetBytes(gfx.indices[x].file_4.PadRight(0x20, '\x0')).CopyTo(filename_bytes, x * 0x94 + 0x64);
                    filename_bytes[x * 0x94 + 0x84] = gfx.indices[x].file_1_rotation;
                    filename_bytes[x * 0x94 + 0x85] = gfx.indices[x].file_2_rotation;
                    filename_bytes[x * 0x94 + 0x86] = gfx.indices[x].file_3_rotation;
                    filename_bytes[x * 0x94 + 0x87] = gfx.indices[x].file_4_rotation;
                    for (int y = 0; x < 9; y++) //Set to 0 by the game, so these values are irrelevant in the editor. This will cause some byte-accuracy to be lost, but it's fine.
                    {
                        filename_bytes[x * 0x94 + 0x88 + y] = 0;
                    }
                    filename_bytes[x * 0x94 + 0x91] = gfx.indices[x].unknown_1;
                    filename_bytes[x * 0x94 + 0x92] = gfx.indices[x].unknown_2;
                    filename_bytes[x * 0x94 + 0x93] = gfx.indices[x].unknown_3;
                }

                //Build the file.
                int gfx_ptr = 0;
                Buffer.BlockCopy(gfx_header, 0, gfx_save_byte, gfx_ptr, gfx_header.Length * 4);
                gfx_ptr += gfx_header.Length * 4;
                Buffer.BlockCopy(gfx_layer_pointer_bytes, 0, gfx_save_byte, gfx_ptr, gfx_layer_pointer_bytes.Length * 4);
                gfx_ptr += gfx_layer_pointer_bytes.Length * 4;
                Buffer.BlockCopy(layer_bytes, 0, gfx_save_byte, gfx_ptr, layer_bytes.Length);
                gfx_ptr += layer_bytes.Length;
                Buffer.BlockCopy(filename_bytes, 0, gfx_save_byte, gfx_ptr, filename_bytes.Length);

                //Write to GFX file.
                Debug.WriteLine(gfx_path);
                File.WriteAllBytes(gfx_path + "test", gfx_save_byte);
                //Debug.WriteLine("File writing disabled!");
            }
        }
    }
}
