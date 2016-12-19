using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Newtonsoft.Json.Linq;

using static WarshipGirlsFinalTool.helper;

namespace WarshipGirlsFinalTool
{
    public class Warshipgirls
    {
        public string firstSever;
        public int market;
        public int channel;
        public string version;
        public string username;
        public string password;
        public enum LANG { SChinese , TChinese , English , Japanese , Thai }
        private string language;
        public LANG Language
        {
            set
            {
                switch (value)
                {
                    case LANG.SChinese: language = "SChinese"; break;
                    case LANG.TChinese: language = "TChinese"; break;
                    case LANG.English: language = "English"; break;
                    case LANG.Japanese: language = "Japanese"; break;
                    case LANG.Thai: language = "Thai"; break;
                    default: throw new ArgumentOutOfRangeException(nameof(value), value, null);
                }
            }
        }

        private const string device = @"Lone Wolf PC Client/0.0.3 (Windows 10) https://github.com/lone-wolf-akela/WarshipGirlsTool";

        private XmlDocument language_xml;

        private JsonText version_txt;
        public JsonText init_txt;
        private JsonText proj_manifest;
        public JsonText passportLogin_txt;
        private JsonText login_txt;
        public JsonText gameinfo;

        private string hf_skey;
        private string loginServer;
        private string gameServer;
        private string ResUrlWu;
        private string packageUrl;
        private string uriend()
        {
            return string.Format("&market={0}&channel={1}&version={2}",
                market, channel, version);
        }

        private JsonText CreateJsonText(string text)
        {
            var result = new JsonText(text);
            if (result["eid"] == null) return result;
            if (init_txt != null && (string) init_txt["errorCode"][(string) result["eid"]] != null)
                throw new Exception((string) init_txt["errorCode"][(string) result["eid"]]);
            else
                throw new Exception("eid:" + result["eid"]);
        }

        public void checkVer()
        {
            string uri = string.Format("{0}index/checkVer/{1}/{2}/{3}{4}",
                firstSever, version, market, channel, uriend());
            var request = WebRequest.Create(uri) as HttpWebRequest;
            request.Method = @"GET";
            request.ProtocolVersion = new Version(1, 1);
            request.UserAgent = device;
            request.KeepAlive = true;

            var response = request.GetResponse() as HttpWebResponse;
            Stream stream = response.GetResponseStream();
            var reader = new StreamReader(stream, Encoding.UTF8);

            version_txt = CreateJsonText(reader.ReadToEnd());
            loginServer = (string) version_txt["loginServer"];
            if (version_txt["ResUrlWu"] != null)
            {
                ResUrlWu = (string) version_txt["ResUrlWu"];
            }
            else
            {
                ResUrlWu = (string) version_txt["ResUrl"];
            }
        }

        public void getInitConfigs()
        {
            if (File.Exists(@"documents\init.txt"))
            {
                var sr = new StreamReader(@"documents\init.txt", Encoding.UTF8);
                init_txt = CreateJsonText(sr.ReadToEnd());
                sr.Close();
                if ((string) init_txt["DataVersion"] == (string) version_txt["DataVersion"])
                {
                    return;
                }
            }
            string uri = string.Format("{0}index/getInitConfigs/&t={1}&e={2}&gz=1{3}",
                loginServer, DateTime.Now.ToUTC(), helper.GetNewUDID(), uriend());
            var request = WebRequest.Create(uri) as HttpWebRequest;
            request.Method = @"GET";
            request.ProtocolVersion = new Version(1, 1);
            request.UserAgent = device;
            request.KeepAlive = true;
            var response = request.GetResponse() as HttpWebResponse;
            Stream responsestream = response.GetResponseStream();
            responsestream.ReadByte();
            responsestream.ReadByte();
            var sr2 = new StreamReader(new DeflateStream(responsestream, CompressionMode.Decompress));
            init_txt = CreateJsonText(sr2.ReadToEnd());
            var sw = new StreamWriter(@"documents\init.txt", false, Encoding.UTF8);
            sw.Write(init_txt.text);
            sw.Close();
        }

