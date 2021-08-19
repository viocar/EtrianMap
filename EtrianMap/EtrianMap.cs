﻿using System;
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
        public EtrianMap(List<byte[]> binaries, List<Table> tables, List<MBM> mbms, List<MapDatCollection> mapdat_list)
        {
            globals.sample_renderer_enabled = true;
            InitializeComponent();
            int new_map = MapSelectDialog(mapdat_list);
            if (new_map > 0)
            {
                globals.open_map = new_map;
                globals.map_data = BuildInitialMapData(mapdat_list[globals.open_map].sys_file, mapdat_list[globals.open_map].gfx_file); //Change this to the MSBFile + MGBFile type later
                globals.sample_renderer_enabled = false;
                //MapRedraw();
            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private int MapSelectDialog(List<MapDatCollection> mapdat_list)
        {
            using (MapSelector MapSelector = new MapSelector(mapdat_list))//This dialogue prompts the user to select a map.
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
    }
}
