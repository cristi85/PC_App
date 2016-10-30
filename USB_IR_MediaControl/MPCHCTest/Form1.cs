using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LateNightStupidities.MpcHcRemoteControl;
using System.Diagnostics;
using WinampFrontEndLib;
using System.IO.Ports;
using System.IO;

namespace MPCHCTest
{
    public partial class Form1 : Form
    {
        MpcHcRemote r;

        public Form1()
        {
            InitializeComponent();
            cB_SerialPorts.Items.AddRange(SerialPort.GetPortNames());
            if (cB_SerialPorts.Items.Count > 0)
            {
                cB_SerialPorts.SelectedIndex = 0;
                btn_Connect.Enabled = true;
            }
        }

        delegate void MPControlPlayPauseControlCallback();
        private void MPControlPlayPause()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.InvokeRequired)
            {
                MPControlPlayPauseControlCallback d = new MPControlPlayPauseControlCallback(MPControlPlayPause);
                this.Invoke(d, new object[] {});
            }
            else
            {
                this.r.PlayPausePlayback();
            }
        }

        delegate void MPControlStartControlCallback();
        private void MPControlStart()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.InvokeRequired)
            {
                MPControlStartControlCallback d = new MPControlStartControlCallback(MPControlStart);
                this.Invoke(d, new object[] { });
            }
            else
            {
                this.r = new MpcHcRemote(@"C:\Program Files (x86)\SVP\MPC-HC\mpc-hc.exe");
                this.r.StartApplication();
            }
        }

        delegate void MPControlDecreaseVolControlCallback();
        private void MPControlDecreaseVol()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.InvokeRequired)
            {
                MPControlDecreaseVolControlCallback d = new MPControlDecreaseVolControlCallback(MPControlDecreaseVol);
                this.Invoke(d, new object[] { });
            }
            else
            {
                this.r.DecreaseVolume();
            }
        }

        delegate void MPControlIncreaseVolControlCallback();
        private void MPControlIncreaseVol()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.InvokeRequired)
            {
                MPControlIncreaseVolControlCallback d = new MPControlIncreaseVolControlCallback(MPControlIncreaseVol);
                this.Invoke(d, new object[] { });
            }
            else
            {
                this.r.IncreaseVolume();
            }
        }

        delegate void MPControlJumpBackwardCallback();
        private void MPControlJumpBackward()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.InvokeRequired)
            {
                MPControlJumpBackwardCallback d = new MPControlJumpBackwardCallback(MPControlJumpBackward);
                this.Invoke(d, new object[] { });
            }
            else
            {
                this.r.JumpBackward();
            }
        }

        delegate void MPControlJumpForwardCallback();
        private void MPControlJumpForward()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.InvokeRequired)
            {
                MPControlJumpForwardCallback d = new MPControlJumpForwardCallback(MPControlJumpForward);
                this.Invoke(d, new object[] { });
            }
            else
            {
                this.r.JumpForward();
            }
        }

        delegate void MPControlToggleFullscreenCallback();
        private void MPControlToggleFullscreen()
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.InvokeRequired)
            {
                MPControlToggleFullscreenCallback d = new MPControlToggleFullscreenCallback(MPControlToggleFullscreen);
                this.Invoke(d, new object[] { });
            }
            else
            {
                this.r.ToggleFullscreen();
            }
        }

        delegate void Form1Controltextbox1Callback(String s);
        private void Form1Controltextbox1(String s)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.InvokeRequired)
            {
                Form1Controltextbox1Callback d = new Form1Controltextbox1Callback(Form1Controltextbox1);
                this.Invoke(d, new object[] { s});
            }
            else
            {
                //this.textBox1.Text += s;
                this.textBox1.AppendText(s);
            }
        }

        private void sp1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            int bytestoread = sp1.BytesToRead;
            byte sprcvdata = 0;
            Boolean flag_read_byte_success = true;
            if (bytestoread == 1)
            {
                try
                {
                    sprcvdata = (byte)sp1.ReadByte();
                }
                catch (Exception ex)
                {
                    flag_read_byte_success = false;
                    textBox1.AppendText("Failed to read byte from " + sp1.PortName + "\r\n");
                    textBox1.AppendText(ex.Message.ToString() + "\r\n");
                }
                if (flag_read_byte_success)
                {
                    switch (sprcvdata)
                    {
                        case (byte)'A':  //power
                            {
                                Process p = new Process();
                                p.StartInfo.FileName = "C:\\Windows\\System32\\shutdown.exe";
                                p.StartInfo.Arguments = "-s -t 30";
                                try
                                {
                                    p.Start();
                                }
                                catch (Exception ex)
                                {
                                    textBox1.AppendText("Failed to start C:\\Windows\\System32\\shutdown.exe \r\n");
                                    textBox1.AppendText(ex.Message.ToString() + "\r\n");
                                }
                                break;
                            }
                        // ========= Media Player Commands ==========
                        case (byte)'B':  // CH+ Button - Fast Forward
                            {
                                MPControlJumpForward();
                                Form1Controltextbox1("'B' - Media Player Fast Forward\r\n");
                                
                                break;
                            }
                        case (byte)'b':  // CH- Button - Rewind
                            {
                                MPControlJumpBackward();
                                Form1Controltextbox1("'b' - Media Player Rewind\r\n");
                                break;
                            }
                        case (byte)'C':  // Play/Pause
                            {
                                MPControlPlayPause();
                                Form1Controltextbox1("'C' - Media Player Play/Pause\r\n");
                                break;
                            }
                        case (byte)'D':  // Red button - Toggle fullscreen
                            {
                                MPControlToggleFullscreen();
                                break;
                            }
                        case (byte)'E':  // Vol+ Button - Volume Increase
                            {
                                MPControlIncreaseVol();
                                WinampLib.VolumeUp();
                                Form1Controltextbox1("'E' - Volume Up\r\n");
                                break;
                            }
                        case (byte)'e':  // Vol- Button - Volume Decrease
                            {
                                MPControlDecreaseVol();
                                WinampLib.VolumeDown();
                                Form1Controltextbox1("'e' - Volume Down\r\n");
                                break;
                            }
                        case (byte)'F':  // Brightness+ Button - Brightness Increase
                            {

                                break;
                            }
                        case (byte)'f':  // Brightness- Button - Brightness Decrease
                            {

                                break;
                            }
                        case (byte)'G':  // Contrast+ Button - Contrast Increase
                            {

                                break;
                            }
                        case (byte)'g':  // Contrast- Button - Contrast Decrease
                            {

                                break;
                            }
                        // ======== END Media Player Commands =========

                        // ======== Winamp Commands =========
                        case (byte)'H':  // Play Button - Play
                            {
                                WinampLib.Play();
                                Form1Controltextbox1("'H' - Winamp Play\r\n");
                                break;
                            }
                        case (byte)'h':  // Pause Button - Pause
                            {
                                WinampLib.Pause();
                                Form1Controltextbox1("'h' - Winamp Pause\r\n");
                                break;
                            }
                        case (byte)'I':  // Stop Button - Stop
                            {
                                WinampLib.Stop();
                                Form1Controltextbox1("'I' - Winamp Stop\r\n");
                                break;
                            }
                        case (byte)'j':  // Rewind Button - Rewind
                            {
                                WinampLib.Rewind5Sec();
                                Form1Controltextbox1("'j' - Winamp Rewind\r\n");
                                break;
                            }
                        case (byte)'J':  // Fast forward Button - Fast forward
                            {
                                WinampLib.Forward5Sec();
                                Form1Controltextbox1("'J' - Winamp Fast Forward\r\n");
                                break;
                            }
                        case (byte)'K':  // Next track Button - Next track
                            {
                                WinampLib.NextTrack();
                                Form1Controltextbox1("'K' - Winamp Next Track\r\n");
                                break;
                            }
                        case (byte)'k':  // Previous track Button - Previous track
                            {
                                WinampLib.PrevTrack();
                                Form1Controltextbox1("'k' - Winamp Previous Track\r\n");
                                break;
                            }
                        case (byte)'L':  // Mute Button - Mute
                            {
                                WinampLib.SetVolume(0);
                                Form1Controltextbox1("'L' - Winamp Mute\r\n");
                                break;
                            }
                        // ======== END Winamp Commands =========
                        case (byte)'X':  // unassigned command
                            {
                                Form1Controltextbox1("'X' - Unassigned Command!\r\n");
                                break;
                            }
                        default:
                            {
                                Form1Controltextbox1(sprcvdata.ToString() + " - Unknown Command!\r\n");
                                break;
                            }
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string MPHCPath = "";
            bool foundMPHCPath = false;
            try
            {
                StreamReader inifile = new System.IO.StreamReader("config.ini");
                string line;
                while ((line = inifile.ReadLine()) != null)
                {
                    if(line.Contains("MediaPlayerHCPath"))
                    {
                        line = line.TrimStart(' ');
                        line = line.Substring(17);
                        line = line.TrimStart(' ');
                        line = line.TrimEnd(' ');
                        MPHCPath = String.Copy(line);
                        foundMPHCPath = true;
                        break;
                    }
                }

                inifile.Close();
                if (foundMPHCPath)
                {
                    r = new MpcHcRemote(MPHCPath);
                    r.StartApplication();
                }
                else
                {
                    textBox1.AppendText("Could not find definition for \"MediaPlayerHCPath\" in config.ini file");
                }
            }
            catch (Exception ex)
            {
                textBox1.AppendText("Error: " + ex.Message.ToString() + "\r\n");
                if (foundMPHCPath)
                {
                    textBox1.AppendText("MediaPlayerHCPath: " + MPHCPath + "\r\n");
                }
            }
        }

        private void btn_Connect_Click(object sender, EventArgs e)
        {
            Boolean flag_port_open_success = true;
            sp1.BaudRate = 19200;
            sp1.DataBits = 8;
            sp1.StopBits = StopBits.One;
            sp1.Parity = Parity.None;
            sp1.PortName = cB_SerialPorts.Items[cB_SerialPorts.SelectedIndex].ToString();
            
            try
            {
                sp1.Open();
            }
            catch (Exception ex)
            {
                flag_port_open_success = false;
                textBox1.AppendText("Failed to open " + sp1.PortName + "\r\n");
                textBox1.AppendText(ex.Message.ToString() + "\r\n");
            }
            if (flag_port_open_success)
            {
                btn_Connect.Enabled = false;
                btn_Disconnect.Enabled = true;
                textBox1.AppendText("Connected to " + sp1.PortName + "\r\n");
            }
        }

        private void btn_Disconnect_Click(object sender, EventArgs e)
        {
            Boolean flag_port_closed_success = true;
            try
            {
                sp1.Close();
            }
            catch (Exception ex)
            {
                flag_port_closed_success = false;
                textBox1.AppendText("Failed to close " + sp1.PortName + "\r\n");
                textBox1.AppendText(ex.Message.ToString() + "\r\n");
            }
            if (flag_port_closed_success)
            {
                btn_Disconnect.Enabled = false;
                btn_Connect.Enabled = true;
                textBox1.AppendText("Closed " + sp1.PortName + "\r\n");
            }
        }
    }
}
