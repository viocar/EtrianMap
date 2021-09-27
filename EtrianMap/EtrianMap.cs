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
        //Initial program setup.
        Globals globals = new Globals();
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
                globals.map_data = BuildInitialMapData();
                globals.map_area = new Rectangle(
                    MapRender.LEFT_EDGE, 
                    MapRender.TOP_EDGE, 
                    (MapRender.BOX_WIDTH + MapRender.LINE_THICKNESS) * globals.map_data.header.map_x, 
                    (MapRender.BOX_HEIGHT + MapRender.LINE_THICKNESS) * globals.map_data.header.map_y
                );
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
        private void EtrianMap_MouseClick(object sender, MouseEventArgs e)
        {
            var mouse_pos = PointToClient(Cursor.Position);
            if (globals.map_area.Contains(new Point(mouse_pos.X, mouse_pos.Y)))
            {
                if (ModifierKeys != Keys.Control)
                {
                    globals.selected_box.Clear();
                    globals.selected_box_x.Clear();
                    globals.selected_box_y.Clear();
                }
                int box_x = (mouse_pos.X - MapRender.LEFT_EDGE) / (MapRender.BOX_WIDTH + MapRender.LINE_THICKNESS);
                int box_y = (mouse_pos.Y - MapRender.TOP_EDGE) / (MapRender.BOX_HEIGHT + MapRender.LINE_THICKNESS);
                globals.selected_box.Add(box_x + box_y * globals.map_data.header.map_x);
                globals.selected_box_x.Add(box_x); //It's easier just to compute this now instead of trying to recompute it later when I need it again.
                globals.selected_box_y.Add(box_y);
                Debug.WriteLine(box_x.ToString("X2") + ", " + box_y.ToString("X2"));
                Invalidate();
            }
            else
            {
                Debug.WriteLine("Not in map area");
            }
        }
    }
}
