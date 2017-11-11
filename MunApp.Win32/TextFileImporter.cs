using System;
using MetroFramework;
using MetroFramework.Forms;
using System.Windows.Forms;
using MetroFramework.Controls;
using System.Collections.Generic;

namespace MunApp.Win32
{
    public partial class TextFileImporter : MetroForm
    {
        private string[] countries;

        public string[] AddedCountries { get; set; }

        public TextFileImporter(string[] values)
        {
            InitializeComponent();
            countries = values;
            checkedListBox1.Items.Clear();
            checkedListBox1.Items.AddRange(countries);
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            //select all
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, true);
            }
        }

        private void metroButton4_Click(object sender, EventArgs e)
        {
            //invert selection
            for(int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, !checkedListBox1.GetItemChecked(i));
            }
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            //ok clicked
            List<String> items = new List<String>();
            DialogResult = DialogResult.OK;
            foreach(object item in checkedListBox1.CheckedItems)
            {
                items.Add(item.ToString());
            }
            AddedCountries = items.ToArray();
            Close();
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            //cancel clicked
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
