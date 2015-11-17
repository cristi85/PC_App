using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LateNightStupidities.MpcHcRemoteControl;

namespace MPCHCTest
{
    public partial class Form1 : Form
    {
        MpcHcRemote r;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            r = new MpcHcRemote(@"C:\Program Files (x86)\Combined Community Codec Pack\MPC\mpc-hc.exe");
            r.StartApplication();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            r.OpenFile(@"E:\Videos\Kurzfilme\Elektriker-Horst.AVI");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            r.GetAudioTracks();
            r.GetNowPlaying();
            r.GetPlaylist();
            r.GetSubtitleTracks();
            r.CloseApplication();
            return;
        }


    }
}
