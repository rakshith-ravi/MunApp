using System;
using MunApp.Common;
using MetroFramework;
using System.Drawing;
using MetroFramework.Forms;
using System.Windows.Forms;
using MetroFramework.Controls;
using System.Collections.Generic;

namespace MunApp.Win32
{
    public partial class TopicChooser : MetroForm
    {
        public Topic Topic { get; set; }

        public TopicChooser()
        {
            InitializeComponent();
            TopicGroup.Add(new TopicGroup(this));
            TopicGroup.Add(new TopicGroup(this));
        }

        private void metroButton1_Click(object sender, EventArgs e)
        {
            //add topic
            TopicGroup.Add(new TopicGroup(this));
        }

        private void metroButton2_Click(object sender, EventArgs e)
        {
            //remove topic
            if (TopicGroup.groups.Count > 1)
                TopicGroup.Remove(TopicGroup.groups[TopicGroup.groups.Count - 1]);
            else
                MetroMessageBox.Show(this, "You cannot remove the only topic!");
        }

        private void metroButton3_Click(object sender, EventArgs e)
        {
            //ok
            int index = TopicGroup.GetCheckedIndex();
            if (index >= 0)
            {
                foreach(TopicGroup group in TopicGroup.groups)
                {
                    Topic topic = new Topic();
                    topic.Heading = group.Topic.Text;
                    topic.Passed = group.Pass.Checked;
                    if (topic.Passed)
                        Topic = topic;
                    topic.ProposedBy = Data.Countries[group.ProposedBy.SelectedIndex];
                    topic.ProposedBy.MC.Add(topic);
                }
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MetroMessageBox.Show(this, "You must pass at least one topic to set the committee topic", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void metroButton4_Click(object sender, EventArgs e)
        {
            //cancel
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    public class TopicGroup
    {
        public static List<TopicGroup> groups = new List<TopicGroup>();

        public MetroLabel TopicNum { get; set; }
        public MetroTextBox Topic { get; set; }
        public MetroComboBox ProposedBy { get; set; }
        public MetroCheckBox Pass { get; set; }
        public TopicChooser parent { get; set; }

        public TopicGroup(TopicChooser parent)
        {
            this.parent = parent;
            TopicNum = new MetroLabel();
            Topic = new MetroTextBox();
            ProposedBy = new MetroComboBox();
            Pass = new MetroCheckBox();
        }

        public static void Add(TopicGroup tg)
        {
            groups.Add(tg);
            tg.TopicNum.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            tg.TopicNum.Size = new Size(50, 20);
            tg.TopicNum.Text = "Topic " + groups.Count;
            tg.TopicNum.Location = new Point(32, 60 + groups.Count * 40);
            tg.Topic.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            tg.Topic.Location = new Point(102, 60 + groups.Count * 40);
            tg.Topic.Size = new Size(tg.parent.Width - 307, 23);
            tg.ProposedBy.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            tg.ProposedBy.Items.AddRange(Data.Countries.ToArray());
            tg.ProposedBy.SelectedIndex = 0;
            tg.ProposedBy.Location = new Point(tg.parent.Width - 191, 57 + groups.Count * 40);
            tg.ProposedBy.Size = new Size(117, 10);
            tg.Pass.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            tg.Pass.Checked = false;
            tg.Pass.Text = " ";
            tg.Pass.Location = new Point(tg.parent.Width - 43, 60 + groups.Count * 40);
            tg.Pass.Size = new Size(28, 17);
            tg.Pass.CheckedChanged += tg.Pass_CheckedChanged;
            tg.parent.Controls.Add(tg.TopicNum);
            tg.parent.Controls.Add(tg.Topic);
            tg.parent.Controls.Add(tg.ProposedBy);
            tg.parent.Controls.Add(tg.Pass);
        }

        public static void Remove(TopicGroup tg)
        {
            groups.Remove(tg);
            tg.Pass.CheckedChanged -= tg.Pass_CheckedChanged;
            tg.parent.Controls.Remove(tg.TopicNum);
            tg.parent.Controls.Remove(tg.Topic);
            tg.parent.Controls.Remove(tg.ProposedBy);
            tg.parent.Controls.Remove(tg.Pass);
            tg.TopicNum.Dispose();
            tg.Topic.Dispose();
            tg.ProposedBy.Dispose();
            tg.Pass.Dispose();
        }

        public static bool IsOneChecked()
        {
            foreach(TopicGroup group in groups)
            {
                if (group.Pass.Checked)
                    return true;
            }
            return false;
        }

        public static int GetCheckedIndex()
        {
            for(int i = 0; i < groups.Count; i++)
            {
                if (groups[i].Pass.Checked)
                    return i;
            }
            return -1;
        }

        private void Pass_CheckedChanged(object sender, EventArgs e)
        {
            foreach (TopicGroup group in groups)
            {
                group.Topic.Enabled = group.ProposedBy.Enabled = group.Pass.Enabled = !Pass.Checked;
            }
            if (Pass.Checked)
            {
                Topic.Enabled = ProposedBy.Enabled = Pass.Enabled = Pass.Checked;
            }
        }
    }
}