        public void downloadRes(Form parentForm)
        {
            if (File.Exists(@"documents\proj.manifest"))
            {
                using (var proj_manifest_Reader = 
                    new StreamReader(@"documents\proj.manifest", Encoding.UTF8))
                {
                    proj_manifest = CreateJsonText(proj_manifest_Reader.ReadToEnd());
                }
                packageUrl = (string) proj_manifest["packageUrl"];
                if ((string) proj_manifest["version"] == (string) version_txt["ResVersion"])
                {
                    return;
                }
            }
            var proj_manifest_Request = WebRequest.Create(ResUrlWu) as HttpWebRequest;
            proj_manifest_Request.Method = @"GET";
            proj_manifest_Request.ProtocolVersion = new Version(1, 1);
            proj_manifest_Request.UserAgent = device;
            proj_manifest_Request.KeepAlive = true;
            using (var proj_manifest_Reader =
                new StreamReader(proj_manifest_Request.GetResponse().GetResponseStream(),
                    Encoding.UTF8))
            {
                proj_manifest = CreateJsonText(proj_manifest_Reader.ReadToEnd());
            }
            packageUrl = (string) proj_manifest["packageUrl"];
            using (var proj_manifest_Writer =
                new StreamWriter(@"documents\proj.manifest", false, Encoding.UTF8))
            {
                proj_manifest_Writer.Write(proj_manifest.text);
            }

            //接下来比较检查每个需要下载的文件
            var filefilter = new string[]
            {
                
            };
            var toDownload = from file2chk in proj_manifest["hot"]
                             where !(from file in filefilter where file2chk["name"].ToString().ToLower().StartsWith(file.ToLower()) select file).Any()
                             where !File.Exists(@"documents\hot\" + file2chk["name"])
                             || file2chk["size"].ToString() != GetFileLength(@"documents\hot\" + file2chk["name"]).ToString()
                             || file2chk["md5"].ToString() != GetMD5FromFile(@"documents\hot\" + file2chk["name"])
                             select new
                             {
                                 uri = packageUrl + file2chk["name"] + @"?md5=" + file2chk["md5"],
                                 filename = @"documents\hot\" + file2chk["name"],
                                 size = long.Parse((string)file2chk["size"])
                             };

            long totalSize = toDownload.Sum(f => f.size);

            //准备进度条对话框
            var pd = new Pietschsoft.ProgressDialog(parentForm.Handle)
            {
                Title = "下载游戏资源",
                CancelMessage = "正在取消...",
                Maximum = (uint) totalSize,
                Value = 0,
                Line1 = "开始下载...",
                Line3 = "正在计算剩余时间..."
            };
            pd.ShowDialog(Pietschsoft.ProgressDialog.PROGDLG.Modal, Pietschsoft.ProgressDialog.PROGDLG.AutoTime, Pietschsoft.ProgressDialog.PROGDLG.NoMinimize);
            //开始下载
            //此处无法使用多线程并行下载：辣鸡幼明会强制断开连接
            long downloadedSize = 0;
            foreach (var file in toDownload)
            {
                if (pd.HasUserCancelled)
                {
                    throw new Exception("用户取消了下载！");
                }
                pd.Value = (uint)downloadedSize;
                pd.Line1 = "正在下载文件：" + Path.GetFileName(file.filename);
                pd.Line2 = string.Format("已下载{0}，共{1}", StrFormatByteSize(downloadedSize), StrFormatByteSize(totalSize));

                Directory.CreateDirectory(Path.GetDirectoryName(file.filename));

                var fileRequest = WebRequest.Create(file.uri) as HttpWebRequest;
                fileRequest.Method = @"GET";
                fileRequest.ProtocolVersion = new Version(1, 1);
                fileRequest.UserAgent = device;
                fileRequest.KeepAlive = true;

                using (var fileResponse = fileRequest.GetResponse().GetResponseStream())
                {
                    using (var fileWriter =
                        new FileStream(file.filename, FileMode.Create, FileAccess.Write))
                    {
                        var buffer = new byte[1024];
                        int size = fileResponse.Read(buffer, 0, buffer.Length);
                        while (size > 0)
                        {
                            fileWriter.Write(buffer, 0, size);
                            size = fileResponse.Read(buffer, 0, buffer.Length);
                        }
                    }
                }
                downloadedSize += file.size;
            }
            pd.CloseDialog();
        }

        public void passportLogin()
        {
            string uri = string.Format("{0}index/passportLogin/&t={1}&e={2}&gz=1{3}",
                loginServer, DateTime.Now.ToUTC(), helper.GetNewUDID(), uriend());
            var request = WebRequest.Create(uri) as HttpWebRequest;
            request.Method = @"POST";
            request.ProtocolVersion = new Version(1, 1);
            request.UserAgent = device;
            request.KeepAlive = true;
            request.ContentType = @"application/x-www-form-urlencoded";
            string param = @"username=" + username + @"&pwd=" + password;
            byte[] bs = Encoding.UTF8.GetBytes(param);
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }
            var response = request.GetResponse() as HttpWebResponse;
            Stream responsestream = response.GetResponseStream();
            responsestream.ReadByte();
            responsestream.ReadByte();
            var sr = new StreamReader(new DeflateStream(responsestream, CompressionMode.Decompress));
            passportLogin_txt = CreateJsonText(sr.ReadToEnd());
            hf_skey = (string) passportLogin_txt["hf_skey"];
        }

        private string StdGetRequest(string command)
        {
            string uri = string.Format("{0}{1}&t={2}&e={3}&gz=1{4}",
                gameServer, command, DateTime.Now.ToUTC(), helper.GetNewUDID(), uriend());
            var request = WebRequest.Create(uri) as HttpWebRequest;
            request.Method = @"GET";
            request.ProtocolVersion = new Version(1, 1);
            request.UserAgent = device;
            request.KeepAlive = true;
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(new Uri(gameServer), new Cookie("hf_skey", hf_skey));
            request.CookieContainer.Add(new Uri(gameServer), new Cookie("path", @"/"));

            var response = request.GetResponse() as HttpWebResponse;
            Stream responsestream = response.GetResponseStream();
            responsestream.ReadByte();
            responsestream.ReadByte();
            var sr = new StreamReader(new DeflateStream(responsestream, CompressionMode.Decompress));
            return sr.ReadToEnd();
        }

        public void login(int server)
        {
            gameServer = passportLogin_txt["serverList"][server]["host"].ToString();
            login_txt = CreateJsonText(StdGetRequest(@"/index/login/" + hf_skey.Split('.')[0]));
        }

        public void initGame()
        {
            gameinfo = CreateJsonText(StdGetRequest(@"api/initGame/"));
        }

        /////////////////////////////////////////////////////////////////////
        public enum ShipImageType
        {
            L,
            M,
            S
        }

        public Image getShipImage(string shipID, ShipImageType type, bool hasBG)
        {
            var shipinfo = (from ship in gameinfo["userShipVO"] where ship["id"].ToString() == shipID select ship).First();
            string shipModel;
            if (shipinfo["skin_cid"].ToString() != "0")
            {
                shipModel = (from skin in init_txt["ShipSkin"] where skin["cid"].ToString() == shipinfo["skin_cid"].ToString() select skin["skinId"].ToString()).First();
            }
            else
            {
                shipModel = (from ship in init_txt["shipCard"] where ship["cid"].ToString() == shipinfo["shipCid"].ToString() select ship["picId"].ToString()).First();
            }
            if (double.Parse((string) shipinfo["battleProps"]["hp"]) < Math.Ceiling(double.Parse((string) shipinfo["battlePropsBasic"]["hp"])/2))
            {
                shipModel = "BROKEN_" + shipModel;
            }
            else
            {
                shipModel = "NORMAL_" + shipModel;
            }
            switch (type)
            {
                case ShipImageType.L:
                    shipModel = @"documents\hot\ccbResources\model\L_" + shipModel;
                    break;
                case ShipImageType.M:
                    shipModel = @"documents\hot\ccbResources\model\M_" + shipModel;
                    break;
                case ShipImageType.S:
                    shipModel = @"documents\hot\ccbResources\model\S_" + shipModel;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            return imageFinder.getImage(shipModel);
        }

        public string getLangStr(string str)
        {
            if (language_xml == null)
            {
                language_xml = new XmlDocument();
                language_xml.Load(@"documents\hot\language.xml");
            }
            try
            {
                var node = language_xml.SelectSingleNode("root/" + str + "/" + language);
                return node?.InnerText ?? str;
            }
            catch (System.Xml.XPath.XPathException)
            {
                return str;
            }
        }
        /////////////////////////////////////////////////////////////////////
        public readonly Music music=new Music();
        public readonly ImageFinder imageFinder = new ImageFinder();
    }

    public class JsonText
    {
        public string text;
        public JObject obj;

        public JsonText(string txt)
        {
            text = txt;
            Parse();
        }

        public JToken this[string index] => obj[index];

        public void Parse()
        {
            obj = JObject.Parse(text);
        }

        public override string ToString()
        {
            return obj.ToString();
        }
    }
}

