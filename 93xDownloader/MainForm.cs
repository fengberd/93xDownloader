using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

using ICSharpCode.SharpZipLib.BZip2;

namespace Downloader_93x
{
    public partial class MainForm : Form
    {
        object download_speed_locker = new object();
        long download_speed = 0;
        int simultaneous_downloads = 0;
        Config config = new Config("config.ini"), item_type = new Config("item_type.ini");

        public MainForm()
        {
            InitializeComponent();
            textBox1.Text = config["MapsPath","./download/maps"];
            textBox2.Text = config["OtherPath","./download/other"];
            textBox3.Text = (simultaneous_downloads = Math.Max(Math.Min(config.GetInt("SimultaneousDownloads",20),10),1)).ToString();
        }

        private void MainForm_Load(object sender,EventArgs e)
        {
            button1.PerformClick();
        }

        private void button1_Click(object sender,EventArgs e)
        {
            listView1.Items.Clear();
            foreach(string k in item_type.Keys)
            {
                listView1.Items.Add(k);
            }
        }

        private void button2_Click(object sender,EventArgs e)
        {
            listView3.Items.Clear();
        }

        private void textBox1_TextChanged(object sender,EventArgs e)
        {
            config["MapsPath"] = textBox1.Text;
        }

        private void textBox2_TextChanged(object sender,EventArgs e)
        {
            config["OtherPath"] = textBox2.Text;
        }

        private void textBox3_Leave(object sender,EventArgs e)
        {
            if(int.TryParse(textBox3.Text,out int parse))
            {
                simultaneous_downloads = Math.Max(Math.Min(parse,20),1);
            }
            textBox3.Text = simultaneous_downloads.ToString();
            config["SimultaneousDownloads"] = textBox3.Text;
        }

        private void timer1_Tick(object sender,EventArgs e)
        {
            while(listView2.Items.Count < simultaneous_downloads && listView3.Items.Count != 0)
            {
                var add = listView3.Items[0];
                listView3.Items.Remove(add);
                add.SubItems.Add(new ListViewItem.ListViewSubItem(add,"CONNECT"));
                ThreadPool.QueueUserWorkItem(new WaitCallback((obj) =>
                {
                    var item = obj as ListViewItem;
                    bool bz2_mode = false;
                    long size = long.Parse(item.SubItems[2].Text);
                    string url = item.SubItems[1].Text, path = url;
                    if(path.ToLower().EndsWith(".bz2"))
                    {
                        bz2_mode = true;
                        path = path.Substring(0,path.Length - 4);
                    }
                    path = Path.Combine(path.ToLower().EndsWith(".bsp") ? textBox1.Text : textBox2.Text,Path.GetFileName(path));
                    Directory.CreateDirectory(Directory.GetParent(path).FullName);
                    if(File.Exists(path) && size > 0 && new FileInfo(path).Length == size)
                    {
                        Invoke(new Action(() =>
                        {
                            listView2.Items.Remove(item);
                        }));
                        return;
                    }
                    try
                    {
                        using(var http = Web.HttpGetStream(url,out long web_size))
                        {
                            var input = http;
                            if(bz2_mode)
                            {
                                input = new BZip2InputStream(input);
                            }
                            else if(size < 0 && web_size > 0)
                            {
                                size = web_size;
                                Invoke(new Action(() =>
                                {
                                    item.SubItems[2].Text = size.ToString();
                                    item.SubItems[3].Text = Program.FormatSize(size);
                                }));
                                if(File.Exists(path) && size > 0 && new FileInfo(path).Length == size)
                                {
                                    Invoke(new Action(() =>
                                    {
                                        listView2.Items.Remove(item);
                                    }));
                                    return;
                                }
                            }
                            using(var output = File.Create(path,81920))
                            {
                                int read;
                                long processed = 0;
                                byte[] buffer = new byte[81920];
                                while((read = input.Read(buffer,0,buffer.Length)) > 0)
                                {
                                    processed += read;
                                    lock(download_speed_locker)
                                    {
                                        download_speed += read;
                                    }
                                    output.Write(buffer,0,read);
                                    Invoke(new Action(() =>
                                    {
                                        item.SubItems[4].Text = size > 0 ? Math.Round((double)processed / size * 100,2) + "%" : "UNKNOWN";
                                    }));
                                }
                            }
                            Invoke(new Action(() =>
                            {
                                listView2.Items.Remove(item);
                            }));
                        }
                    }
                    catch(Exception ex)
                    {
                        Invoke(new Action(() =>
                        {
                            MessageBox.Show(ex.ToString(),"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                        }));
                    }
                }),listView2.Items.Add(add));
            }
        }

        private void timer2_Tick(object sender,EventArgs e)
        {
            lock(download_speed_locker)
            {
                label3.Text = "(Decompress)Speed: " + Program.FormatSize(download_speed) + "/s";
                download_speed = 0;
            }
        }

        private void checkBox1_CheckedChanged(object sender,EventArgs e)
        {
            timer1.Enabled = checkBox1.Checked;
        }

        private void listView3_KeyDown(object sender,KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Delete)
            {
                foreach(ListViewItem i in listView3.SelectedItems)
                {
                    listView3.Items.Remove(i);
                }
            }
        }

        private void listView1_DoubleClick(object sender,EventArgs e)
        {
            if(listView1.SelectedItems.Count != 0)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback((obj) =>
                {
                    try
                    {
                        string[] result = Web.HttpGet(item_type[(string)obj]).Split('\n');
                        var items = new Dictionary<string,ItemToDownload>();
                        foreach(string row in result)
                        {
                            string[] split = row.Split(new[] { "--" },StringSplitOptions.None);
                            if(split.Length >= 2)
                            {
                                items[split[0]] = new ItemToDownload()
                                {
                                    url = split[1].Replace("\r",""),
                                    size = split.Length >= 3 ? long.Parse(split[2]) : -1
                                };
                            }
                        }
                        Invoke(new Action(() =>
                        {
                            new ItemsForm(this,(string)obj,items).Show();
                        }));
                    }
                    catch(Exception ex)
                    {
                        Invoke(new Action(() =>
                        {
                            MessageBox.Show(ex.ToString(),"Error",MessageBoxButtons.OK,MessageBoxIcon.Error);
                        }));
                    }
                }),listView1.SelectedItems[0].Text);
            }
        }
    }
}
