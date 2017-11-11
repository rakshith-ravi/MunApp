using System;
using System.IO;
using MunApp.Common;
using MetroFramework;
using System.Drawing;
using System.Windows.Forms;
using MetroFramework.Forms;
using MetroFramework.Drawing;
using MetroFramework.Controls;
using System.Collections.Generic;

namespace MunApp.Win32
{
    public partial class MainWindow : MetroForm
    {
        private int selectedIndex = -1;
        private List<MetroLink> links = new List<MetroLink>();
        private List<Control[]> tabsControls = new List<Control[]>();
        private MetroPanel linksPanel;
        private MetroPanel contentPanel;
        private Rectangle defaultRectangle;
        private CountDown gslTimer = new CountDown();
        private bool gslPointGiven = false;
        private CountDown mcTimer = new CountDown();
        private Topic mcTopic = null;
        private bool mcPointGiven = false;
        private CountDown unmcTimer = new CountDown();
        private CountDown sslTimer = new CountDown();

        public MainWindow()
        {
            InitializeComponent();
            InitializeTabs();
            InitializeControls();
            SelectedIndex = 0;
            MinimumSize = new Size(800, 600);
        }

        public event EventHandler TabChanged;

        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
            set
            {
                if(selectedIndex != value)
                {
                    selectedIndex = value;
                    RedoTabs();
                    if (TabChanged != null)
                        TabChanged(this, EventArgs.Empty);
                }
            }
        }

        private void InitializeTabs()
        {
            //initialize the sided tabbing system
            Controls.Remove(metroTabControl1);
            
            linksPanel = new MetroPanel();
            linksPanel.Name = "linksPanel";
            linksPanel.Size = new Size(150, 200);
            linksPanel.Dock = DockStyle.Left;
            linksPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            contentPanel = new MetroPanel();
            contentPanel.Name = "contentPanel";
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.BorderStyle = System.Windows.Forms.BorderStyle.None;

            for (int i = 0; i < metroTabControl1.TabPages.Count; i++)
            {
                TabPage page = metroTabControl1.TabPages[i];

                Control[] controlsToAdd = new Control[page.Controls.Count];
                page.Controls.CopyTo(controlsToAdd, 0);
                tabsControls.Add(controlsToAdd);

                MetroLink link = new MetroLink();
                link.Text = page.Text;
                link.TextAlign = ContentAlignment.MiddleCenter;
                link.Margin = new Padding(5);
                link.FontSize = MetroLinkSize.Tall;
                link.FontWeight = MetroLinkWeight.Light;
                if (i != 0)
                    link.Location = new Point(0, links[i - 1].Location.Y + links[i - 1].Size.Height + links[i - 1].Margin.Top);
                link.Size = new Size(linksPanel.Width, 50);
                link.Text = link.Text.Replace("  ", Environment.NewLine);
                link.UseCustomForeColor = true;
                link.Click += link_Click;
                links.Add(link);
            }

            for (int i = metroTabControl1.TabPages.Count - 1; i >= 0; i--)
                linksPanel.Controls.Add(links[i]);

            Controls.Add(contentPanel);
            Controls.Add(linksPanel);

            Width += linksPanel.Width;
        }

