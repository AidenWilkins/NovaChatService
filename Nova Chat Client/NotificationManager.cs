﻿using System;
using System.IO;
using System.Media;
using ToastLib;

namespace Client
{
    public class NotificationManager
    {

        public NotificationManager(Tcp_Client parent, ref ObservableDictionary<string, object> settingsDictionary)
        {
            this.parent = parent;
        }

        public enum NotificationType
        {
            Disabled,
            ToastOnly,
            SoundOnly,
            Both
        }

        SoundPlayer player;
        ToastGenerator generator = new ToastGenerator();
        public NotificationType SelectedStyle { get; set; }
        private string SoundLocation = "";

        string appName = "Nova Chat";
        private Tcp_Client parent;

        public void SetSoundLocation(string path)
        {
            if (File.Exists(path))
            {

            }
        }

        public void PlaySound()
        {
            player = new SoundPlayer();

            if (SoundLocation.Length <= 0)
            {
                player.Stream = Properties.Resources.Notification;
            }
            else
            {
                player.SoundLocation = SoundLocation;
            }

            try
            {
                player.Play();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            player.Dispose();
        }

        public void ShowNotification(string sender, string message)
        {
            if (!parent.debug)
            {
                switch (SelectedStyle)
                {
                    case NotificationType.SoundOnly:
                        PlaySound();
                        break;

                    case NotificationType.ToastOnly:
                        generator.MakeToast(appName, sender, message);
                        break;

                    case NotificationType.Both:
                        generator.MakeToast(appName, sender, message);
                        PlaySound();
                        break;
                }
            }
            else
            {
                generator.MakeToast(appName, sender, message);
                PlaySound();
            }
        }
    }
}
