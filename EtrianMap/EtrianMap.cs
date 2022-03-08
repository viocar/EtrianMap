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
        //Constants that are used in the main form's UI.
        readonly string[] SYS_CONTROLS = { "Tiles", "Objects", "Tile Data", "Encounters" }; //Is there somewhere else I should be putting this code?
        readonly string[] SYS_TILES_CONTROLS = { "Layer 1", "Layer 2", "Layer 3", "Layer 4", "Layer 5", "Layer 6", "Layer 7", "Layer 8", "Layer 9", "Layer 10", };
        readonly string[] SYS_OBJECTS_CONTROLS = { "Doors", "Staircases", "Chests", "Two-way passages", "One-way passages", "Doodads", "Pits", "Geomagnetic poles", "Unknown", "Currents", "Moving platforms", "Scripted events", "Rising platforms" };
        readonly string[] SYS_ENCOUNTERS_CONTROLS = { "Encounter groups", "Danger" };
        readonly string[] GFX_CONTROLS = { "Layers", "Filenames" };
        readonly string[] GFX_LAYERS_CONTROLS = { "Layer 1", "Layer 2", "Layer 3", "Layer 4", "Layer 5", "Layer 6", "Layer 7", "Layer 8", "Layer 9", "Layer 10", };
        enum SYS_SELECTIONS //I won't be using enums for the subselections because they are data-complete, while the main selections could be broken out into further subdivisions.
        {
            Tiles,
            Objects,
            TileData,
            Encounters
        }
        enum GFX_SELECTIONS
        {
            Layers,
            Filenames
        }
        enum SYS_TILES_SELECTIONS
        {
            Doors,
            Staircases,
            Chests,
            TwoWays,
            OneWays,
            Doodads,
            Pits,
            Poles,
            Unknown,
            Currents,
            MovingPlatforms,
            ScriptedEvents,
            RisingPlatforms
        }
        enum VALIDATOR_TYPES
        {
            u8,
            s8,
            u16,
            s16,
            u32,
            s32,
            str,
        }
        const int COLUMN_WIDTH_NARROW = 45;
        const int COLUMN_WIDTH_MEDIUM = 80;
        const int COLUMN_WIDTH_WIDE = 115;

        //Initial program setup.
        readonly Globals globals = new Globals();

        public EtrianMap(List<byte[]> binaries, List<Table> tables, List<MBM> mbms, List<MapDatCollection> mapdat_list, string open_path)
        {
            int open_map = MapSelectDialog(mapdat_list);
            InitializeComponent();
            if (open_map > -1)
            {
                globals.binaries = binaries;
                globals.tables = tables;
                globals.mbms = mbms;
                globals.mapdat_list = mapdat_list;
                globals.open_path = open_path;
                globals.open_map = open_map;
                this.Text = "EtrianMap - " + globals.open_path + "\\" + globals.mapdat_list[open_map].sys_filename;
                globals.sys_data = BuildInitialMapData();
                globals.gfx_data = BuildInitialGfxData();
                globals.encounts = BuildEncountList();
                foreach (KeyValuePair<int, int> x in globals.encounts)
                {
                    Debug.WriteLine(x);
                }
                globals.map_area = new Rectangle(
                    MapRender.LEFT_EDGE,
                    MapRender.TOP_EDGE,
                    (MapRender.BOX_WIDTH + MapRender.LINE_THICKNESS) * globals.sys_data.header.map_x,
                    (MapRender.BOX_HEIGHT + MapRender.LINE_THICKNESS) * globals.sys_data.header.map_y
                );
                rb_Sys.Checked = true;
                cb_Type.DataSource = SYS_CONTROLS;
                cb_Type.BindingContext = new BindingContext();
                cb_Type.SelectedIndex = 0;

            }
        }

        //Save the map.
        private void Save_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        //This dialogue prompts the user to select a map.
        private int MapSelectDialog(List<MapDatCollection> mapdat_list)
        {
            using (MapSelector MapSelector = new MapSelector(mapdat_list))
            {
                if (MapSelector.ShowDialog() == DialogResult.OK)
                {
                    return MapSelector.MapSelected;
                }
                else
                {
                    return -1;
                }
            }
        }

        //Switch the UI.
        private void rb_Sys_CheckedChanged(object sender, EventArgs e)
        {
            cb_Type.DataSource = SYS_CONTROLS;
            cb_Type.BindingContext = new BindingContext();
            cb_Subtype.DataSource = SYS_TILES_CONTROLS;
            cb_Subtype.BindingContext = new BindingContext();
            Invalidate();
        }

        //Switch the UI.
        private void rb_Gfx_CheckedChanged(object sender, EventArgs e)
        {
            cb_Type.DataSource = GFX_CONTROLS;
            cb_Type.BindingContext = new BindingContext();
            cb_Subtype.DataSource = GFX_LAYERS_CONTROLS;
            cb_Subtype.BindingContext = new BindingContext();
            Invalidate();
        }

        //This handles the dropdown lists to change the DataGridView.
        private void cb_Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rb_Sys.Checked) //The GFX file has only one subtype menu so there's no need to switch here.
            {
                if (cb_Type.SelectedIndex == (int)SYS_SELECTIONS.Tiles)
                {
                    cb_Subtype.DataSource = SYS_TILES_CONTROLS;
                    cb_Subtype.BindingContext = new BindingContext();
                }
                else if (cb_Type.SelectedIndex == (int)SYS_SELECTIONS.Objects)
                {
                    cb_Subtype.DataSource = SYS_OBJECTS_CONTROLS;
                    cb_Subtype.BindingContext = new BindingContext();
                }
                else if (cb_Type.SelectedIndex == (int)SYS_SELECTIONS.Encounters)
                {
                    cb_Subtype.DataSource = SYS_ENCOUNTERS_CONTROLS;
                    cb_Subtype.BindingContext = new BindingContext();
                }
            }
            BuildDataGridView();
            Invalidate();
        }

        //And this is for the subtype.
        private void cb_Subtype_SelectedIndexChanged(object sender, EventArgs e)
        {
            BuildDataGridView();
            Invalidate();
        }

        //Handle clicks on the map.
        private void EtrianMap_MouseClick(object sender, MouseEventArgs e) //This also runs on MouseDoubleClick since this didn't fire if I double clicked. What is the better way?
        {
            var mouse_pos = PointToClient(Cursor.Position);
            if (globals.map_area.Contains(new Point(mouse_pos.X, mouse_pos.Y)))
            {
                if (ModifierKeys != Keys.Control)
                {
                    if ((rb_Sys.Checked && (cb_Type.SelectedIndex == (int)SYS_SELECTIONS.Tiles || cb_Type.SelectedIndex == (int)SYS_SELECTIONS.Encounters)) || (rb_Gfx.Checked && (cb_Type.SelectedIndex == (int)GFX_SELECTIONS.Layers)))
                    {
                        dgv_Data.Rows.Clear(); //SYS Tiles, SYS Encounters, and GFX Layers should not be cleared here.
                    }
                    globals.selected_box.Clear(); //Make sure we clear the rows before clearing the selection boxes or else we'll mess it up.
                    globals.selected_box_x.Clear();
                    globals.selected_box_y.Clear();
                }
                int box_x = (mouse_pos.X - MapRender.LEFT_EDGE) / (MapRender.BOX_WIDTH + MapRender.LINE_THICKNESS);
                int box_y = (mouse_pos.Y - MapRender.TOP_EDGE) / (MapRender.BOX_HEIGHT + MapRender.LINE_THICKNESS);
                if (!globals.selected_box.Contains(box_x + box_y * globals.sys_data.header.map_x)) //Don't add the same box multiple times.
                {
                    int add_index = box_x + box_y * globals.sys_data.header.map_x;
                    globals.selected_box.Add(add_index);
                    globals.selected_box_x.Add(box_x); //It's easier just to compute this now instead of trying to recompute it later when I need it again.
                    globals.selected_box_y.Add(box_y);
                    AddRowToDataGridView(globals.selected_box.IndexOf(add_index));
                }
                else
                {
                    int remove_index = globals.selected_box.IndexOf(box_x + box_y * globals.sys_data.header.map_x);
                    if (rb_Sys.Checked && (cb_Type.SelectedIndex == (int)SYS_SELECTIONS.Tiles || cb_Type.SelectedIndex == (int)SYS_SELECTIONS.Encounters || cb_Type.SelectedIndex == (int)GFX_SELECTIONS.Layers))
                    {
                        RemoveRowFromDataGridView(remove_index);
                    }
                    globals.selected_box.RemoveAt(remove_index);
                    globals.selected_box_x.RemoveAt(remove_index);
                    globals.selected_box_y.RemoveAt(remove_index);
                }
                Invalidate();
            }
        }

        //Highlight selected areas on the map.
        private void dgv_Data_SelectionChanged(object sender, EventArgs e)
        {
            globals.highlighted_box.Clear();
            globals.highlighted_box_x.Clear();
            globals.highlighted_box_y.Clear();
            if (rb_Gfx.Checked && cb_Type.SelectedIndex == (int)GFX_SELECTIONS.Filenames && dgv_Data.SelectedCells.Count > 0)
            {
                List<int> highlight = new List<int>();
                for (int x = 0; x < dgv_Data.SelectedCells.Count; x++)
                {
                    if (!highlight.Contains(dgv_Data.SelectedCells[x].RowIndex) && dgv_Data.SelectedCells[x].RowIndex != 0) //Perhaps hardcoding 0 as null is wrong? How does EON handle it?
                    {
                        highlight.Add(dgv_Data.SelectedCells[x].RowIndex);
                    }
                }
                for (int x = 0; x < highlight.Count; x++)
                {
                    int value = highlight[x];
                    for (int y = 0; y < globals.gfx_data.header.map_x * globals.gfx_data.header.map_y; y++)
                    {
                        for (int z = 0; z < globals.gfx_data.header.layer_count; z++)
                        {
                            int gfx_id = globals.gfx_data.layer_tiles[z][y].id;
                            if (gfx_id == value)
                            {
                                globals.highlighted_box.Add(gfx_id);
                                globals.highlighted_box_x.Add(y % 35);
                                globals.highlighted_box_y.Add(y / 35);
                            }
                        }
                    }
                }
                Invalidate();
            }
        }

        //Update values upon input. I'm using column and row indexes here, which is probably not a good idea if those indexes ever change. Find a better way?
        private void dgv_Data_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) //Making sure the data we enter is valid.
        {
            if (rb_Sys.Checked == true) //SYS is selected.
            {
                if (cb_Type.SelectedIndex == (int)SYS_SELECTIONS.Tiles) //"Tiles" is selected.
                {
                    (bool, object) result = TypeValidator(e.FormattedValue.ToString(), VALIDATOR_TYPES.u8); //u8 types
                    if (result.Item1 == true) //Using an if statement because there are only two values.
                    {
                        if (e.ColumnIndex == 2) //Type
                        {
                            globals.sys_data.behaviour_tiles[cb_Subtype.SelectedIndex][globals.selected_box[e.RowIndex]].type = (byte)result.Item2;
                        }
                        else if (e.ColumnIndex == 3) //ID
                        {
                            globals.sys_data.behaviour_tiles[cb_Subtype.SelectedIndex][globals.selected_box[e.RowIndex]].id = (byte)result.Item2;
                        }
                    }
                    else
                    {
                        FailEdit(e);
                    }
                }
                else if (cb_Type.SelectedIndex == (int)SYS_SELECTIONS.Objects) //"Objects" is selected.
                {
                    (bool, object) result = TypeValidator(e.FormattedValue.ToString(), VALIDATOR_TYPES.u8);
                    if (result.Item1 == true)
                    {
                        Debug.WriteLine(e.RowIndex);
                        switch (e.ColumnIndex)
                        {
                            case 1:
                                globals.sys_data.tile_objects[e.RowIndex].map_x = (byte)result.Item2;
                                break;
                            case 2:
                                globals.sys_data.tile_objects[e.RowIndex].map_y = (byte)result.Item2;
                                break;
                            case 3:
                                globals.sys_data.tile_objects[e.RowIndex].graphic = (byte)result.Item2;
                                break;
                            case 4:
                                globals.sys_data.tile_objects[e.RowIndex].unknown_1 = (byte)result.Item2;
                                break;
                            case 5:
                                globals.sys_data.tile_objects[e.RowIndex].unknown_2 = (byte)result.Item2;
                                break;
                            case 6:
                                globals.sys_data.tile_objects[e.RowIndex].unknown_3 = (byte)result.Item2;
                                break;
                            case 7:
                                globals.sys_data.tile_objects[e.RowIndex].activation_direction_1 = (byte)result.Item2;
                                break;
                            case 8:
                                globals.sys_data.tile_objects[e.RowIndex].activation_direction_2 = (byte)result.Item2;
                                break;
                            case 9:
                                globals.sys_data.tile_objects[e.RowIndex].activation_direction_3 = (byte)result.Item2;
                                break;
                            case 10:
                                globals.sys_data.tile_objects[e.RowIndex].activation_direction_4 = (byte)result.Item2;
                                break;
                            default: //Should never come here.
                                break;
                        }
                    }
                    else
                    {
                        FailEdit(e);
                    }
                }
                else if (cb_Type.SelectedIndex == (int)SYS_SELECTIONS.TileData) //"Objects" is selected.
                {
                    //I've honestly forgotten what TileData was supposed to be... oops.
                }
                else if (cb_Type.SelectedIndex == (int)SYS_SELECTIONS.Encounters) //"Encounters" is selected.
                {
                    (bool, object) result = TypeValidator(e.FormattedValue.ToString(), VALIDATOR_TYPES.u16);
                    if (result.Item1 == true)
                    {
                        Debug.WriteLine(e.RowIndex);
                        switch (e.ColumnIndex)
                        {
                            case 2:
                                globals.sys_data.encounters[globals.selected_box_x[e.RowIndex] + (globals.selected_box_y[e.RowIndex] * globals.sys_data.header.map_x)].encounter_id = (ushort)result.Item2;
                                break;
                            case 3:
                                globals.sys_data.encounters[globals.selected_box_x[e.RowIndex] + (globals.selected_box_y[e.RowIndex] * globals.sys_data.header.map_x)].danger = (ushort)result.Item2;
                                break;
                            case 4:
                                globals.sys_data.encounters[globals.selected_box_x[e.RowIndex] + (globals.selected_box_y[e.RowIndex] * globals.sys_data.header.map_x)].unknown_1 = (ushort)result.Item2;
                                break;
                            case 5:
                                globals.sys_data.encounters[globals.selected_box_x[e.RowIndex] + (globals.selected_box_y[e.RowIndex] * globals.sys_data.header.map_x)].unknown_2 = (ushort)result.Item2;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            else if (rb_Gfx.Checked == true) //GFX is selected.
            {
                if (cb_Type.SelectedIndex == (int)GFX_SELECTIONS.Layers) //"Layers" is selected.
                {
                    (bool, object) result = TypeValidator(e.FormattedValue.ToString(), VALIDATOR_TYPES.u8);
                    if (result.Item1 == true)
                    {
                        Debug.WriteLine(e.RowIndex);
                        switch (e.ColumnIndex)
                        {
                            case 2:
                                globals.gfx_data.layer_tiles[cb_Subtype.SelectedIndex][globals.selected_box[e.RowIndex]].id = (byte)result.Item2;
                                break;
                            case 3:
                                globals.gfx_data.layer_tiles[cb_Subtype.SelectedIndex][globals.selected_box[e.RowIndex]].rotation = (byte)result.Item2;
                                break;
                            case 4:
                                globals.gfx_data.layer_tiles[cb_Subtype.SelectedIndex][globals.selected_box[e.RowIndex]].unknown_1 = (byte)result.Item2;
                                break;
                            case 5:
                                globals.gfx_data.layer_tiles[cb_Subtype.SelectedIndex][globals.selected_box[e.RowIndex]].unknown_2 = (byte)result.Item2;
                                break;
                            default:
                                break;
                        }
                    }
                }
                else if (cb_Type.SelectedIndex == (int)GFX_SELECTIONS.Filenames) //"Filenames" is selected.
                {
                    (bool, object) result;
                    if ((e.ColumnIndex & 1) == 0) //If the column is even, it's a number. This is a fairly hacky way to do it.
                    {
                        result = TypeValidator(e.FormattedValue.ToString(), VALIDATOR_TYPES.u8);
                    }
                    else
                    {
                        result = TypeValidator(e.FormattedValue.ToString(), VALIDATOR_TYPES.str);
                    }
                    if (result.Item1 == true)
                    {
                        Debug.WriteLine(e.RowIndex);
                        switch (e.ColumnIndex)
                        {
                            case 1:
                                globals.gfx_data.indices[e.RowIndex].file_1 = (string)result.Item2;
                                break;
                            case 2:
                                globals.gfx_data.indices[e.RowIndex].file_1_rotation = (byte)result.Item2;
                                break;
                            case 3:
                                globals.gfx_data.indices[e.RowIndex].file_2 = (string)result.Item2;
                                break;
                            case 4:
                                globals.gfx_data.indices[e.RowIndex].file_2_rotation = (byte)result.Item2;
                                break;
                            case 5:
                                globals.gfx_data.indices[e.RowIndex].file_3 = (string)result.Item2;
                                break;
                            case 6:
                                globals.gfx_data.indices[e.RowIndex].file_3_rotation = (byte)result.Item2;
                                break;
                            case 7:
                                globals.gfx_data.indices[e.RowIndex].file_4 = (string)result.Item2;
                                break;
                            case 8:
                                globals.gfx_data.indices[e.RowIndex].file_4_rotation = (byte)result.Item2;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }
        private void dgv_Data_CellEnter(object sender, DataGridViewCellEventArgs e) //This is used for types that display map-wide lists of values rather than individual cell values.
        {
            if (rb_Sys.Checked && cb_Type.SelectedIndex == (int)SYS_SELECTIONS.Objects) //"Objects" is selected.
            {
                List<int> selected_rows = new List<int>();
                globals.selected_box.Clear();
                globals.selected_box_x.Clear();
                globals.selected_box_y.Clear();
                foreach (DataGridViewCell cell in dgv_Data.SelectedCells) //The easy way, but also the slow way.
                {
                    int row = cell.RowIndex;
                    if (!selected_rows.Contains(row))
                    {
                        selected_rows.Add(row);
                    }
                }
                foreach (int row in selected_rows)
                {
                    int box_x = Convert.ToInt32(dgv_Data[1, row].Value);
                    int box_y = Convert.ToInt32(dgv_Data[2, row].Value);
                    if (!globals.selected_box.Contains(box_x + box_y * globals.sys_data.header.map_x)) //Violating DRY here a bit, but it's not that big a deal. This is only the first copy.
                    {
                        int add_index = box_x + box_y * globals.sys_data.header.map_x;
                        globals.selected_box.Add(add_index);
                        globals.selected_box_x.Add(box_x);
                        globals.selected_box_y.Add(box_y);
                        AddRowToDataGridView(globals.selected_box.IndexOf(add_index));
                    }
                }
                Invalidate();
            }
            else if (rb_Gfx.Checked && cb_Type.SelectedIndex == (int)GFX_SELECTIONS.Filenames) //"Filenames" is selected.
            {

            }
        }

        private void FailEdit(DataGridViewCellValidatingEventArgs e)
        {
            dgv_Data.CancelEdit();
            dgv_Data.RefreshEdit();
            lb_Error.Visible = true;
            lb_Error.Text = "Value in row " + e.RowIndex.ToString() + ", column " + e.ColumnIndex.ToString() + " not updated due to invalid value.";
        }

        //Break this out into its own function, as it's called from a few spots.
        private void BuildDataGridView()
        {
            dgv_Data.Columns.Clear();
            dgv_Data.Rows.Clear();
            if (rb_Sys.Checked == true) //SYS is selected.
            {
                cb_Subtype.Visible = true;
                if (cb_Type.SelectedIndex == (int)SYS_SELECTIONS.Tiles) //"Tiles" is selected.
                {
                    cb_Subtype.Visible = true;
                    dgv_Data.Columns.Add("x", "X"); //I feel like this could be better looking with a for loop and pre-made arrays, but I find this a bit easier to follow.
                    dgv_Data.Columns[0].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns[0].ReadOnly = true;
                    dgv_Data.Columns.Add("y", "Y");
                    dgv_Data.Columns[1].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns[1].ReadOnly = true;
                    dgv_Data.Columns.Add("type", "Type");
                    dgv_Data.Columns[2].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns[2].ValueType = typeof(byte);
                    dgv_Data.Columns[2].ReadOnly = false;
                    dgv_Data.Columns.Add("value", "Value");
                    dgv_Data.Columns[3].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns[3].ReadOnly = false;

                    for (int x = 0; x < globals.selected_box.Count; x++)
                    {
                        dgv_Data.Rows.Add(
                            globals.selected_box_x[x],
                            globals.selected_box_y[x],
                            globals.sys_data.behaviour_tiles[cb_Subtype.SelectedIndex][globals.selected_box[x]].type,
                            globals.sys_data.behaviour_tiles[cb_Subtype.SelectedIndex][globals.selected_box[x]].id
                        );
                    }
                }
                else if (cb_Type.SelectedIndex == (int)SYS_SELECTIONS.Objects) //"Objects" is selected.
                {
                    dgv_Data.Columns.Add("entry", "Entry");
                    dgv_Data.Columns[0].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns[0].ReadOnly = true;
                    dgv_Data.Columns.Add("x", "X"); //X and Y are not arranged this way internally, but doing it like this keeps it consistent on the UI. Be careful about this.
                    dgv_Data.Columns[1].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns[1].ReadOnly = false;
                    dgv_Data.Columns.Add("y", "Y");
                    dgv_Data.Columns[2].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns[2].ReadOnly = false;
                    dgv_Data.Columns.Add("gfx", "Model");
                    dgv_Data.Columns[3].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns[3].ReadOnly = false;
                    dgv_Data.Columns.Add("unk1", "Unknown 1"); //0x3
                    dgv_Data.Columns[4].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns[4].ReadOnly = false;
                    dgv_Data.Columns.Add("unk2", "Unknown 2"); //0x4
                    dgv_Data.Columns[5].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns[5].ReadOnly = false;
                    dgv_Data.Columns.Add("unk3", "Unknown 3"); //0x7
                    dgv_Data.Columns[6].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns[6].ReadOnly = false;
                    dgv_Data.Columns.Add("ad1", "Direction 1");
                    dgv_Data.Columns[7].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns[7].ReadOnly = false;
                    dgv_Data.Columns.Add("ad2", "Direction 2");
                    dgv_Data.Columns[8].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns[8].ReadOnly = false;
                    dgv_Data.Columns.Add("ad3", "Direction 3");
                    dgv_Data.Columns[9].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns[9].ReadOnly = false;
                    dgv_Data.Columns.Add("ad4", "Direction 4");
                    dgv_Data.Columns[10].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns[10].ReadOnly = false;
                    for (int x = 0; x < globals.sys_data.tile_objects.Count; x++)
                    {
                        if (globals.sys_data.tile_objects[x].type == cb_Subtype.SelectedIndex + 0xD)
                        {
                            dgv_Data.Rows.Add(
                                globals.sys_data.tile_objects[x].id,
                                globals.sys_data.tile_objects[x].map_x,
                                globals.sys_data.tile_objects[x].map_y,
                                globals.sys_data.tile_objects[x].graphic,
                                globals.sys_data.tile_objects[x].unknown_1,
                                globals.sys_data.tile_objects[x].unknown_2,
                                globals.sys_data.tile_objects[x].unknown_3,
                                globals.sys_data.tile_objects[x].activation_direction_1,
                                globals.sys_data.tile_objects[x].activation_direction_2,
                                globals.sys_data.tile_objects[x].activation_direction_3,
                                globals.sys_data.tile_objects[x].activation_direction_4

                            );
                        }
                    }
                }
                else if (cb_Type.SelectedIndex == (int)SYS_SELECTIONS.Objects) //"Objects" is selected. These will be filled on a case-by-case basis.
                {
                    if (cb_Subtype.SelectedIndex == (int)SYS_TILES_SELECTIONS.Doors)
                    {

                    }
                    else if (cb_Subtype.SelectedIndex == (int)SYS_TILES_SELECTIONS.Staircases)
                    {

                    }
                    else if (cb_Subtype.SelectedIndex == (int)SYS_TILES_SELECTIONS.Chests)
                    {

                    }
                    else if (cb_Subtype.SelectedIndex == (int)SYS_TILES_SELECTIONS.TwoWays)
                    {

                    }
                    else if (cb_Subtype.SelectedIndex == (int)SYS_TILES_SELECTIONS.OneWays)
                    {

                    }
                    else if (cb_Subtype.SelectedIndex == (int)SYS_TILES_SELECTIONS.Doodads)
                    {

                    }
                    else if (cb_Subtype.SelectedIndex == (int)SYS_TILES_SELECTIONS.Pits)
                    {

                    }
                    else if (cb_Subtype.SelectedIndex == (int)SYS_TILES_SELECTIONS.Poles)
                    {

                    }
                    else if (cb_Subtype.SelectedIndex == (int)SYS_TILES_SELECTIONS.Unknown)
                    {

                    }
                    else if (cb_Subtype.SelectedIndex == (int)SYS_TILES_SELECTIONS.Currents)
                    {

                    }
                    else if (cb_Subtype.SelectedIndex == (int)SYS_TILES_SELECTIONS.MovingPlatforms)
                    {

                    }
                    else if (cb_Subtype.SelectedIndex == (int)SYS_TILES_SELECTIONS.ScriptedEvents)
                    {

                    }
                    else if (cb_Subtype.SelectedIndex == (int)SYS_TILES_SELECTIONS.RisingPlatforms)
                    {

                    }
                }
                else if (cb_Type.SelectedIndex == (int)SYS_SELECTIONS.Encounters) //"Encounters" is selected.
                {
                    cb_Subtype.Visible = true;
                    dgv_Data.Columns.Add("x", "X");
                    dgv_Data.Columns[0].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns[0].ReadOnly = true;
                    dgv_Data.Columns.Add("y", "Y");
                    dgv_Data.Columns[1].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns[1].ReadOnly = true;
                    dgv_Data.Columns.Add("eid", "Encounter");
                    dgv_Data.Columns[2].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns[2].ReadOnly = false;
                    dgv_Data.Columns.Add("danger", "Danger");
                    dgv_Data.Columns[3].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns[3].ReadOnly = false;
                    dgv_Data.Columns.Add("unk1", "Unk. 1");
                    dgv_Data.Columns[4].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns[4].ReadOnly = false;
                    dgv_Data.Columns.Add("unk2", "Unk. 2");
                    dgv_Data.Columns[5].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns[5].ReadOnly = false;
                    for (int x = 0; x < globals.selected_box.Count; x++)
                    {
                        dgv_Data.Rows.Add(
                            globals.selected_box_x[x],
                            globals.selected_box_y[x],
                            globals.sys_data.encounters[globals.selected_box[x]].encounter_id,
                            globals.sys_data.encounters[globals.selected_box[x]].danger,
                            globals.sys_data.encounters[globals.selected_box[x]].unknown_1,
                            globals.sys_data.encounters[globals.selected_box[x]].unknown_2
                        );
                    }
                }
            }
            else if (rb_Gfx.Checked == true) //GFX is selected.
            {
                if (cb_Type.SelectedIndex == (int)GFX_SELECTIONS.Layers)
                {
                    cb_Subtype.Visible = true;
                    dgv_Data.Columns.Add("x", "X");
                    dgv_Data.Columns[0].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns[0].ReadOnly = true;
                    dgv_Data.Columns.Add("y", "Y");
                    dgv_Data.Columns[1].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns[1].ReadOnly = true;
                    dgv_Data.Columns.Add("gid", "Graphic");
                    dgv_Data.Columns[2].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns[2].ReadOnly = false;
                    dgv_Data.Columns.Add("rtn", "Rotation");
                    dgv_Data.Columns[3].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns[3].ReadOnly = false;
                    dgv_Data.Columns.Add("unk1", "Unk. 1");
                    dgv_Data.Columns[4].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns[4].ReadOnly = false;
                    dgv_Data.Columns.Add("unk2", "Unk. 2");
                    dgv_Data.Columns[5].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns[5].ReadOnly = false;
                    for (int x = 0; x < globals.selected_box.Count; x++)
                    {
                        dgv_Data.Rows.Add(
                            globals.selected_box_x[x],
                            globals.selected_box_y[x],
                            globals.gfx_data.layer_tiles[cb_Subtype.SelectedIndex][globals.selected_box[x]].id,
                            globals.gfx_data.layer_tiles[cb_Subtype.SelectedIndex][globals.selected_box[x]].rotation,
                            globals.gfx_data.layer_tiles[cb_Subtype.SelectedIndex][globals.selected_box[x]].unknown_1,
                            globals.gfx_data.layer_tiles[cb_Subtype.SelectedIndex][globals.selected_box[x]].unknown_2
                        );
                    }
                }
                else if (cb_Type.SelectedIndex == (int)GFX_SELECTIONS.Filenames)
                {
                    cb_Subtype.Visible = false;
                    dgv_Data.Columns.Add("id", "ID");
                    dgv_Data.Columns[0].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns[0].ReadOnly = true;
                    dgv_Data.Columns.Add("fn1", "Filename 1");
                    dgv_Data.Columns[1].Width = COLUMN_WIDTH_WIDE;
                    dgv_Data.Columns[1].ReadOnly = false;
                    dgv_Data.Columns.Add("rot1", "Angle");
                    dgv_Data.Columns[2].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns[2].ReadOnly = false;
                    dgv_Data.Columns.Add("Filename 2", "Filename 2");
                    dgv_Data.Columns[3].Width = COLUMN_WIDTH_WIDE;
                    dgv_Data.Columns[3].ReadOnly = false;
                    dgv_Data.Columns.Add("rot2", "Angle");
                    dgv_Data.Columns[4].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns[4].ReadOnly = false;
                    dgv_Data.Columns.Add("fn3", "Filename 3");
                    dgv_Data.Columns[5].Width = COLUMN_WIDTH_WIDE;
                    dgv_Data.Columns[5].ReadOnly = false;
                    dgv_Data.Columns.Add("rot3", "Angle");
                    dgv_Data.Columns[6].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns[6].ReadOnly = false;
                    dgv_Data.Columns.Add("fn4", "Filename 4");
                    dgv_Data.Columns[7].Width = COLUMN_WIDTH_WIDE;
                    dgv_Data.Columns[7].ReadOnly = false;
                    dgv_Data.Columns.Add("rot4", "Angle");
                    dgv_Data.Columns[8].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns[8].ReadOnly = false;
                    for (int x = 0; x < globals.gfx_data.indices.Count; x++)
                    {
                        dgv_Data.Rows.Add(
                            x,
                            globals.gfx_data.indices[x].file_1, globals.gfx_data.indices[x].file_1_rotation,
                            globals.gfx_data.indices[x].file_2, globals.gfx_data.indices[x].file_2_rotation,
                            globals.gfx_data.indices[x].file_3, globals.gfx_data.indices[x].file_3_rotation,
                            globals.gfx_data.indices[x].file_4, globals.gfx_data.indices[x].file_4_rotation
                        );
                    }
                }
            }
        }

        //Instead of redrawing the whole DataGridView, have a function for adding to it.
        private void AddRowToDataGridView(int offset)
        {
            if (rb_Sys.Checked == true)
            {
                cb_Subtype.Visible = true;
                if (cb_Type.SelectedIndex == (int)SYS_SELECTIONS.Tiles)
                {
                    dgv_Data.Rows.Add(
                        globals.selected_box_x[offset],
                        globals.selected_box_y[offset],
                        globals.sys_data.behaviour_tiles[cb_Subtype.SelectedIndex][globals.selected_box[offset]].type,
                        globals.sys_data.behaviour_tiles[cb_Subtype.SelectedIndex][globals.selected_box[offset]].id
                    );
                }
                else if (cb_Type.SelectedIndex == (int)SYS_SELECTIONS.Encounters)
                {
                    dgv_Data.Rows.Add(
                        globals.selected_box_x[offset],
                        globals.selected_box_y[offset],
                        globals.sys_data.encounters[globals.selected_box[offset]].encounter_id,
                        globals.sys_data.encounters[globals.selected_box[offset]].danger,
                        globals.sys_data.encounters[globals.selected_box[offset]].unknown_1,
                        globals.sys_data.encounters[globals.selected_box[offset]].unknown_2
                    );
                }
            }
            else
            {
                if (cb_Type.SelectedIndex == (int)GFX_SELECTIONS.Layers)
                {
                    cb_Subtype.Visible = true;
                    if (globals.selected_box.Count > 0)
                    {
                        Debug.WriteLine(offset);
                        dgv_Data.Rows.Add(
                            globals.selected_box_x[offset],
                            globals.selected_box_y[offset],
                            globals.gfx_data.layer_tiles[cb_Subtype.SelectedIndex][globals.selected_box[offset]].id,
                            globals.gfx_data.layer_tiles[cb_Subtype.SelectedIndex][globals.selected_box[offset]].rotation,
                            globals.gfx_data.layer_tiles[cb_Subtype.SelectedIndex][globals.selected_box[offset]].unknown_1,
                            globals.gfx_data.layer_tiles[cb_Subtype.SelectedIndex][globals.selected_box[offset]].unknown_2
                        );
                    }
                }
            }
        }

        //This could probably be embedded into places where it's called, but I'm making it its own function in case I want to expand it.
        private void RemoveRowFromDataGridView(int offset)
        {
            dgv_Data.Rows.Remove(dgv_Data.Rows[offset]);
        }

        //This creates the list of all encounts used by the encount view display.
        private Dictionary<int, int> BuildEncountList()
        {
            Dictionary<int, int> encounts = new Dictionary<int, int>();
            for (int x = 0; x < globals.sys_data.encounters.Count; x++)
            {
                int group = globals.sys_data.encounters[x].encounter_id;
                if (!encounts.ContainsKey(group))
                {
                    encounts.Add(group, encounts.Count); //The group needs to be the key so it's easy to get the offset we need for the colour array.
                }
            }
            return encounts;
        }
        static private (bool success, object value) TypeValidator(string val, VALIDATOR_TYPES type) //Fairly sure I'm not supposed to be using object like this.
        {
            if (type == VALIDATOR_TYPES.u8)
            {
                bool success = Byte.TryParse(val, out byte result);
                return (success, result);
            }
            else if (type == VALIDATOR_TYPES.s8)
            {
                bool success = SByte.TryParse(val, out sbyte result);
                return (success, result);
            }
            else if (type == VALIDATOR_TYPES.u16)
            {
                bool success = ushort.TryParse(val, out ushort result);
                return (success, result);
            }
            else if (type == VALIDATOR_TYPES.s16)
            {
                bool success = short.TryParse(val, out short result);
                return (success, result);
            }
            else if (type == VALIDATOR_TYPES.u32)
            {
                bool success = uint.TryParse(val, out uint result);
                return (success, result);
            }
            else if (type == VALIDATOR_TYPES.s32)
            {
                bool success = int.TryParse(val, out int result);
                return (success, result);
            }
            else if (type == VALIDATOR_TYPES.str)
            {
                if (val.Length <= 0x20)
                {
                    if (System.Text.Encoding.UTF8.GetByteCount(val) == val.Length) //Checks for the presence of non-ASCII characters.
                    {
                        return (true, val);
                    }
                }
            }
            return (false, 0);
        }
    }
}
