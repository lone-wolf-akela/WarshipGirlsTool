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
using System.Linq;
using System.Resources;
using System.Xml.Linq;
using Data;
using NAudio.Wave;
using NLua;
using WarshipGirlsPNGTool;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Windows.Media;

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

        //From https://adamprescott.net/2012/03/02/custom-shaped-windows-forms-from-images/
        public static Region GetRegionFromImg(Bitmap _img)
        {
            var rgn = new Region();
            rgn.MakeEmpty();
            var rc = new Rectangle(0, 0, 0, 0);
            bool inimage = false;
            for (int y = 0; y < _img.Height; y++)
            {
                for (int x = 0; x < _img.Width; x++)
                {
                    if (!inimage)
                    {
                        // if pixel is not transparent
                        if (_img.GetPixel(x, y).A != 0)
                        {
                            inimage = true;
                            rc.X = x;
                            rc.Y = y;
                            rc.Height = 1;
                        }
                    }
                    else
                    {
                        // if pixel is transparent
                        if (_img.GetPixel(x, y).A == 0)
                        {
                            inimage = false;
                            rc.Width = x - rc.X;
                            rgn.Union(rc);
                        }
                    }
                }
                if (inimage)
                {
                    inimage = false;
                    rc.Width = _img.Width - rc.X;
                    rgn.Union(rc);
                }
            }
            return rgn;
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
        public static string toHMS(this TimeSpan time)
        {
            return $"{(int) time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}";
        }

        public static ImageSource toImageSource(this Image img)
        {
            var bi = new BitmapImage();
            using (var ms = new MemoryStream())
            {
                img.Save(ms,ImageFormat.Png);
                ms.Position = 0;                
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = ms;
                bi.EndInit();
            }
            return bi;
        }
    }
    public class Music : IDisposable
    {
        private bool disposed = false;
        /// <summary>
        /// 实现IDisposable中的Dispose方法
        /// </summary>
        public void Dispose()
        {
            //必须为true
            Dispose(true);
            //通知垃圾回收机制不再调用终结器（析构器）
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 不是必要的，提供一个Close方法仅仅是为了更符合其他语言（如C++）的规范
        /// </summary>
        public void Close()
        {
            Dispose();
        }

        /// <summary>
        /// 必须，以备程序员忘记了显式调用Dispose方法
        /// </summary>
        ~Music()
        {
            //必须为false
            Dispose(false);
        }

        /// <summary>
        /// 非密封类修饰用protected virtual
        /// 密封类修饰用private
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            if (disposing)
            {
                // 清理托管资源
                waveOutDevice?.Stop();
                waveOutDevice?.Dispose();
                audioFile?.End();
                audioFile?.Close();
                luastate?.Dispose();
            }
            //让类型知道自己已经被释放
            disposed = true;
        }

        /********************************/
        public Music()
        {
            luastate.LoadCLRPackage();
            luastate.DoFile(@"mod\musicReplaceFunc.lua");
        }

        public void play(string music, bool fromBegin)
        {
            if (isPlaying != music)
            {
                if(isPlaying!="")
                    stop();
                isPlaying = music;

                var imageReplace = luastate["musicReplace"] as LuaFunction;
                var res = (string)imageReplace?.Call(music.ToLower())?.First();

                audioFile = new LoopStream(new AudioFileReader(res ?? @"documents\hot\audio\" + music));
                if (!fromBegin && timeRecord.ContainsKey(music))
                    audioFile.Position = timeRecord[music];
                waveOutDevice =new WaveOut();
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
                isPlaying = "";
                waveOutDevice.Stop();
                waveOutDevice.Dispose();
                waveOutDevice = null;
                audioFile.End();
                audioFile.Close();
                audioFile = null;
            }           
        }

        private readonly Dictionary<string, long> timeRecord = new Dictionary<string, long>();
        private string isPlaying = "";

        private IWavePlayer waveOutDevice;
        private LoopStream audioFile;

        private readonly Lua luastate = new Lua();

        /// <summary>
        /// Stream for looping playback
        /// From http://mark-dot-net.blogspot.com/2009/10/looped-playback-in-net-with-naudio.html
        /// </summary>
        private class LoopStream : WaveStream
        {
            public void End()
            {
                sourceStream?.Close();
            }
           /****************/
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
        private readonly List<PList> plists = new List<PList>();
        private readonly Dictionary<string, Image> cache = new Dictionary<string, Image>();
        private Lua luastate;
        public ImageFinder()
        {
            reset();
        }
        public void reset()
        {
            string[] files = Directory.GetFiles(@"documents\hot\ccbResources", "*.plist", SearchOption.AllDirectories);
            plists.Clear();
            plists.AddRange(from file in files select new PList(file));

            cache.Clear();

            luastate?.Dispose();
            luastate = new Lua();
            luastate.LoadCLRPackage();
            luastate.DoFile(@"mod\imageReplaceFunc.lua");
        }
        public Image getImage(string file)
        {
            Image ret;
            string filestr;
            string typestr;

            if (cache.TryGetValue(file, out ret))
                return ret;

            if (File.Exists(file + ".muka"))
            {
                filestr = file + ".muka";
                typestr = "shipmodel";
                ret = WSGPNG.getShipModel(filestr);
            }
            else if (File.Exists(file + ".mukaR"))
            {
                filestr = file + ".mukaR";
                typestr = "shipmodel";
                ret = WSGPNG.getShipModel(filestr);
            }
            else if (File.Exists(file + ".png"))
            {
                filestr = file + ".png";
                typestr = "png";
                ret = Image.FromFile(filestr);
            }
            else
            {
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

                    filestr = file;
                    typestr = "split";
                    ret = surface;
                }
                else
                    throw new Exception("Can't Find Picture: " + file);
            }

            var imageReplace = luastate["imageReplace"] as LuaFunction;
            var res = (string) imageReplace?.Call(filestr, ret, typestr)?.First();

            if (res != null)
            {
                ret = Image.FromFile(res);
            }

            cache.Add(file,ret);
            return ret;          
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