using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
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

        private const string device = @"Lone Wolf PC Client/0.0.1 (Windows 10)";

        private readonly JsonText version_txt = new JsonText();
        public readonly JsonText init_txt = new JsonText();
        private readonly JsonText proj_manifest = new JsonText();
        public readonly JsonText passportLogin_txt = new JsonText();
        private readonly JsonText login_txt = new JsonText();
        public readonly JsonText gameinfo = new JsonText();

        private string hf_skey;
        private string loginServer;
        private string gameServer;
        private string ResUrlWu;
        private string packageUrl;

        private string uriend()
        {
            return @"&market=" + market + @"&channel=" + channel + @"&version=" + version;
        }
        public void checkVer()
        {
            string uri = firstSever + @"index/checkVer/" +
                         version + @"/" + market + @"/" + channel + uriend();
            var request = WebRequest.Create(uri) as HttpWebRequest;
            request.Method = @"GET";
            request.ProtocolVersion = new Version(1, 1);
            request.UserAgent = device;
            request.KeepAlive = true;

            var response = request.GetResponse() as HttpWebResponse;
            Stream stream = response.GetResponseStream();
            var reader = new StreamReader(stream, Encoding.UTF8);
            version_txt.text = reader.ReadToEnd();
            version_txt.Parse();
            loginServer = version_txt["loginServer"].ToString().Replace(@"\/", @"/");
            if (version_txt["ResUrlWu"] != null)
            {
                ResUrlWu = version_txt["ResUrlWu"].ToString().Replace(@"\/", @"/");
            }
            else
            {
                ResUrlWu = version_txt["ResUrl"].ToString().Replace(@"\/", @"/");
            }
        }
        public void getInitConfigs()
        {
            if (File.Exists(@"documents\init.txt"))
            {
                var sr = new StreamReader(@"documents\init.txt", Encoding.UTF8);
                init_txt.text = sr.ReadToEnd();
                sr.Close();
                init_txt.Parse();
                if ((string)init_txt["DataVersion"] == (string)version_txt["DataVersion"])
                {
                    return;
                }
            }
            string uri = loginServer + @"index/getInitConfigs/&t=" + DateTime.Now.ToUTC() +
                         @"&e=" + helper.GetNewUDID() + @"&gz=1&market=" + market +
                         @"&channel=" + channel + @"&version=" + version;
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
            init_txt.text = sr2.ReadToEnd();
            init_txt.Parse();
            var sw = new StreamWriter(@"documents\init.txt", false, Encoding.UTF8);
            sw.Write(init_txt.text);
            sw.Close();
        }

        struct DownloadFile
        {
            public string uri;
            public string filename;
            public long size;
        }
        public void downloadRes(Form parentForm)
        {
            if (File.Exists(@"documents\proj.manifest"))
            {
                var sr = new StreamReader(@"documents\proj.manifest", Encoding.UTF8);
                proj_manifest.text = sr.ReadToEnd();
                sr.Close();
                proj_manifest.Parse();
                packageUrl = (string)proj_manifest["packageUrl"];
                if ((string)proj_manifest["version"] == (string)version_txt["ResVersion"])
                {
                    return;
                }
            }
            var request = WebRequest.Create(ResUrlWu) as HttpWebRequest;
            request.Method = @"GET";
            request.ProtocolVersion = new Version(1, 1);
            request.UserAgent = device;
            request.KeepAlive = true;
            var response = request.GetResponse() as HttpWebResponse;
            Stream responsestream = response.GetResponseStream();
            var sr2 = new StreamReader(responsestream, Encoding.UTF8);
            proj_manifest.text = sr2.ReadToEnd();
            proj_manifest.Parse();
            packageUrl = (string)proj_manifest["packageUrl"];
            var sw = new StreamWriter(@"documents\proj.manifest", false, Encoding.UTF8);
            sw.Write(proj_manifest.text);
            sw.Close();

            //准备进度条对话框
            var pd = new Pietschsoft.ProgressDialog(parentForm.Handle);
            //准备进度条对话框
            pd = new Pietschsoft.ProgressDialog(parentForm.Handle);
            pd.Title = "检查游戏资源更新";
            pd.CancelMessage = "正在取消...";
            pd.Maximum = (uint) proj_manifest["hot"].Count();
            pd.Value = 0;
            pd.Line1 = "已完成0%";
            pd.Line3 = "正在计算剩余时间...";

            pd.ShowDialog(Pietschsoft.ProgressDialog.PROGDLG.Modal, Pietschsoft.ProgressDialog.PROGDLG.AutoTime, Pietschsoft.ProgressDialog.PROGDLG.NoMinimize);

            //接下来比较检查每个需要下载的文件
            var toDownload = from file2chk in proj_manifest["hot"]
                             where
                                 (file2chk["name"].ToString().StartsWith(@"ccbResources/model/M_") ||
                                 file2chk["name"].ToString().StartsWith(@"ccbResources/ship_star_bg")) &&
                                 (!File.Exists(@"documents\hot\" + file2chk["name"]) ||
                                  file2chk["size"].ToString() !=
                                  GetFileLength(@"documents\hot\" + file2chk["name"]).ToString() ||
                                  file2chk["md5"].ToString() !=
                                  GetMD5FromFile(@"documents\hot\" + file2chk["name"])
                                     )
                             where pd.valueadd()
                             select new DownloadFile
                             {
                                 uri = packageUrl + file2chk["name"] + @"?md5=" + file2chk["md5"],
                                 filename = @"documents\hot\" + file2chk["name"],
                                 size = long.Parse((string)file2chk["size"])
                             };
            pd.CloseDialog();

            long totalSize = toDownload.Sum(f => f.size);

            //准备进度条对话框
            pd = new Pietschsoft.ProgressDialog(parentForm.Handle);
            pd.Title = "下载游戏资源";
            pd.CancelMessage = "正在取消...";
            pd.Maximum = (uint)totalSize;
            pd.Value = 0;
            pd.Line1 = "开始下载...";
            pd.Line3 = "正在计算剩余时间...";

            pd.ShowDialog(Pietschsoft.ProgressDialog.PROGDLG.Modal, Pietschsoft.ProgressDialog.PROGDLG.AutoTime, Pietschsoft.ProgressDialog.PROGDLG.NoMinimize);

            //开始下载
            long downloadedSize = 0;
            foreach (DownloadFile file in toDownload)
            {
                if (pd.HasUserCancelled)
                {
                    throw new Exception("用户取消了下载！");
                }
                pd.Value = (uint)downloadedSize;
                pd.Line1 = "正在下载文件：" + Path.GetFileName(file.filename);
                pd.Line2 = string.Format("已下载{0}，共{1}",
                    StrFormatByteSize(downloadedSize), StrFormatByteSize(totalSize));

                Directory.CreateDirectory(Path.GetDirectoryName(file.filename));

                request = WebRequest.Create(file.uri) as HttpWebRequest;
                request.Method = @"GET";
                request.ProtocolVersion = new Version(1, 1);
                request.UserAgent = device;
                request.KeepAlive = true;

                response = request.GetResponse() as HttpWebResponse;
                responsestream = response.GetResponseStream();
                var fs = new FileStream(file.filename, FileMode.Create, FileAccess.Write);
                var buffer = new byte[1024];
                int size = responsestream.Read(buffer, 0, buffer.Length);
                while (size > 0)
                {
                    fs.Write(buffer, 0, size);
                    size = responsestream.Read(buffer, 0, buffer.Length);
                }
                fs.Close();
                downloadedSize += file.size;
            }
            pd.CloseDialog();
        }
        public void passportLogin()
        {
            string uri = loginServer + @"index/passportLogin/&t=" + DateTime.Now.ToUTC() +
                         @"&e=" + GetNewUDID() + @"&gz=1" + uriend();
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
            passportLogin_txt.text = sr.ReadToEnd();
            passportLogin_txt.Parse();
            hf_skey = (string)passportLogin_txt["hf_skey"];
        }

        private string StdGetRequest(string command)
        {
            string uri = gameServer + command + @"&t=" + DateTime.Now.ToUTC() +
                @"&e=" + helper.GetNewUDID() + @"&gz=1" + uriend();
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
            gameServer = passportLogin_txt["serverList"][server]["host"].ToString().Replace(@"\/", @"/");
            login_txt.text = StdGetRequest(@"/index/login/" + hf_skey.Split('.')[0]);
            login_txt.Parse();

        }
        public void initGame()
        {
            gameinfo.text = StdGetRequest(@"api/initGame/");
            gameinfo.Parse();
        }
    }

    public class JsonText
    {
        public string text;
        public JObject obj;

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

