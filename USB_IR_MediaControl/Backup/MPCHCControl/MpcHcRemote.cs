using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;

namespace LateNightStupidities.MpcHcRemoteControl
{
    public class MpcHcRemote
    {
        #region Windows message definitions
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

        private struct COPYDATASTRUCT
        {
            public UIntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }
        #endregion

        #region Constants
        private const int WM_COPYDATA = 0x4A;

        private const uint CMD_CONNECT =                0x50000000;
        private const uint CMD_STATE =                  0x50000001;
        private const uint CMD_PLAYMODE =               0x50000002;
        private const uint CMD_NOWPLAYING =             0x50000003;
        private const uint CMD_LISTSUBTITLETRACKS =     0x50000004;
        private const uint CMD_LISTAUDIOTRACKS =        0x50000005;
        private const uint CMD_PLAYLIST =               0x50000006;

        private const uint CMD_OPENFILE =               0xA0000000;
        private const uint CMD_STOP =                   0xA0000001;
        private const uint CMD_CLOSEFILE =              0xA0000002;
        private const uint CMD_PLAYPAUSE =              0xA0000003;

        private const uint CMD_ADDTOPLAYLIST =          0xA0001000;
        private const uint CMD_CLEARPLAYLIST =          0xA0001001;
        private const uint CMD_STARTPLAYLIST =          0xA0001002;
        private const uint CMD_REMOVEFROMPLAYLIST =     0xA0001003;

        private const uint CMD_SETPOSITION =            0xA0002000;
        private const uint CMD_SETAUDIODELAY =          0xA0002001;
        private const uint CMD_SETSUBTITLEDELAY =       0xA0002002;
        private const uint CMD_SETINDEXPLAYLIST =       0xA0002003;
        private const uint CMD_SETAUDIOTRACK =          0xA0002004;
        private const uint CMD_SETSUBTITLETRACK =       0xA0002005;

        private const uint CMD_GETSUBTITLETRACKS =      0xA0003000;
        private const uint CMD_GETAUDIOTRACKS =         0xA0003001;
        private const uint CMD_GETNOWPLAYING =          0xA0003002;
        private const uint CMD_GETPLAYLIST =            0xA0003003;

        private const uint CMD_TOGGLEFULLSCREEN =       0xA0004000;
        private const uint CMD_JUMPFORWARDMED =         0xA0004001;
        private const uint CMD_JUMPBACKWARDMED =        0xA0004002;
        private const uint CMD_INCREASEVOLUME =         0xA0004003;
        private const uint CMD_DECREASEVOLUME =         0xA0004004;
        private const uint CMD_SHADER_TOGGLE =          0xA0004005;
        private const uint CMD_CLOSEAPP =               0xA0004006;
        #endregion

        #region Enums
        public enum LoadState
        {
            MLS_CLOSED,
            MLS_LOADING,
            MLS_LOADED,
            MLS_CLOSING
        }

        public enum PlayState
        {
            PS_PLAY = 0,
            PS_PAUSE = 1,
            PS_STOP = 2,
            PS_UNUSED = 3
        }
        #endregion

        #region Classes
        public class FileProperties
        {
            public string Title { get; set; }
            public string Author { get; set; }
            public string Description { get; set; }
            public string Path { get; set; }
            public TimeSpan Duration { get; set; }
        }

        private class MessageReceiver : Control
        {
            public MpcHcRemote Master { get; set; }

            public MessageReceiver(MpcHcRemote master) : base()
            {
                Master = master;
            }

            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_COPYDATA)
                {
                    COPYDATASTRUCT cds = (COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(COPYDATASTRUCT));

                    uint command = cds.dwData.ToUInt32();
                    string param = Marshal.PtrToStringAuto(cds.lpData);
                    string[] multiParam = param.Split(new string[] { "|" }, StringSplitOptions.None);

                    switch (cds.dwData.ToUInt32())
                    {
                        case CMD_CONNECT:
                            Master.MpcHandle = Int32.Parse(param);
                            break;
                        case CMD_STATE:
                            Master.MpcLoadState = (LoadState)Enum.Parse(typeof(LoadState), param);
                            break;
                        case CMD_PLAYMODE:
                            Master.MpcPlayState = (PlayState)Int32.Parse(param);
                            break;
                        case CMD_NOWPLAYING:
                            Master.MpcNowPlaying = new FileProperties
                            {
                                Title = multiParam[0],
                                Author = multiParam[1],
                                Description = multiParam[2],
                                Path = multiParam[3],
                                Duration = new TimeSpan(Int32.Parse(multiParam[4]) * TimeSpan.TicksPerSecond)
                            };
                            break;
                        case CMD_LISTSUBTITLETRACKS:
                            if (param == "-1" || param == "-2")
                            {
                                Master.MpcSubtitleTracks = new List<string>();
                                Master.MpcActiveSubtitleTrack = Int32.Parse(param);
                            }
                            else
                            {
                                Master.MpcSubtitleTracks = multiParam.ToList<string>();
                                Master.MpcSubtitleTracks.RemoveAt(Master.MpcSubtitleTracks.Count - 1);
                            }
                            break;
                        case CMD_LISTAUDIOTRACKS:
                            if (param == "-1" || param == "-2")
                            {
                                Master.MpcAudioTracks = new List<string>();
                                Master.MpcActiveAudioTrack = Int32.Parse(param);
                            }
                            else
                            {
                                Master.MpcAudioTracks = multiParam.ToList<string>();
                                Master.MpcAudioTracks.RemoveAt(Master.MpcAudioTracks.Count - 1);
                            }
                            break;
                        case CMD_PLAYLIST:
                            Master.MpcPlaylist = multiParam.ToList<string>();
                            Master.MpcActiveAudioTrack = Int32.Parse(multiParam[multiParam.Length - 1]);
                            Master.MpcPlaylist.RemoveAt(Master.MpcPlaylist.Count - 1);
                            break;
                        default:
                            break;
                    }
                }

