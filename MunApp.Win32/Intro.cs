using System;
using MunApp.Common;
using MetroFramework;
using System.Windows.Forms;
using MetroFramework.Forms;
using MetroFramework.Controls;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace MunApp.Win32
{
    public partial class Intro : MetroForm
    {
        private List<MetroPanel> pages = new List<MetroPanel>();

        public int SelectedIndex { get; set; }

        private event EventHandler SelectedIndexChanged;

        public Intro()
        {
            InitializeComponent();
            InitializePages();
        }

        private void InitializePages()
        {
            SelectedIndexChanged += (sender, e) =>
            {
                if (SelectedIndex >= pages.Count)
                    SelectedIndex = pages.Count - 1;
                if (SelectedIndex < 0)
                    SelectedIndex = 0;
                if (SelectedIndex == 0)
                    metroButton3.Enabled = false;
                else
                    metroButton3.Enabled = true;
                if (SelectedIndex == pages.Count - 1)
                    metroButton2.Text = "Finish";
                else
                    metroButton2.Text = "Next >>";
                metroPanel1.Controls.Clear();
                pages[SelectedIndex].Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                metroPanel1.Controls.Add(pages[SelectedIndex]);
            };
            foreach(TabPage page in metroTabControl1.TabPages)
            {
                Control[] controls = new Control[page.Controls.Count];
                page.Controls.CopyTo(controls, 0);
                if (controls[0] is MetroPanel)
                    pages.Add(controls[0] as MetroPanel);
                else
                    throw new Exception();
            }
            Controls.Remove(metroTabControl1);
            metroPanel1.Controls.Add(pages[0]);
            SelectedIndex = 0;
            if (SelectedIndexChanged != null)
                SelectedIndexChanged(this, EventArgs.Empty);

            foreach(Country country in Data.Countries)
            {
                listBox1.Items.Add(country.Name);
            }
            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            listBox1.SelectedIndex = 0;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image = Properties.Resources.FlagNotAvailable;
            }
            else
            {
                Country country = Data.Countries[listBox1.SelectedIndex];
                if (File.Exists(country.Flag))
                    pictureBox1.Image = Image.FromFile(country.Flag);
                else
                    pictureBox1.Image = Properties.Resources.FlagNotAvailable;
                metroTextBox2.Text = country.Name;
            }
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            //next
            switch(SelectedIndex)
            {
                case 4:
                    DialogResult = DialogResult.OK;
                    Hide();
                    Data.FirstRun = false;
                    using (MainWindow mainWindow = new MainWindow())
                    {
                        mainWindow.ShowDialog();
                    }
                    Close();
                    break;
                case 1:
                    metroLabel4.Visible = true;
                    //activation code
                    string serialkey = metroTextBox1.Text.Replace(" ", "").Replace("-", "");
                    ActivationStatus status = Data.Activate(serialkey);
                    switch(status)
                    {
                        case ActivationStatus.Success:
                            SelectedIndex++;
                            if (SelectedIndexChanged != null)
                                SelectedIndexChanged(this, EventArgs.Empty);
                            break;
                        case ActivationStatus.UnableToConnect:
                            MetroMessageBox.Show(this, "Unable to connect to the server. Please check your internet connection and try again. If the problem persists, please contact us.", "Connection error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            break;
                        case ActivationStatus.Failure:
                            MetroMessageBox.Show(this, "The serial key that you have entered appears to be invalid. Please check your serial key and try again. If you are facing problems in activating your product, please contact us.", "Invalid serial key", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            break;
                    }
                    metroLabel4.Visible = false;
                    break;
                default:
                    SelectedIndex++;
                    if (SelectedIndexChanged != null)
                        SelectedIndexChanged(this, EventArgs.Empty);
                    break;
            }
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            //back
            SelectedIndex--;
            if (SelectedIndexChanged != null)
                SelectedIndexChanged(this, EventArgs.Empty);
        }

        private void metroButton4_Click(object sender, EventArgs e)
        {
            //cancel
            Close();
        }

        private void metroButton5_Click(object sender, EventArgs e)
        {
            //add country
            Country country = new Country();
            country.Name = "New Country " + (Data.Countries.Count + 1);
            country.Flag = "";
            Data.Countries.Add(country);

            Data.Save();

            RefreshCountriesList();
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }

        private void metroButton21_Click(object sender, EventArgs e)
        {
            //clear list
            Data.Countries.Clear();
            pictureBox1.Image = null;
            metroTextBox2.Text = "";
            Data.Save();

            RefreshCountriesList();
        }

        private void metroButton22_Click(object sender, EventArgs e)
        {
            //remove selection
            Data.Countries.RemoveAt(listBox1.SelectedIndex);

            Data.Save();

            RefreshCountriesList();
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }

        private void metroButton7_Click(object sender, EventArgs e)
        {
            //load default countries
            Data.Countries.AddRange(Data.GetDefaultCountries());

            Data.Save();

            RefreshCountriesList();
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }

        private void metroButton17_Click(object sender, EventArgs e)
        {
            //import countries from text file
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = false;
                ofd.Title = "Choose the text file to import countries from";
                ofd.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    string[] lines = File.ReadAllLines(ofd.FileName);
                    using (TextFileImporter tfi = new TextFileImporter(lines))
                    {
                        if (tfi.ShowDialog() == DialogResult.OK)
                        {
                            List<Country> countries = new List<Country>();
                            foreach (string item in tfi.AddedCountries)
                            {
                                if (!Data.CountryExists(item))
                                {
                                    Country country = new Country();
                                    country.Name = item;
                                    country.Flag = "";
                                    Data.Countries.Add(country);
                                }
                            }
                            Data.Save();

                            RefreshCountriesList();
                            listBox1.SelectedIndex = listBox1.Items.Count - 1;
                        }
                    }
                }
            }
        }

        private void metroButton6_Click(object sender, EventArgs e)
        {
            //import countries from excel file
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = false;
                ofd.Title = "Choose the text file to import countries from";
                ofd.Filter = "Excel files (*.xls, *.xlsx)|*.xls;*.xlsx";
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    using (ExcelImporter ei = new ExcelImporter(ofd.FileName))
                    {
                        if (ei.ShowDialog() == DialogResult.OK)
                        {
                            List<Country> countries = new List<Country>();
                            foreach (string item in ei.SelectedCountries)
                            {
                                if (!Data.CountryExists(item))
                                {
                                    Country country = new Country();
                                    country.Name = item;
                                    country.Flag = "";
                                    Data.Countries.Add(country);
                                }
                            }
                            Data.Save();

                            RefreshCountriesList();
                            listBox1.SelectedIndex = listBox1.Items.Count - 1;
                        }
                    }
                }
            }
        }

        private void RefreshCountriesList()
        {
            listBox1.Items.Clear();
            string[] countries = Data.GetCountryNames().ToArray();
            Country[] presentCountries = Data.GetPresentCountries().ToArray();
            listBox1.Items.AddRange(countries);
            if (listBox1.Items.Count > 0)
                listBox1.SelectedIndex = 0;
        }

        private void metroTextBox3_TextChanged(object sender, EventArgs e)
        {
            //committee text changed
            Data.CommitteeName = metroTextBox3.Text;
        }

        private void metroTextBox4_TextChanged(object sender, EventArgs e)
        {
            //gsl agenda text changed
            Data.GSLAgenda = metroTextBox4.Text;
        }

        private void metroTextBox6_TextChanged(object sender, EventArgs e)
        {
            //ssl agenda text changed
            Data.SSLAgenda = metroTextBox6.Text;
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            //change country flag
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = false;
                ofd.Title = "Choose the image file to set as a flag";
                ofd.Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png";
                ofd.ShowDialog(this);
                Country country = Data.Countries[listBox1.SelectedIndex];
                country.Flag = ofd.FileName;
                pictureBox1.Image = Image.FromFile(country.Flag);
                Data.Countries[listBox1.SelectedIndex] = country;
                Data.Save();
            }
        }

        private void metroTextBox2_Leave(object sender, EventArgs e)
        {
            //leave settings country name textBox without saving (which is pressing enter)
            metroTextBox6.Text = Data.Countries[listBox1.SelectedIndex].Name;
        }

        private void metroTextBox2_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //settings country name changed
                if (metroTextBox6.Text != null || metroTextBox6.Text != string.Empty)
                {
                    Country country = Data.Countries[listBox1.SelectedIndex];
                    country.Name = metroTextBox6.Text;

                    Data.Save();

                    int index = listBox1.SelectedIndex;
                    RefreshCountriesList();
                    listBox1.SelectedIndex = index;
                }
                else
                {
                    metroTextBox6.Text = Data.Countries[listBox1.SelectedIndex].Name;
                    MetroMessageBox.Show(this, "The name of a country cannot be null", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            //settings countries list index changed
            Country country = Data.Countries[listBox1.SelectedIndex];
            if (File.Exists(country.Flag))
                pictureBox1.Image = Image.FromFile(country.Flag);
            else
                pictureBox1.Image = Properties.Resources.FlagNotAvailable;
            metroTextBox2.Text = country.Name;
        }

        private void metroTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            //activation enter key pressed
            if (e.KeyChar == '\r')
                metroButton2.PerformClick();
        }
    }
}
