using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SoundFromATray
{
    public partial class Form1 : Form
    {
        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;

        private WaveOut waveOut = null;

        bool toggled = false;

        Dictionary<Keys, float> sounds = new Dictionary<Keys, float>
        {
            { Keys.Q, 261.63f },
            { Keys.D2, 277.18f },
            { Keys.W, 293.66f },
            { Keys.D3, 311.13f },
            { Keys.E, 329.63f },
            { Keys.R, 349.23f },
            { Keys.D5, 369.99f },
            { Keys.T, 392.00f },
            { Keys.D6, 415.30f },
            { Keys.Y, 440.00f },
            { Keys.D7, 466.16f },
            { Keys.U,493.88f },
            { Keys.I, 523.25f }
        };
        List<Hotkey> hotkeys = new List<Hotkey>();

        public Form1()
        {
            InitializeComponent();

            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = "Sound From A Tray";
            trayIcon.Icon = new Icon(SystemIcons.Application, 40, 40);

            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;

            ShowMessage("Sound From A Tray", "Ctrl+Shift+S to toggle :)", 1);

            Hotkey toggle = new Hotkey(Keys.S, true, true, false, false);
            toggle.Pressed += Toggle;

            if (!toggle.GetCanRegister(this))
            {
                ShowMessage("Sound From A Tray", "Failed to register hotkey", 1);
            }
            else
            {
                toggle.Register(this);
            }

            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public void Toggle(object sender, EventArgs e)
        {
            toggled = !toggled;

            if (toggled) EnableKeys();
            else DisableKeys();
        }

        public void EnableKeys()
        {
            ShowMessage("Sound From A Tray", "Enabled", 1);


            foreach (KeyValuePair<Keys, float> key in sounds)
            {
                Hotkey temp = new Hotkey(key.Key, false, false, false, false);

                temp.Pressed += Sound;

                if (!temp.GetCanRegister(this))
                {
                    ShowMessage("Sound From A Tray", "Failed to register hotkey", 1);
                }
                else
                {
                    temp.Register(this);
                }

                hotkeys.Add(temp);
            }
        }

        public void DisableKeys()
        {
            ShowMessage("Sound From A Tray", "Disabled", 1);

            foreach (Hotkey h in hotkeys)
            {
                h.Unregister();
            }

            hotkeys.Clear();
        }

        public void ShowMessage(string title, string message, int timeout)
        {
            trayIcon.BalloonTipTitle = title;
            trayIcon.BalloonTipText = message;
            trayIcon.ShowBalloonTip(timeout);
        }

        public void Sound(object sender, EventArgs e)
        {
            if (waveOut != null)
            {
                loop.CancelAsync();
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }

            var sineWaveProvider = new SineWaveProvider32();
            sineWaveProvider.SetWaveFormat(16000, 1); // 16kHz mono
            sineWaveProvider.Frequency = sounds[((Hotkey)sender).KeyCode];
            sineWaveProvider.Amplitude = 0.5f;
            waveOut = new WaveOut();
            waveOut.Init(sineWaveProvider);
            waveOut.Play();

            loop.RunWorkerAsync();
        }

        public void loop_DoWork(object sender, DoWorkEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() =>
                {
                    Thread.Sleep(32);

                    waveOut.Stop();
                    waveOut.Dispose();
                    waveOut = null;
                }));
            }
            else
            {
                while (waveOut != null)
                {
                    Thread.Sleep(16);

                    waveOut.Stop();
                    waveOut.Dispose();
                    waveOut = null;
                }
            }
        }
    }
}