        private void InitializeControls()
        {
            //initialize the controls as required
            AutoCompleteStringCollection collection = new AutoCompleteStringCollection();
            collection.AddRange(Data.GetCountryNames().ToArray());
            textBox1.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            textBox1.AutoCompleteSource = AutoCompleteSource.CustomSource;
            textBox1.AutoCompleteCustomSource = collection;
            textBox2.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            textBox2.AutoCompleteSource = AutoCompleteSource.CustomSource;
            textBox2.AutoCompleteCustomSource = collection;
            textBox3.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            textBox3.AutoCompleteSource = AutoCompleteSource.CustomSource;
            textBox3.AutoCompleteCustomSource = collection;
            //initialize the listBoxes which should contain the countries
            RefreshCountriesList();
            metroComboBox1.Items.AddRange((object[])Enum.GetNames(typeof(MetroColorStyle)));
            metroComboBox1.SelectedIndex = 4;

            metroTextBox1.Text = Data.CommitteeName;
            metroTextBox5.Text = Data.GSLAgenda;
            metroTextBox4.Text = Data.SSLAgenda;

            metroLabel20.Text = gslTimer.Time;
            metroLabel18.Text = mcTimer.Time;
            metroLabel28.Text = unmcTimer.Time;
            metroLabel10.Text = sslTimer.Time;

            metroTabControl2.SelectedIndex = 0;

            if (Data.ExtraData != null && Data.ExtraData != string.Empty)
            {
                string[] datas = Data.ExtraData.Split(new string[] { "::" }, StringSplitOptions.None);
                metroToggle1.Checked = bool.Parse(datas[0]);
                switch (datas[1])
                {
                    case "Dark":
                        Theme = MetroThemeStyle.Dark;
                        break;
                    case "Light":
                        Theme = MetroThemeStyle.Light;
                        break;
                    default:
                    case "Default":
                        Theme = MetroThemeStyle.Default;
                        break;
                }
                switch (datas[2])
                {
                    case "Black":
                        Style = MetroColorStyle.Black;
                        break;
                    case "Blue":
                        Style = MetroColorStyle.Blue;
                        break;
                    default:
                    case "Default":
                        Style = MetroColorStyle.Default;
                        break;
                    case "Brown":
                        Style = MetroColorStyle.Brown;
                        break;
                    case "Green":
                        Style = MetroColorStyle.Green;
                        break;
                    case "Lime":
                        Style = MetroColorStyle.Lime;
                        break;
                    case "Magenta":
                        Style = MetroColorStyle.Magenta;
                        break;
                    case "Orange":
                        Style = MetroColorStyle.Orange;
                        break;
                    case "Pink":
                        Style = MetroColorStyle.Pink;
                        break;
                    case "Purple":
                        Style = MetroColorStyle.Purple;
                        break;
                    case "Red":
                        Style = MetroColorStyle.Red;
                        break;
                    case "Silver":
                        Style = MetroColorStyle.Silver;
                        break;
                    case "Teal":
                        Style = MetroColorStyle.Teal;
                        break;
                    case "White":
                        Style = MetroColorStyle.White;
                        break;
                    case "Yellow":
                        Style = MetroColorStyle.Yellow;
                        break;
                }
            }

            TabChanged += (sender, e) =>
            {
                Data.Save();
                metroLabel25.Text = Data.GSLAgenda;
                metroLabel12.Text = Data.SSLAgenda;
            };

            gslTimer.Tick += (sender, e) =>
            {
                metroLabel20.Text = gslTimer.Time;
            };
            mcTimer.Tick += (sender, e) =>
            {
                metroLabel18.Text = mcTimer.Time;
            };
            unmcTimer.Tick += (sender, e) =>
            {
                metroLabel28.Text = unmcTimer.Time;
            };
            sslTimer.Tick += (sender, e) =>
            {
                metroLabel10.Text = sslTimer.Time;
            };
            gslTimer.Elapsed += (sender, e) =>
            {
                metroButton14.Text = "&Start";
                MetroMessageBox.Show(this, "Time elapsed!", "Time elapsed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            mcTimer.Elapsed += (sender, e) =>
            {
                metroButton7.Text = "&Start";
                MetroMessageBox.Show(this, "Time elapsed!", "Time elapsed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            unmcTimer.Elapsed += (sender, e) =>
            {
                metroButton6.Text = "Start";
                MetroMessageBox.Show(this, "Time elapsed!", "Time elapsed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            sslTimer.Elapsed += (sender, e) =>
            {
                MetroMessageBox.Show(this, "Time elapsed!", "Time elapsed", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }

        private void link_Click(object sender, EventArgs e)
        {
            MetroLink link = (MetroLink)sender;
            int index = links.IndexOf(link);
            SelectedIndex = index;
        }

        private void RedoTabs()
        {
            //redo tabs and colours of the links
            SuspendLayout();
            foreach(MetroLink link in links)
            {
                link.ForeColor = MetroPaint.ForeColor.Link.Normal(Theme);
                if (link.Text == "GSL" || link.Text == "Moderated" + Environment.NewLine + "Caucus" || link.Text == "Unmoderated" + Environment.NewLine + "Caucus" || link.Text == "SSL")
                {
                    link.Enabled = Data.RollCallDone;
                    if (!Data.RollCallDone)
                        link.ForeColor = Color.Gray;
                }
            }
            links[SelectedIndex].ForeColor = MetroPaint.GetStyleColor(Style);

            Text = Data.CommitteeName + " - " + links[SelectedIndex].Text.Replace(Environment.NewLine, " ");
            Invalidate();
            contentPanel.Controls.Clear();
            Control[] controlsToAdd = tabsControls[SelectedIndex];
            contentPanel.Controls.AddRange(controlsToAdd);
            ResumeLayout(false);
            PerformLayout();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            //Alt + Tab or Alt + Ctrl + Tab
            if (keyData == (Keys.Tab | Keys.Control))
            {
                if (SelectedIndex != links.Count - 1)
                {
                    SelectedIndex++;
                }
                return true;
            }
            else if (keyData == (Keys.Tab | Keys.Control | Keys.Shift))
            {
                if (SelectedIndex > 0)
                {
                    SelectedIndex--;
                }
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void RefreshCountriesList()
        {
            //called when the list of countries changes and the list has to be updated
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            listBox4.Items.Clear();
            listBox5.Items.Clear();
            string[] countries = Data.GetCountryNames().ToArray();
            Country[] presentCountries = Data.GetPresentCountries().ToArray();
            listBox1.Items.AddRange(countries);
            listBox2.Items.AddRange(presentCountries);
            listBox3.Items.AddRange(presentCountries);
            listBox4.Items.AddRange(presentCountries);
            listBox5.Items.AddRange(countries);
            if (listBox1.Items.Count > 0)
                listBox1.SelectedIndex = 0;
            if (listBox2.Items.Count > 0)
                listBox2.SelectedIndex = 0;
            if (listBox3.Items.Count > 0)
                listBox3.SelectedIndex = 0;
            if (listBox4.Items.Count > 0)
                listBox4.SelectedIndex = 0;
            if (listBox5.Items.Count > 0)
                listBox5.SelectedIndex = 0;
        }

        private void metroToggle1_CheckedChanged(object sender, EventArgs e)
        {
            //fullscreen / unfullscreen
            if(metroToggle1.Checked)
            {
                if (WindowState == FormWindowState.Maximized)
                    WindowState = FormWindowState.Normal;
                Movable = false;
                defaultRectangle = new Rectangle(Location, Size);
                Rectangle rect = Screen.FromControl(this).Bounds;
                Location = rect.Location;
                Size = rect.Size;
            }
            else
            {
                this.Movable = true;
                Location = defaultRectangle.Location;
                Size = defaultRectangle.Size;
            }
            Data.ExtraData = metroToggle1.Checked + "::" + Theme.ToString() + "::" + Style.ToString();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //roll call list changed
            Country country = Data.Countries[listBox1.SelectedIndex];
            metroLabel2.Text = country.Name;
            if (File.Exists(country.Flag))
                pictureBox1.Image = Image.FromFile(country.Flag);
            else
                pictureBox1.Image = Properties.Resources.FlagNotAvailable;
        }

        private void metroTile1_Click(object sender, EventArgs e)
        {
            //roll call present clicked
            if(listBox1.SelectedIndex != -1)
            {
                Country country = Data.Countries[listBox1.SelectedIndex];
                country.Attendance = Attendance.Present;
                if (listBox1.SelectedIndex < listBox1.Items.Count - 1)
                {
                    listBox1.SelectedIndex++;
                }
                else
                {
                    Data.RollCallDone = true;
                    SelectedIndex++;
                    RedoTabs();
                    listBox2.Items.Clear();
                    listBox2.Items.AddRange(Data.GetPresentCountries().ToArray());
                    listBox3.Items.Clear();
                    listBox3.Items.AddRange(Data.GetPresentCountries().ToArray());
                    listBox4.Items.Clear();
                    listBox4.Items.AddRange(Data.GetPresentCountries().ToArray());
                }
            }
        }

        private void metroTile2_Click(object sender, EventArgs e)
        {
            //roll call present and voting clicked
            if (listBox1.SelectedIndex != -1)
            {
                Country country = Data.Countries[listBox1.SelectedIndex];
                country.Attendance = Attendance.PresentAndVoting;
                if (listBox1.SelectedIndex < listBox1.Items.Count - 1)
                {
                    listBox1.SelectedIndex++;
                }
                else
                {
                    Data.RollCallDone = true;
                    SelectedIndex++;
                    listBox2.Items.Clear();
                    listBox2.Items.AddRange(Data.GetPresentCountries().ToArray());
                    listBox3.Items.Clear();
                    listBox3.Items.AddRange(Data.GetPresentCountries().ToArray());
                    listBox4.Items.Clear();
                    listBox4.Items.AddRange(Data.GetPresentCountries().ToArray());
                }
            }
        }

        private void metroTile3_Click(object sender, EventArgs e)
        {
            //roll call absent clicked
            if (listBox1.SelectedIndex != -1)
            {
                Country country = Data.Countries[listBox1.SelectedIndex];
                country.Attendance = Attendance.Absent;
                if (listBox1.SelectedIndex < listBox1.Items.Count - 1)
                {
                    listBox1.SelectedIndex++;
                }
                else
                {
                    Data.RollCallDone = true;
                    SelectedIndex++;
                    listBox2.Items.Clear();
                    listBox2.Items.AddRange(Data.GetPresentCountries().ToArray());
                    listBox3.Items.Clear();
                    listBox3.Items.AddRange(Data.GetPresentCountries().ToArray());
                    listBox4.Items.Clear();
                    listBox4.Items.AddRange(Data.GetPresentCountries().ToArray());
                }
            }
        }

        private void metroTextBox1_TextChanged(object sender, EventArgs e)
        {
            //Settings committee name text changed
            Data.CommitteeName = metroTextBox1.Text;
        }

        private void metroComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //theme style changed
            MetroColorStyle[] styles = (MetroColorStyle[])Enum.GetValues(typeof(MetroColorStyle));
            Style = styles[metroComboBox1.SelectedIndex];
            metroStyleManager1.Style = styles[metroComboBox1.SelectedIndex];
            foreach (MetroLink link in links)
            {
                link.ForeColor = MetroPaint.ForeColor.Link.Normal(Theme);
            }
            if (links.Count > SelectedIndex && SelectedIndex != -1)
                links[SelectedIndex].ForeColor = MetroPaint.GetStyleColor(Style);
            Data.ExtraData = metroToggle1.Checked + "::" + Theme.ToString() + "::" + Style.ToString();
        }

        private void metroRadioButton1_CheckedChanged(object sender, EventArgs e)
        {
            //light theme checked
            Theme = MetroThemeStyle.Light;
            metroStyleManager1.Theme = MetroThemeStyle.Light;
            foreach(MetroLink link in links)
            {
                link.ForeColor = MetroPaint.ForeColor.Link.Normal(Theme);
            }
            links[SelectedIndex].ForeColor = MetroPaint.GetStyleColor(Style);
            Data.ExtraData = metroToggle1.Checked + "::" + Theme.ToString() + "::" + Style.ToString();
        }

        private void metroRadioButton2_CheckedChanged(object sender, EventArgs e)
        {
            //dark theme checked
            Theme = MetroThemeStyle.Dark;
            metroStyleManager1.Theme = MetroThemeStyle.Dark;
            foreach (MetroLink link in links)
            {
                link.ForeColor = MetroPaint.ForeColor.Link.Normal(Theme);
            }
            links[SelectedIndex].ForeColor = MetroPaint.GetStyleColor(Style);
            Data.ExtraData = metroToggle1.Checked + "::" + Theme.ToString() + "::" + Style.ToString();
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            //gsl list changed
            Country country = Data.GetPresentCountries()[listBox2.SelectedIndex];
            metroLabel21.Text = country.Name;
            gslTimer.Reset((int)numericUpDown4.Value, (int)numericUpDown3.Value);
            metroLabel20.Text = gslTimer.Time;
            metroButton14.Text = "&Start";
            gslPointGiven = false;
            if (File.Exists(country.Flag))
                pictureBox2.Image = Image.FromFile(country.Flag);
            else
                pictureBox2.Image = Properties.Resources.FlagNotAvailable;
        }

        private void metroTextBox5_TextChanged(object sender, EventArgs e)
        {
            //gsl agenda
            Data.GSLAgenda = metroTextBox5.Text;
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            //gsl search key up
            if(e.KeyCode == Keys.Enter && textBox1.Text != null && textBox1.Text != string.Empty && Data.CountryExists(textBox1.Text))
            {
                listBox2.SelectedIndex = Data.GetPresentCountries().IndexOf(Data.FindCountry(textBox1.Text));
            }
        }

        private void metroButton14_Click(object sender, EventArgs e)
        {
            //gsl start
            if (!gslPointGiven)
            {
                Data.Countries[listBox2.SelectedIndex].GSL++;
                gslPointGiven = true;
            }
            if (gslTimer.Running)
            {
                gslTimer.Pause();
                metroButton14.Text = "&Start";
            }
            else
            {
                gslTimer.Start();
                metroButton14.Text = "&Stop";
            }
        }

        private void metroButton13_Click(object sender, EventArgs e)
        {
            //gsl pause
            gslTimer.Pause();
        }

        private void metroButton15_Click(object sender, EventArgs e)
        {
            //gsl cancel
            if (gslPointGiven)
            {
                Data.Countries[listBox2.SelectedIndex].GSL--;
                gslPointGiven = false;
                gslTimer.Reset((int)numericUpDown4.Value, (int)numericUpDown3.Value);
                metroLabel20.Text = gslTimer.Time;
                metroButton14.Text = "&Start";
            }
        }

        private void metroButton4_Click(object sender, EventArgs e)
        {
            //gsl next
            if (listBox2.SelectedIndex < listBox2.Items.Count - 1)
            {
                listBox2.SelectedIndex++;
            }
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            //mc list changed
            Country country = Data.GetPresentCountries()[listBox3.SelectedIndex];
            mcTimer.Reset((int)numericUpDown2.Value, (int)numericUpDown1.Value);
            metroLabel18.Text = gslTimer.Time;
            metroButton7.Text = "&Start";
            metroLabel19.Text = country.Name;
            mcPointGiven = false;
            if (File.Exists(country.Flag))
                pictureBox3.Image = Image.FromFile(country.Flag);
            else
                pictureBox3.Image = Properties.Resources.FlagNotAvailable;
        }

        private void textBox2_KeyUp(object sender, KeyEventArgs e)
        {
            //mc search key up
            if (e.KeyCode == Keys.Enter && textBox2.Text != null && textBox2.Text != string.Empty && Data.CountryExists(textBox2.Text))
            {
                listBox3.SelectedIndex = Data.GetPresentCountries().IndexOf(Data.FindCountry(textBox2.Text));
            }
        }

        private void metroButton7_Click(object sender, EventArgs e)
        {
            //mc timer start
            if (!mcPointGiven)
            {
                mcTopic.SpeakingCountries.Add(Data.Countries[listBox3.SelectedIndex]);
                mcPointGiven = true;
            }
            if (mcTimer.Running)
            {
                mcTimer.Pause();
                metroButton7.Text = "&Start";
            }
            else
            {
                mcTimer.Start();
                metroButton7.Text = "&Stop";
            }
        }

        private void metroButton8_Click(object sender, EventArgs e)
        {
            //mc Timer cancel
            if (mcPointGiven)
            {
                Country currentCountry = Data.Countries[listBox2.SelectedIndex];
                if (mcTopic.SpeakingCountries.Contains(currentCountry))
                    mcTopic.SpeakingCountries.Remove(currentCountry);
                mcPointGiven = false;
                mcTimer.Reset((int)numericUpDown2.Value, (int)numericUpDown1.Value);
                metroLabel18.Text = mcTimer.Time;
                metroButton7.Text = "&Start";
            }
        }

        private void metroButton5_Click(object sender, EventArgs e)
        {
            //mc next
            if(listBox3.SelectedIndex < listBox3.Items.Count - 1)
            {
                listBox3.SelectedIndex++;
            }
        }

        private void metroButton6_Click(object sender, EventArgs e)
        {
            //unmc start
            unmcTimer.Start();
        }

        private void metroButton24_Click(object sender, EventArgs e)
        {
            //unmc stop
            unmcTimer.Reset((int)numericUpDown8.Value, (int)numericUpDown7.Value);
            metroLabel28.Text = unmcTimer.Time;
        }

        private void metroButton23_Click(object sender, EventArgs e)
        {
            //unmc pause
            unmcTimer.Pause();
        }
        
        private void listBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            //ssl list changed
            Country country = Data.GetPresentCountries()[listBox4.SelectedIndex];
            metroLabel11.Text = country.Name;
            if (File.Exists(country.Flag))
                pictureBox10.Image = Image.FromFile(country.Flag);
            else
                pictureBox10.Image = Properties.Resources.FlagNotAvailable;
        }

        private void metroTextBox4_TextChanged(object sender, EventArgs e)
        {
            //SSL agenda
            Data.SSLAgenda = metroTextBox4.Text;
        }

        private void textBox3_KeyUp(object sender, KeyEventArgs e)
        {
            //ssl search key up
            if (e.KeyCode == Keys.Enter && textBox3.Text != null && textBox3.Text != string.Empty && Data.CountryExists(textBox3.Text))
            {
                listBox4.SelectedIndex = Data.GetPresentCountries().IndexOf(Data.FindCountry(textBox3.Text));
            }
        }

        private void metroButton18_Click(object sender, EventArgs e)
        {
            //ssl timer start
            sslTimer.Start();
        }

        private void metroButton10_Click(object sender, EventArgs e)
        {
            //ssl timer pause
            sslTimer.Pause();
        }

        private void metroButton9_Click(object sender, EventArgs e)
        {
            //ssl timer stop
            sslTimer.Stop();
            metroLabel10.Text = sslTimer.Time;
            if (listBox4.SelectedIndex < listBox4.Items.Count - 1)
            {
                Data.GetPresentCountries()[listBox4.SelectedIndex].SSL++;
                listBox4.SelectedIndex++;
            }
        }

        private void metroButton20_Click(object sender, EventArgs e)
        {
            //ssl timer reset
            sslTimer.Reset((int)numericUpDown6.Value, (int)numericUpDown5.Value);
        }

        private void metroButton19_Click(object sender, EventArgs e)
        {
            //ssl timer cancel
            sslTimer.Stop();
            metroLabel10.Text = sslTimer.Time;
        }

        private void listBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            //settings countries list index changed
            Country country = Data.Countries[listBox5.SelectedIndex];
            if (File.Exists(country.Flag))
                pictureBox4.Image = Image.FromFile(country.Flag);
            else
                pictureBox4.Image = Properties.Resources.FlagNotAvailable;
            metroTextBox6.Text = country.Name;
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            //settings change country flag
            using(OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = false;
                ofd.Title = "Choose the image file to set as a flag";
                ofd.Filter = "Image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png";
                ofd.ShowDialog(this);
                Country country = Data.Countries[listBox5.SelectedIndex];
                country.Flag = ofd.FileName;
                pictureBox4.Image = Image.FromFile(country.Flag);
                Data.Countries[listBox5.SelectedIndex] = country;
                Data.Save();
            }
        }

        private void metroTextBox6_Leave(object sender, EventArgs e)
        {
            //leave settings country name textBox without saving (which is pressing enter)
            metroTextBox6.Text = Data.Countries[listBox5.SelectedIndex].Name;
        }

        private void metroTextBox6_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                //settings country name changed
                if (metroTextBox6.Text != null || metroTextBox6.Text != string.Empty)
                {
                    Country country = Data.Countries[listBox5.SelectedIndex];
                    country.Name = metroTextBox6.Text;

                    Data.Save();

                    int index = listBox5.SelectedIndex;
                    RefreshCountriesList();
                    listBox5.SelectedIndex = index;
                }
                else
                {
                    metroTextBox6.Text = Data.Countries[listBox5.SelectedIndex].Name;
                    MetroMessageBox.Show(this, "The name of a country cannot be null", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            //settings add country
            Country country = new Country();
            country.Name = "New Country " + (Data.Countries.Count + 1);
            country.Flag = "";
            Data.Countries.Add(country);

            Data.Save();

            RefreshCountriesList();
            listBox5.SelectedIndex = Data.Countries.IndexOf(country);
        }

        private void metroButton22_Click(object sender, EventArgs e)
        {
            //settings remove country
            Data.Countries.RemoveAt(listBox5.SelectedIndex);

            Data.Save();

            RefreshCountriesList();
            listBox5.SelectedIndex = listBox5.Items.Count - 1;
        }

        private void metroButton21_Click(object sender, EventArgs e)
        {
            //settings clear countries
            Data.Countries.Clear();
            Country country = new Country();
            country.Name = "New Country 1";
            country.Flag = "";
            listBox5.SelectedIndex = 0;
            Data.Countries.Add(country);
            Data.Save();

            RefreshCountriesList();
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
                        if(tfi.ShowDialog() == DialogResult.OK)
                        {
                            List<Country> countries = new List<Country>();
                            foreach(string item in tfi.AddedCountries)
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
                            listBox5.SelectedIndex = listBox5.Items.Count - 1;
                        }
                    }
                }
            }
        }

        private void metroButton2_Click(object sender, EventArgs e)
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
                            listBox5.SelectedIndex = listBox5.Items.Count - 1;
                        }
                    }
                }
            }
        }

