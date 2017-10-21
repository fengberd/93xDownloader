using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Downloader_93x
{
    public partial class ItemsForm : Form
    {
        MainForm main = null;

        public ItemsForm(MainForm main,string title,Dictionary<string,ItemToDownload> items)
        {
            InitializeComponent();
            listView3.BeginUpdate();
            foreach(string k in items.Keys)
            {
                listView3.Items.Add(new ListViewItem(new string[]
                {
                    k,
                    items[k].url,
                    items[k].size.ToString(),
                    Program.FormatSize(items[k].size)
                }));
            }
            listView3.EndUpdate();
            label1.Text = "Source:" + title;
            this.main = main;
        }

        private void button2_Click(object sender,EventArgs e)
        {
            listView3.BeginUpdate();
            foreach(ListViewItem i in listView3.Items)
            {
                i.Checked = true;
            }
            listView3.EndUpdate();
        }

        private void button3_Click(object sender,EventArgs e)
        {
            listView3.BeginUpdate();
            foreach(ListViewItem i in listView3.Items)
            {
                i.Checked = !i.Checked;
            }
            listView3.EndUpdate();
        }

        private void button1_Click(object sender,EventArgs e)
        {
            foreach(ListViewItem i in listView3.Items)
            {
                if(i.Checked && main.listView3.FindItemWithText(i.Text) == null)
                {
                    main.listView3.Items.Add(i.Clone() as ListViewItem);
                }
            }
            Close();
        }

        private void ItemsForm_Load(object sender,EventArgs e)
        {

        }

        private void listView3_MouseClick(object sender,MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right && listView3.SelectedItems.Count == 1)
            {
                Clipboard.SetText(listView3.SelectedItems[0].SubItems[1].Text);
            }
        }

    }
}
