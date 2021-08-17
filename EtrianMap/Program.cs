using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using OriginTablets.Types;
using System.IO;
using System.Diagnostics;

namespace EtrianMap
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            List<byte[]> binaries = new List<byte[]>();
            //List<(string, string, byte[], byte[])> mapdat_list = new List<(string, string, byte[], byte[])>(); //Not quite sure this is a good way to do it.
            List<MapDatCollection> mapdat_list = new List<MapDatCollection>();
            List<Table> tables = new List<Table>();
            List<MBM> mbms = new List<MBM>(); //Might not need this if we're only loading one MBM.
            Application.EnableVisualStyles();
            //Load all the files we need. Is it bad form to do it this way, rather than just-in-time? They're so small that it probably won't matter...
            using (var openDialog = new CommonOpenFileDialog())
            {
                openDialog.IsFolderPicker = true;
                openDialog.AllowNonFileSystemItems = true;
                openDialog.Title = "Select your main Etrian Odyssey Nexus directory";
                if (openDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string enemynametable = openDialog.FileName + "\\Monster\\Table\\enemynametable.tbl";
                    string useitemnametable = openDialog.FileName + "\\Item\\useitemnametable.tbl";
                    string equipitemnametable = openDialog.FileName + "\\Item\\equipitemnametable.tbl";
                    string dungeonname = openDialog.FileName + "\\InterfaceFile\\dungeonname.mbm";
                    string encount = openDialog.FileName + "\\Dungeon\\encount.tbl";
                    string encount_group = openDialog.FileName + "\\Dungeon\\encount_group.tbl";
                    string floor = openDialog.FileName + "\\Dungeon\\floor.tbl";
                    string mapdat = openDialog.FileName + "\\MapDat\\";
                    if (File.Exists(enemynametable) && File.Exists(encount) && File.Exists(encount_group) && File.Exists(floor) && File.Exists(dungeonname) && File.Exists(useitemnametable)
                        && File.Exists(equipitemnametable) && File.Exists(mapdat + "\\sys01.msb") && File.Exists(mapdat + "\\gfx01.mgb"))
                    {
                        int encount_length = (int)(new FileInfo(encount).Length);
                        int encount_group_length = (int)(new FileInfo(encount_group).Length);
                        int floor_length = (int)(new FileInfo(floor).Length);
                        Table enemynametable_file = new Table(enemynametable, false);
                        Table useitemnametable_file = new Table(useitemnametable, false);
                        Table equipitemnametable_file = new Table(equipitemnametable, false);
                        MBM dungeonname_file = new MBM(dungeonname);
                        byte[] encount_file = new byte[encount_length];
                        using (BinaryReader encount_stream = new BinaryReader(new FileStream(encount, FileMode.Open)))
                        {
                            encount_stream.Read(encount_file, 0, encount_length);
                        }
                        binaries.Add(encount_file);
                        byte[] encount_group_file = new byte[encount_group_length];
                        using (BinaryReader encount_group_stream = new BinaryReader(new FileStream(encount_group, FileMode.Open)))
                        {
                            encount_group_stream.Read(encount_group_file, 0, encount_group_length);
                        }
                        binaries.Add(encount_group_file);
                        byte[] floor_file = new byte[floor_length];
                        using (BinaryReader floor_stream = new BinaryReader(new FileStream(floor, FileMode.Open)))
                        {
                            floor_stream.Read(floor_file, 0, floor_length);
                        }
                        binaries.Add(floor_file);
                        DirectoryInfo mapdat_files = new DirectoryInfo(mapdat);
                        int sys_count = mapdat_files.GetFiles("*.msb").Length;
                        int gfx_count = mapdat_files.GetFiles("*.mgb").Length;
                        List<string> sys_list = new List<string>();
                        List<string> gfx_list = new List<string>();
                        foreach (var f in mapdat_files.GetFiles("*.msb"))
                        {
                            sys_list.Add(f.Name);
                        }
                        foreach (var f in mapdat_files.GetFiles("*.mgb"))
                        {
                            gfx_list.Add(f.Name);
                        }
                        if (sys_count == gfx_count)
                        {
                            for (int x = 0; x < sys_count; x++)
                            {
                                string sys_filename = mapdat + sys_list[x];
                                string gfx_filename = mapdat + gfx_list[x];
                                int sys_length = (int)(new FileInfo(sys_filename).Length);
                                int gfx_length = (int)(new FileInfo(gfx_filename).Length);
                                byte[] sys_file = new byte[sys_length];
                                byte[] gfx_file = new byte[gfx_length];
                                using (BinaryReader sys_stream = new BinaryReader(new FileStream(sys_filename, FileMode.Open)))
                                {
                                    sys_stream.Read(sys_file, 0, sys_length);
                                }
                                using (BinaryReader gfx_stream = new BinaryReader(new FileStream(gfx_filename, FileMode.Open)))
                                {
                                    gfx_stream.Read(gfx_file, 0, gfx_length);
                                }
                                MapDatCollection entry = new MapDatCollection();
                                entry.sys_filename = sys_list[x];
                                entry.gfx_filename = gfx_list[x];
                                entry.sys_file = sys_file;
                                entry.gfx_file = gfx_file;
                                mapdat_list.Add(entry); //This feels slightly weird...
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Invalid Etrian Odyssey Nexus directory. Give this error message more detail later.");
                        Environment.Exit(0);
                    }
                }
                else
                {
                    MessageBox.Show("Invalid Etrian Odyssey Nexus directory. Give this error message more detail later.");
                    Environment.Exit(0);
                }
            }
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new EtrianMap(binaries, tables, mbms, mapdat_list));
        }
    }
}