        private void metroButton25_Click(object sender, EventArgs e)
        {
            //generate report
            List<String> lines = new List<String>();
            foreach(Country country in Data.Countries)
            {
                lines.Add("Country Name : " + country.Name);
                lines.Add("Attendance : " + country.Attendance);
                lines.Add("Number of times spoken in GSL : " + country.GSL);
                lines.Add("Number of times spoken in Moderated Caucus : " + country.MC);
                lines.Add("Number of times spoken in SSL : " + country.SSL);
                lines.Add("");
            }
            File.WriteAllLines("Report.txt", lines.ToArray());
        }

        private void metroButton26_Click(object sender, EventArgs e)
        {
            //MC propose new topic
            using (TopicChooser topicChooser = new TopicChooser())
            {
                if(topicChooser.ShowDialog() == DialogResult.OK)
                {
                    mcTopic = topicChooser.Topic;
                    metroLabel26.Text = mcTopic.Heading;
                    
                    metroButton26.Visible = false;

                    metroLabel26.Visible = true;
                    metroLabel3.Visible = true;
                    textBox2.Visible = true;
                    pictureBox3.Visible = true;
                    metroLabel19.Visible = true;
                    metroLabel18.Visible = true;
                    metroLabel9.Visible = true;
                    numericUpDown2.Visible = true;
                    metroLabel4.Visible = true;
                    numericUpDown1.Visible = true;
                    metroButton7.Visible = true;
                    metroButton5.Visible = true;
                    metroButton8.Visible = true;
                    metroButton27.Visible = true;
                    listBox3.Visible = true;
                }
            }
        }

        private void metroButton27_Click(object sender, EventArgs e)
        {
            //dismiss current MC topic
            metroButton26.Visible = true;

            metroLabel26.Visible = false;
            metroLabel3.Visible = false;
            textBox2.Visible = false;
            pictureBox3.Visible = false;
            metroLabel19.Visible = false;
            metroLabel18.Visible = false;
            metroLabel9.Visible = false;
            numericUpDown2.Visible = false;
            metroLabel4.Visible = false;
            numericUpDown1.Visible = false;
            metroButton7.Visible = false;
            metroButton5.Visible = false;
            metroButton8.Visible = false;
            metroButton27.Visible = false;
            listBox3.Visible = false;
        }

        private void metroButton28_Click(object sender, EventArgs e)
        {
            //reset all data
            if(MetroMessageBox.Show(this, "This will format all data and reset the application for first time use. All your data will lost. Are you sure you want to continue?", "Think twice before you act", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == DialogResult.Yes)
            {
                //reset all data
                File.Delete("Data.dat");
                Application.Exit();
            }
        }
    }
}