                base.WndProc(ref m);
            }
        }
        #endregion

        #region General properties
        public string Path { get; set; }
        public int MpcHandle { get; set; }
        private MessageReceiver Slave { get; set; }
        #endregion

        #region MPC state properties
        public LoadState MpcLoadState { get; private set; }
        public PlayState MpcPlayState { get; private set; }
        public FileProperties MpcNowPlaying { get; private set; }

        public List<string> MpcSubtitleTracks { get; private set; }
        public int MpcActiveSubtitleTrack { get; private set; }

        public List<string> MpcAudioTracks { get; private set; }
        public int MpcActiveAudioTrack { get; private set; }

        public List<string> MpcPlaylist { get; private set; }
        public int MpcActivePlaylistItem { get; private set; }
        #endregion

        public MpcHcRemote(string path)
        {
            this.Path = path;
            this.MpcHandle = 0;
            this.Slave = new MessageReceiver(this);
        }

        #region Set methods
        public void StartApplication()
        {
            Process p = new Process();
            p.StartInfo.FileName = Path;
            p.StartInfo.Arguments = "/slave " + Slave.Handle.ToString();
            p.Start();
        }

        public void OpenFile(string path)
        {
            SendMpcMessage(CMD_OPENFILE, path);
        }

        public void StopPlayback()
        {
            SendMpcMessage(CMD_STOP);
        }

        public void CloseFile()
        {
            SendMpcMessage(CMD_CLOSEFILE);
        }

        public void PlayPausePlayback()
        {
            SendMpcMessage(CMD_PLAYPAUSE);
        }

        public void AddToPlaylist(string path)
        {
            SendMpcMessage(CMD_ADDTOPLAYLIST, path);
        }

        public void ClearPlaylist()
        {
            SendMpcMessage(CMD_CLEARPLAYLIST);
        }

        public void StartPlaylist()
        {
            SendMpcMessage(CMD_STARTPLAYLIST);
        }

        public void RemoveFromPlaylist(int index)
        {
            throw new NotImplementedException();
        }

        public void SetPosition(int seconds)
        {
            SendMpcMessage(CMD_SETPOSITION, seconds.ToString());
        }

        public void SetAudioDelay(int milliseconds)
        {
            SendMpcMessage(CMD_SETAUDIODELAY, milliseconds.ToString());
        }

        public void SetSubtitleDelay(int milliseconds)
        {
            SendMpcMessage(CMD_SETSUBTITLEDELAY, milliseconds.ToString());
        }

        public void SetPlaylistIndex(int index)
        {
            throw new NotImplementedException();
            //SendMpcMessage(CMD_SETINDEXPLAYLIST, index.ToString());
        }

        public void SetAudioTrack(int index)
        {
            SendMpcMessage(CMD_SETAUDIOTRACK, index.ToString());
        }

        public void SetSubtitleTrack(int index)
        {
            SendMpcMessage(CMD_SETSUBTITLETRACK, index.ToString());
        }

        public void ToggleFullscreen()
        {
            SendMpcMessage(CMD_TOGGLEFULLSCREEN);
        }

        public void JumpForward()
        {
            SendMpcMessage(CMD_JUMPFORWARDMED);
        }

        public void JumpBackward()
        {
            SendMpcMessage(CMD_JUMPBACKWARDMED);
        }

        public void IncreaseVolume()
        {
            SendMpcMessage(CMD_INCREASEVOLUME);
        }

        public void DecreaseVolume()
        {
            SendMpcMessage(CMD_DECREASEVOLUME);
        }

        public void ToggleShader()
        {
            SendMpcMessage(CMD_SHADER_TOGGLE);
        }

        public void CloseApplication()
        {
            SendMpcMessage(CMD_CLOSEAPP);
        }
        #endregion

        #region Receiverequest methods
        public void GetSubtitleTracks()
        {
            SendMpcMessage(CMD_GETSUBTITLETRACKS);
        }

        public void GetAudioTracks()
        {
            SendMpcMessage(CMD_GETAUDIOTRACKS);
        }

        public void GetNowPlaying()
        {
            SendMpcMessage(CMD_GETNOWPLAYING);
        }

        public void GetPlaylist()
        {
            SendMpcMessage(CMD_GETPLAYLIST);
        }
        #endregion

        #region Send methods
        private void SendMpcMessage(uint command)
        {
            SendMpcMessage(command, "");
        }

        private void SendMpcMessage(uint command, string parameter)
        {
            if (MpcHandle > 0)
            {
                parameter += (char)0;

                COPYDATASTRUCT cds;
                cds.dwData = (UIntPtr)command;
                cds.lpData = Marshal.StringToCoTaskMemAuto(parameter);
                cds.cbData = parameter.Length * Marshal.SystemDefaultCharSize;

                SendMessage(MpcHandle, WM_COPYDATA, Slave.Handle.ToInt32(), ref cds);
            }
        }
        #endregion

    }
}
