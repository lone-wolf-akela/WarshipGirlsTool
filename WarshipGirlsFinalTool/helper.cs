using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;
using System.Xml.Linq;
using Data;
using NAudio.Wave;
using WarshipGirlsPNGTool;

namespace WarshipGirlsFinalTool
{
    public static class helper
    {
        public static string GetNewUDID()
        {
            var md5 = MD5.Create();
            byte[] Bytes = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
            byte[] hash = md5.ComputeHash(Bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
        public static string GetMD5FromFile(string fileName)
        {
            var file = new FileStream(fileName, FileMode.Open);
            var md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(file);
            file.Close();
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
        public static long GetFileLength(string fileName)
        {
            var file = new FileInfo(fileName);
            return file.Length;
        }

        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        private static extern long StrFormatByteSize(
                long fileSize
                , [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer
                , int bufferSize);


        /// <summary>
        /// Converts a numeric value into a string that represents the number expressed as a size value in bytes, kilobytes, megabytes, or gigabytes, depending on the size.
        /// </summary>
        /// <param name="filesize">The numeric value to be converted.</param>
        /// <returns>the converted string</returns>
        public static string StrFormatByteSize(long filesize)
        {
            StringBuilder sb = new StringBuilder(1024);
            StrFormatByteSize(filesize, sb, sb.Capacity);
            return sb.ToString();
        }
    }
    internal static class Extension
    {
        public static long ToUTC(this DateTime vDate)
        {
            vDate = vDate.ToUniversalTime();
            DateTime dtZone = new DateTime(1970, 1, 1, 0, 0, 0);
            return (long)vDate.Subtract(dtZone).TotalMilliseconds;
        }
    }

    public class Music
    {
        public void play(string music, bool fromBegin)
        {
            if (isPlaying != music)
            {
                stop();
                isPlaying = music;
                audioFile = new LoopStream(new AudioFileReader(@"documents\hot\audio\" + music));
                if (!fromBegin && timeRecord.ContainsKey(music))
                    audioFile.Position = timeRecord[music];
                waveOutDevice.Init(audioFile);
                waveOutDevice.Play();
            }
            else
            {
                if (fromBegin)
                    audioFile.Position = 0;
            }
        }
        public void stop()
        {
            if (isPlaying != "")
            {
                timeRecord[isPlaying] = audioFile.Position;
                waveOutDevice.Stop();
                isPlaying = "";
            }
        }

        private readonly Dictionary<string, long> timeRecord = new Dictionary<string, long>();
        private string isPlaying = "";

        private readonly IWavePlayer waveOutDevice = new WaveOut();
        private LoopStream audioFile;

        /// <summary>
        /// Stream for looping playback
        /// From http://mark-dot-net.blogspot.com/2009/10/looped-playback-in-net-with-naudio.html
        /// </summary>
        private class LoopStream : WaveStream
        {
            private readonly WaveStream sourceStream;

            /// <summary>
            /// Creates a new Loop stream
            /// </summary>
            /// <param name="sourceStream">The stream to read from. Note: the Read method of this stream should return 0 when it reaches the end
            /// or else we will not loop to the start again.</param>
            public LoopStream(WaveStream sourceStream)
            {
                this.sourceStream = sourceStream;
                this.EnableLooping = true;
            }

            /// <summary>
            /// Use this to turn looping on or off
            /// </summary>
            public bool EnableLooping { get; set; }

            /// <summary>
            /// Return source stream's wave format
            /// </summary>
            public override WaveFormat WaveFormat => sourceStream.WaveFormat;

            /// <summary>
            /// LoopStream simply returns
            /// </summary>
            public override long Length => sourceStream.Length;

            /// <summary>
            /// LoopStream simply passes on positioning to source stream
            /// </summary>
            public override long Position
            {
                get { return sourceStream.Position; }
                set { sourceStream.Position = value; }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int totalBytesRead = 0;

                while (totalBytesRead < count)
                {
                    int bytesRead = sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                    if (bytesRead == 0)
                    {
                        if (sourceStream.Position == 0 || !EnableLooping)
                        {
                            // something wrong with the source stream
                            break;
                        }
                        // loop
                        sourceStream.Position = 0;
                    }
                    totalBytesRead += bytesRead;
                }
                return totalBytesRead;
            }
        }
    }

    public class ImageFinder
    {
        private readonly List<Data.PList> plists = new List<PList>();

        public ImageFinder()
        {
            //string folder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            string[] files = Directory.GetFiles(@"documents\hot\ccbResources", "*.plist", SearchOption.AllDirectories);
            plists.AddRange(from file in files select new PList(file));
        }
        public Image getImage(string file)
        {
            if (File.Exists(file + ".muka"))
                return WSGPNG.getShipModel(file + ".muka");
            if (File.Exists(file + ".mukaR"))
                return WSGPNG.getShipModel(file + ".mukaR");
            if (File.Exists(file + ".png"))
                return Image.FromFile(file + ".png");

            var found = from plist in plists
                where
                    (from key in ((PList) plist["frames"]).Keys where key == file select key).Any()
                select plist;
            if (found.Any())
            {
                PList plist = found.First();
                Bitmap srcImage =
                    WSGPNG.getPVRCCZ(@"documents\hot\ccbResources\"
                                     + plist["metadata"]["realTextureFileName"]);

                string frame = plist["frames"][file]["frame"];
                string[] pos = frame.Replace("{", "").Replace("}", "").Split(',');

                Rectangle srcRect;
                if (plist["frames"][file]["rotated"])
                {
                    srcRect = new Rectangle(int.Parse(pos[0]), int.Parse(pos[1]),
                        int.Parse(pos[3]), int.Parse(pos[2]));
                    
                }
                else
                {
                    srcRect = new Rectangle(int.Parse(pos[0]), int.Parse(pos[1]),
                        int.Parse(pos[2]), int.Parse(pos[3]));
                }
                Bitmap surface = srcImage.Clone(srcRect, srcImage.PixelFormat);

                if (plist["frames"][file]["rotated"])
                    surface.RotateFlip(RotateFlipType.Rotate270FlipNone);

                return surface;
            }

            throw new Exception("Can't Find Picture: " + file);
        }
    }
}

//From https://www.codeproject.com/Tips/406235/A-Simple-PList-Parser-in-Csharp
namespace Data
{
    public class PList : Dictionary<string, dynamic>
    {
        public PList()
        {
        }

        public PList(string file)
        {
            Load(file);
        }

        public void Load(string file)
        {
            Clear();

            XDocument doc = XDocument.Load(file);
            XElement plist = doc.Element("plist");
            XElement dict = plist.Element("dict");

            var dictElements = dict.Elements();
            Parse(this, dictElements);
        }

        private void Parse(PList dict, IEnumerable<XElement> elements)
        {
            for (int i = 0; i < elements.Count(); i += 2)
            {
                XElement key = elements.ElementAt(i);
                XElement val = elements.ElementAt(i + 1);

                dict[key.Value] = ParseValue(val);
            }
        }

        private List<dynamic> ParseArray(IEnumerable<XElement> elements)
        {
            List<dynamic> list = new List<dynamic>();
            foreach (XElement e in elements)
            {
                dynamic one = ParseValue(e);
                list.Add(one);
            }

            return list;
        }

        private dynamic ParseValue(XElement val)
        {
            switch (val.Name.ToString())
            {
                case "string":
                    return val.Value;
                case "integer":
                    return int.Parse(val.Value);
                case "real":
                    return float.Parse(val.Value);
                case "true":
                    return true;
                case "false":
                    return false;
                case "dict":
                    PList plist = new PList();
                    Parse(plist, val.Elements());
                    return plist;
                case "array":
                    List<dynamic> list = ParseArray(val.Elements());
                    return list;
                default:
                    throw new ArgumentException("Unsupported");
            }
        }
    }
}