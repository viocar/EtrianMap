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
        readonly string[] SYS_CONTROLS = { "Tiles", "Objects", "Encounters" }; //Is there somewhere else I should be putting this code?
        readonly string[] SYS_TILES_CONTROLS = { "Layer 1", "Layer 2", "Layer 3", "Layer 4", "Layer 5", "Layer 6", "Layer 7", "Layer 8", "Layer 9", "Layer 10", };
        readonly string[] SYS_OBJECTS_CONTROLS = { "Doors", "Staircases", "Chests", "Two-way passages", "One-way passages", "Doodads", "Pits", "Geomagnetic poles", "Unknown", "Currents", "Moving platforms", "Scripted events", "Rising platforms" };
        readonly string[] SYS_ENCOUNTERS_CONTROLS = { "Encounter groups", "Monster groups" };
        readonly string[] GFX_CONTROLS = { "Layers", "Filenames" };
        readonly string[] GFX_LAYERS_CONTROLS = { "Layer 1", "Layer 2", "Layer 3", "Layer 4", "Layer 5", "Layer 6", "Layer 7", "Layer 8", "Layer 9", "Layer 10", };
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
                globals.sys_data = BuildInitialMapData();
                globals.gfx_data = BuildInitialGfxData();
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

        //Handle clicks on the map.
        private void EtrianMap_MouseClick(object sender, MouseEventArgs e) //This also runs on MouseDoubleClick since this didn't fire if I double clicked. What is the better way?
        {
            var mouse_pos = PointToClient(Cursor.Position);
            if (globals.map_area.Contains(new Point(mouse_pos.X, mouse_pos.Y)))
            {
                if (ModifierKeys != Keys.Control)
                {
                    globals.selected_box.Clear();
                    globals.selected_box_x.Clear();
                    globals.selected_box_y.Clear();
                    dgv_Data.Rows.Clear();

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
                    globals.selected_box.RemoveAt(remove_index);
                    globals.selected_box_x.RemoveAt(remove_index);
                    globals.selected_box_y.RemoveAt(remove_index);
                    Debug.WriteLine(globals.selected_box.Count);
                    RemoveRowFromDataGridView(remove_index);
                }
                Invalidate();
            }
        }

        private void dgv_Data_SelectionChanged(object sender, EventArgs e)
        {
            globals.highlighted_box.Clear();
            globals.highlighted_box_x.Clear();
            globals.highlighted_box_y.Clear();
            if (cb_Type.SelectedIndex == 1 && dgv_Data.SelectedCells.Count > 0)
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
        private void rb_Sys_CheckedChanged(object sender, EventArgs e)
        {
            cb_Type.DataSource = SYS_CONTROLS;
            cb_Type.BindingContext = new BindingContext();
            cb_Subtype.DataSource = SYS_TILES_CONTROLS;
            cb_Subtype.BindingContext = new BindingContext();
            Invalidate();
        }

        private void rb_Gfx_CheckedChanged(object sender, EventArgs e)
        {
            cb_Type.DataSource = GFX_CONTROLS;
            cb_Type.BindingContext = new BindingContext();
            cb_Subtype.DataSource = GFX_LAYERS_CONTROLS;
            cb_Subtype.BindingContext = new BindingContext();
            Invalidate();
        }

        private void cb_Type_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (rb_Sys.Checked) //The GFX file has only one subtype menu so there's no need to switch here.
            {
                if (cb_Type.SelectedIndex == 0)
                {
                    cb_Subtype.DataSource = SYS_TILES_CONTROLS;
                    cb_Subtype.BindingContext = new BindingContext();
                }
                else if (cb_Type.SelectedIndex == 1)
                {
                    cb_Subtype.DataSource = SYS_OBJECTS_CONTROLS;
                    cb_Subtype.BindingContext = new BindingContext();
                }
                else if (cb_Type.SelectedIndex == 2)
                {
                    cb_Subtype.DataSource = SYS_ENCOUNTERS_CONTROLS;
                    cb_Subtype.BindingContext = new BindingContext();
                }
            }
            Invalidate();
            BuildDataGridView();
        }

        private void cb_Subtype_SelectedIndexChanged(object sender, EventArgs e)
        {
            BuildDataGridView(); 
        }
        private void BuildDataGridView()
        {
            dgv_Data.Columns.Clear();
            dgv_Data.Rows.Clear();
            if (rb_Sys.Checked == true) //SYS is selected.
            {
                cb_Subtype.Visible = true;
                if (cb_Type.SelectedIndex == 0) //"Tiles" is selected.
                {
                    cb_Subtype.Visible = true;
                    dgv_Data.Columns.Add("x", "X"); //I feel like this could be better looking with a for loop and pre-made arrays, but I find this a bit easier to follow.
                    dgv_Data.Columns[0].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns.Add("y", "Y");
                    dgv_Data.Columns[1].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns.Add("type", "Type");
                    dgv_Data.Columns[2].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns.Add("id", "ID");
                    dgv_Data.Columns[3].Width = COLUMN_WIDTH_MEDIUM;
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
                else if (cb_Type.SelectedIndex == 1) //"Objects" is selected.
                {

                }
                else if (cb_Type.SelectedIndex == 2) //"Encounters" is selected.
                {
                    cb_Subtype.Visible = true;
                    if (cb_Subtype.SelectedIndex == 0)
                    {
                        dgv_Data.Columns.Add("x", "X");
                        dgv_Data.Columns[0].Width = COLUMN_WIDTH_NARROW;
                        dgv_Data.Columns.Add("y", "Y");
                        dgv_Data.Columns[1].Width = COLUMN_WIDTH_NARROW;
                        dgv_Data.Columns.Add("eid", "Encounter");
                        dgv_Data.Columns[2].Width = COLUMN_WIDTH_MEDIUM;
                        dgv_Data.Columns.Add("danger", "Danger");
                        dgv_Data.Columns[3].Width = COLUMN_WIDTH_MEDIUM;
                        dgv_Data.Columns.Add("unk1", "Unk. 1");
                        dgv_Data.Columns[4].Width = COLUMN_WIDTH_MEDIUM;
                        dgv_Data.Columns.Add("unk2", "Unk. 2");
                        dgv_Data.Columns[5].Width = COLUMN_WIDTH_MEDIUM;
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
            }
            else if (rb_Gfx.Checked == true) //GFX is selected.
            {
                if (cb_Type.SelectedIndex == 0) //This is a slightly unreliable way to do this if the values in the control box change.
                {
                    cb_Subtype.Visible = true;
                    dgv_Data.Columns.Add("x", "X");
                    dgv_Data.Columns[0].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns.Add("y", "Y");
                    dgv_Data.Columns[1].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns.Add("gid", "Graphic");
                    dgv_Data.Columns[2].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns.Add("rtn", "Rotation");
                    dgv_Data.Columns[3].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns.Add("unk1", "Unk. 1");
                    dgv_Data.Columns[4].Width = COLUMN_WIDTH_MEDIUM;
                    dgv_Data.Columns.Add("unk2", "Unk. 2");
                    dgv_Data.Columns[5].Width = COLUMN_WIDTH_MEDIUM;
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
                else if (cb_Type.SelectedIndex == 1)
                {
                    cb_Subtype.Visible = false;
                    dgv_Data.Columns.Add("id", "ID");
                    dgv_Data.Columns[0].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns.Add("fn1", "Filename 1");
                    dgv_Data.Columns[1].Width = COLUMN_WIDTH_WIDE;
                    dgv_Data.Columns.Add("rot1", "Angle");
                    dgv_Data.Columns[2].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns.Add("Filename 2", "Filename 2");
                    dgv_Data.Columns[3].Width = COLUMN_WIDTH_WIDE;
                    dgv_Data.Columns.Add("rot2", "Angle");
                    dgv_Data.Columns[4].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns.Add("fn3", "Filename 3");
                    dgv_Data.Columns[5].Width = COLUMN_WIDTH_WIDE;
                    dgv_Data.Columns.Add("rot3", "Angle");
                    dgv_Data.Columns[6].Width = COLUMN_WIDTH_NARROW;
                    dgv_Data.Columns.Add("fn4", "Filename 4");
                    dgv_Data.Columns[7].Width = COLUMN_WIDTH_WIDE;
                    dgv_Data.Columns.Add("rot4", "Angle");
                    dgv_Data.Columns[8].Width = COLUMN_WIDTH_NARROW;
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
        private void AddRowToDataGridView(int offset)
        {
            if (rb_Sys.Checked == true)
            {
                cb_Subtype.Visible = true;
                if (cb_Type.SelectedIndex == 0)
                {
                    dgv_Data.Rows.Add(
                        globals.selected_box_x[offset],
                        globals.selected_box_y[offset],
                        globals.sys_data.behaviour_tiles[cb_Subtype.SelectedIndex][globals.selected_box[offset]].type,
                        globals.sys_data.behaviour_tiles[cb_Subtype.SelectedIndex][globals.selected_box[offset]].id
                    );
                }
                else if (cb_Type.SelectedIndex == 1)
                {

                }
                else if (cb_Type.SelectedIndex == 2)
                {
                    if (cb_Subtype.SelectedIndex == 0)
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
            }
            else
            {
                if (cb_Type.SelectedIndex == 0) //This is a slightly unreliable way to do this if the values in the control box change.
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
        private void RemoveRowFromDataGridView(int offset)
        {
            dgv_Data.Rows.Remove(dgv_Data.Rows[offset]);
        }
    }
}
