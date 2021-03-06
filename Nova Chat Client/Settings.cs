﻿using ChatLib.Extras;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Client.NotificationManager;

namespace Client
{
    public partial class Settings : Form
    {
        private Tcp_Client parent;
        private ObservableDictionary<string, object> settings;
        private NotificationManager notifications;

        public Settings(Tcp_Client parent, ref ObservableDictionary<string, object> settingsDictionary, NotificationManager notifications)
        {
            InitializeComponent();
            this.parent = parent;
            this.settings = settingsDictionary;
            this.notifications = notifications;

            Task.Run(() =>
            {
                ColorSelector.Items.Clear();

                PropertyInfo[] colors = typeof(Color).GetProperties();

                for (int i = 0; i < colors.Length; i++)
                {
                    if (colors[i].PropertyType == typeof(Color))
                    {
                        MethodInfo getMethod = colors[i].GetGetMethod();
                        if ((getMethod != null) && ((getMethod.Attributes & (MethodAttributes.Static | MethodAttributes.Public)) == (MethodAttributes.Static | MethodAttributes.Public)))
                        {
                            object[] index = null;
                            ColorSelector.Items.Add((Color)colors[i].GetValue(null, index));
                        }
                    }
                }
            });
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            ColorSelectorDisplay.Text = parent.GetFormattedTagColor();

            RoomSelector.Items.Clear();
            //RoomSelectorDisplay.Text = parent.CurrentRoom;

            if (parent.IsConnected())
            {
                foreach (KeyValuePair<string, Room> room in parent.Rooms)
                {
                    RoomSelector.Items.Add(room.Value.Name);
                }
            }

            if (bool.Parse(RegOps.GetSettingFromDict("ShowLog", settings).ToString()))
            {
                toggleLog.Text = "Hide Log";
            }
            else
            {
                toggleLog.Text = "Show Log";
            }

            if (parent.IsConnected())
            {
                RoomSelector.Enabled = true;
            }
            else
            {
                RoomSelector.Enabled = false;
            }

            if (settings.ContainsKey("NotificationStyle"))
            {
                switch(settings["NotificationStyle"])
                {
                    case NotificationType.Disabled:
                        radioButton1.Checked = false;
                        radioButton4.Checked = false;
                        radioButton2.Checked = false;
                        radioButton3.Checked = true;
                        groupBox3.Enabled = false;
                        break;

                    case NotificationType.SoundOnly:
                        radioButton1.Checked = false;
                        radioButton4.Checked = false;
                        radioButton2.Checked = true;
                        radioButton3.Checked = false;
                        groupBox3.Enabled = true;
                        break;

                    case NotificationType.ToastOnly:
                        radioButton1.Checked = false;
                        radioButton4.Checked = true;
                        radioButton2.Checked = false;
                        radioButton3.Checked = false;
                        groupBox3.Enabled = false;
                        break;

                    case NotificationType.Both:
                        radioButton1.Checked = true;
                        radioButton4.Checked = false;
                        radioButton2.Checked = false;
                        radioButton3.Checked = false;
                        groupBox3.Enabled = true;
                        break;

                    default:
                        radioButton1.Checked = true;
                        radioButton4.Checked = false;
                        radioButton2.Checked = false;
                        radioButton3.Checked = false;
                        groupBox3.Enabled = true;
                        break;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to reset all settings?" +
                "\n\n" +
                "This action cannot be undone.", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                RegOps.ResetSettings(ref settings);
            }
        }

        #region Client Settings

        private void toggleLog_Click(object sender, EventArgs e)
        {
            if (toggleLog.Tag != null)
            {
                if (toggleLog.Tag.ToString() == "true")
                {
                    parent.SetLogVisibility(false);
                    toggleLog.Tag = "false";
                    toggleLog.Text = "Show Log";
                    RegOps.WriteSetting("ShowLog", 0, RegistryValueKind.DWord, ref settings);
                }
                else
                {
                    parent.SetLogVisibility(true);
                    toggleLog.Tag = "true";
                    toggleLog.Text = "Hide Log";
                    RegOps.WriteSetting("ShowLog", 1, RegistryValueKind.DWord, ref settings);
                }
            }
            else
            {
                parent.SetLogVisibility(true);
                toggleLog.Tag = "true";
                toggleLog.Text = "Hide Log";
                RegOps.WriteSetting("ShowLog", 1, RegistryValueKind.DWord, ref settings);
            }
        }

        #endregion

        #region Chat Settings

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void ColorSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ColorSelector.SelectedItem != null)
            {
                NColor color = parent.ColorToNColor((Color)ColorSelector.SelectedItem);
                parent.ChangeTagColor(color.R, color.G, color.B);
                ColorSelectorDisplay.Text = parent.GetFormattedTagColor();
            }
        }

        private void ColorSelectorDisplay_Click(object sender, EventArgs e)
        {
            DialogResult result = colorDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                var color = parent.ChangeTagColor(colorDialog1.Color.R, colorDialog1.Color.G, colorDialog1.Color.B);

                foreach (Color c in ColorSelector.Items)
                {
                    Console.WriteLine(c.ToKnownColor().ToString(), colorDialog1.Color.ToKnownColor().ToString());
                    if (c.ToKnownColor() == colorDialog1.Color.ToKnownColor())
                    {
                        ColorSelector.SelectedItem = c;
                        break;
                    }
                    ColorSelector.SelectedIndex = -1;
                }

                ColorSelectorDisplay.Text = color;
            }
        }

        private void RoomSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (KeyValuePair<string, Room> room in parent.Rooms)
            {
                if (RoomSelector.Text == room.Value.Name)
                {
                    parent.ChangeRoom(room.Value.ID.ToString());
                    break;
                }
            }
        }

        #endregion

        #region Notification Buttons

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            RegOps.WriteSetting("NotificationStyle", "Both", RegistryValueKind.String, ref settings);
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            RegOps.WriteSetting("NotificationStyle", "SoundOnly", RegistryValueKind.String, ref settings);
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            RegOps.WriteSetting("NotificationStyle", "ToastOnly", RegistryValueKind.String, ref settings);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            RegOps.WriteSetting("NotificationStyle", "Disabled", RegistryValueKind.String, ref settings);
        }

        #endregion

        #region Sound Buttons

        private void radioButton5_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void radioButton6_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        #endregion
    }
}
