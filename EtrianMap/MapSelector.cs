using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace EtrianMap
{
    public partial class MapSelector : Form
    {
        public int MapSelected
        {
            get
            {
                return ListBox_MapSelect.SelectedIndex;
            }
        }
        public MapSelector(List<MapDatCollection> mapdat_data)
        {
            InitializeComponent();
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            foreach (MapDatCollection map_filename in mapdat_data)
            {
                ListBox_MapSelect.Items.Add(map_filename.sys_filename);
            }
            ListBox_MapSelect.SelectedIndex = 0;
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            SelectMap();
        }
        private void ListBox_MapSelect_DoubleClick(object sender, EventArgs e)
        {
            SelectMap();
        }
        private void ListBox_MapSelect_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SelectMap();
            }
        }
        private void SelectMap()
        {
            DialogResult = DialogResult.OK;
        }
    }
}
