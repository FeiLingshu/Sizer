using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Screen = System.Windows.Forms.Screen;
using WinFormRect = System.Drawing.Rectangle;

namespace Sizer
{
    public static class Settings
    {
        private static FileStream stream = null;

        public static void CheckFile(MainWindow UI)
        {
            string path = string.Empty;
            using (Process process = Process.GetCurrentProcess())
            {
                string mypath = process.MainModule.FileName;
                path = $"{Path.GetDirectoryName(mypath)}\\{Path.GetFileNameWithoutExtension(mypath)}.bin";
            }
            void WriteFile()
            {
                byte[] bytes = new byte[14];
                bytes[0] = (byte)0;
                bytes[1] = (byte)((0 >> 24) & 0xFF);
                bytes[2] = (byte)((0 >> 16) & 0xFF);
                bytes[3] = (byte)((0 >> 8) & 0xFF);
                bytes[4] = (byte)((0) & 0xFF);
                bytes[5] = (byte)((0 >> 24) & 0xFF);
                bytes[6] = (byte)((0 >> 16) & 0xFF);
                bytes[7] = (byte)((0 >> 8) & 0xFF);
                bytes[8] = (byte)((0) & 0xFF);
                bytes[9] = (byte)0;
                bytes[10] = (byte)((2400 >> 8) & 0xFF);
                bytes[11] = (byte)((2400) & 0xFF);
                bytes[12] = (byte)((1080 >> 8) & 0xFF);
                bytes[13] = (byte)((1080) & 0xFF);
                File.WriteAllBytes(path, bytes);
                UI.CacheList.Clear();
                UI.CacheList.Add((2400, 1080));
                UI.CacheIndex = 0;
            }
            if (File.Exists(path))
            {
                byte[] bytes = File.ReadAllBytes(path);
                if (bytes.Length >= 14 && bytes.Length % 2 == 0)
                {
                    if (bytes[0] == (byte)1)
                    {
                        UI.SettingIcon_1.Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0xC0, 0xC0, 0xC0));
                        UI.Setting_1.IsChecked = true;
                        int _1 = bytes[1] << 24 | bytes[2] << 16 | bytes[3] << 8 | bytes[4];
                        int _2 = bytes[5] << 24 | bytes[6] << 16 | bytes[7] << 8 | bytes[8];
                        bool flag = false;
                        foreach (Screen screen in Screen.AllScreens)
                        {
                            if (screen.WorkingArea.Contains(new WinFormRect(_1, _2, (int)UI.Width, (int)UI.Height)))
                            {
                                flag = true;
                                break;
                            }
                        }
                        if (flag)
                        {
                            UI.WindowStartupLocation = WindowStartupLocation.Manual;
                            UI.Left = _1;
                            UI.Top = _2;
                        }
                    }
                    if (bytes[9] == (byte)1)
                    {
                        UI.SettingIcon_2.Stroke = new SolidColorBrush(Color.FromArgb(0xFF, 0xC0, 0xC0, 0xC0));
                        UI.Setting_2.IsChecked = true;
                        UI.CacheList.Clear();
                        for (int i = 10; i < bytes.Length; i += 4)
                        {
                            UI.CacheList.Add(((short)(bytes[i] << 8 | bytes[i + 1]), (short)(bytes[i + 2] << 8 | bytes[i + 3])));
                        }
                        UI.CacheIndex = 0;
                        UI.SETWIDTH.Text = UI.CacheList[0].width.ToString();
                        UI.SETHEIGHT.Text = UI.CacheList[0].height.ToString();
                    }
                }
                else
                {
                    WriteFile();
                }
            }
            else
            {
                WriteFile();
            }
            stream = File.OpenWrite(path);
        }

        public enum Dataindex : long
        {
            WindowFlag = 0L,
            WindowData = 1L,
            SettingFlag = 9L,
            SettingData = 10L
        }

        public static void SaveData(Dataindex type, byte[] data)
        {
            if (stream != null && data != null)
            {
                stream.Seek((long)type, SeekOrigin.Begin);
                stream.Write(data, 0, data.Length);
            }
        }

        public static byte[] GetBytes(Type type, object data)
        {
            if (type == typeof(bool))
            {
                byte[] bytes = new byte[1] { (bool)data ? (byte)1 : (byte)0 };
                return bytes;
            }
            if (type == typeof(int))
            {
                int intdata = (int)data;
                byte[] bytes = new byte[4]
                {
                    (byte)((intdata >> 24) & 0xFF),
                    (byte)((intdata >> 16) & 0xFF),
                    (byte)((intdata >> 8) & 0xFF),
                    (byte)((intdata) & 0xFF)
                };
                return bytes;
            }
            if (type == typeof(Point))
            {
                Point pointdata = (Point)data;
                byte[] bytes = new byte[8]
                {
                    (byte)(((int)pointdata.X >> 24) & 0xFF),
                    (byte)(((int)pointdata.X >> 16) & 0xFF),
                    (byte)(((int)pointdata.X >> 8) & 0xFF),
                    (byte)(((int)pointdata.X) & 0xFF),
                    (byte)(((int)pointdata.Y >> 24) & 0xFF),
                    (byte)(((int)pointdata.Y >> 16) & 0xFF),
                    (byte)(((int)pointdata.Y >> 8) & 0xFF),
                    (byte)(((int)pointdata.Y) & 0xFF)
                };
                return bytes;
            }
            if (type == typeof(Size))
            {
                List<(short width, short height)> sizedata = (List<(short width, short height)>)data;
                byte[] bytes = new byte[sizedata.Count * 4];
                for (int i = 0; i < sizedata.Count; i++)
                {
                    bytes[i * 4] = (byte)((sizedata[i].width >> 8) & 0xFF);
                    bytes[i * 4 + 1] = (byte)((sizedata[i].width) & 0xFF);
                    bytes[i * 4 + 2] = (byte)((sizedata[i].height >> 8) & 0xFF);
                    bytes[i * 4 + 3] = (byte)((sizedata[i].height) & 0xFF);
                }
                return bytes;
            }
            return null;
        }

        public static void Dispose()
        {
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
                stream = null;
            }
        }
    }
}
