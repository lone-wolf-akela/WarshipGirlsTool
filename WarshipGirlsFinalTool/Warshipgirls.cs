using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace WarshipGirlsFinalTool
{
    public class Warshipgirls
    {
        public class GameInfo
        {
            public string uid;
            public string username;
            public string exp;
            public string level;
            public int oil;
            public int ammo;
            public int steel;
            public int aluminium;
            public ExploreInfo exploreinfo = new ExploreInfo();
            public class ExploreInfo
            {
                public int exploreCount = 0;
                public string[] exploreId = new string[4];
                public string[] fleetId=new string[4];
                public long[] startTime = new long[4];
                public long[] endTime=new long[4];
            }
        }

        public GameInfo gameInfo;
        public int market { get; set; }
        public  int channel { get; set; }
        public string version { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        //public string udid { get; set; }

        public List<string> serverList = new List<string>();

        private const string device = @"Lone Wolf PC Client/0.0.1 (Windows 10)";
        

        private iniListItem version_txt = new iniListItem();
        private string loginServer;
        private iniListItem init_txt = new iniListItem();
        private iniListItem logininfo = new iniListItem();
        private string hf_skey;
        private iniListItem GameEnterInfo = new iniListItem();
        private iniListItem gameinfo = new iniListItem();

        private string gameServer;

        private string uriend()
        {
            return @"&market=" + market + @"&channel=" + channel + @"&version=" + version;
        }
        public void checkVer()
        {
            string uri = @"http://version.jr.moefantasy.com/index/checkVer/" +
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
            version_txt.decompile();
            loginServer = version_txt.i("loginServer").s();
            loginServer = loginServer.Replace(@"\/", @"/");
        }

        public void login()
        {
            string uri = loginServer+ @"index/passportLogin/&t="+ DateTime.Now.ToUTC() +
                @"&e="+helper.GetNewUDID()+@"&gz=1"+uriend();
            var request = WebRequest.Create(uri) as HttpWebRequest;
            request.Method = @"POST";                         
            request.ProtocolVersion = new Version(1, 1); 
            request.UserAgent = device;
            request.KeepAlive = true;
            request.ContentType = @"application/x-www-form-urlencoded";
            string param = @"username="+username+@"&pwd="+password;
            byte[] bs = Encoding.UTF8.GetBytes(param);
            using (Stream reqStream = request.GetRequestStream())
            {
                reqStream.Write(bs, 0, bs.Length);
            }
            var response = request.GetResponse() as HttpWebResponse;
            Stream responsestream = response.GetResponseStream();
            responsestream.ReadByte();
            responsestream.ReadByte();
            var sb = new StreamReader(new DeflateStream(responsestream, CompressionMode.Decompress));
            logininfo.text = sb.ReadToEnd();
            logininfo.decompile();
            hf_skey = logininfo.i("hf_skey").s();
            for (int i = 0; i < logininfo.i("serverList").Length(); i++)
            {
                serverList.Add(logininfo.i("serverList").i(i).i("name").s());
            }
        }

        private string StdGetRequest(string uri)
        {
            var request = WebRequest.Create(uri) as HttpWebRequest;
            request.Method = @"GET";
            request.ProtocolVersion = new Version(1, 1);
            request.UserAgent = device;
            request.KeepAlive = true;
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(new Uri(gameServer), new Cookie("hf_skey", hf_skey));
            request.CookieContainer.Add(new Uri(gameServer), new Cookie("path", @"/"));
            //request.CookieContainer.Add(new Uri(gameServer), new Cookie("QCLOUD", udid));

            var response = request.GetResponse() as HttpWebResponse;
            Stream responsestream = response.GetResponseStream();
            responsestream.ReadByte();
            responsestream.ReadByte();
            var sb=new StreamReader(new DeflateStream(responsestream, CompressionMode.Decompress));
            return sb.ReadToEnd();
        }
        public void enter(int server)
        {
            gameServer = logininfo.i("serverList").i(server).i("host").s().Replace(@"\/", @"/");
            string uri = gameServer + @"/index/login/" + hf_skey.Split('.')[0] + @"&t=" +
                         DateTime.Now.ToUTC() + @"&e=" + helper.GetNewUDID() + @"&gz=1" + uriend();

            GameEnterInfo.text = StdGetRequest(uri);
            GameEnterInfo.decompile();
            gameInfo = new GameInfo
            {
                uid = GameEnterInfo.i("user").i("uid").s(),
                username = GameEnterInfo.i("user").i("username").s(),
                exp = GameEnterInfo.i("user").i("exp").s(),
                level = GameEnterInfo.i("user").i("level").s(),
                oil = (int)GameEnterInfo.i("user").i("oil").n(),
                ammo = (int)GameEnterInfo.i("user").i("ammo").n(),
                steel = (int)GameEnterInfo.i("user").i("steel").n(),
                aluminium = (int)GameEnterInfo.i("user").i("aluminium").n()

            };
            //hf_skey = gameinfo.i("hf_skey").s();

            uri= gameServer+ @"api/initGame/&t="+ @"&t=" + DateTime.Now.ToUTC() + 
                @"&e=" + helper.GetNewUDID() + @"&gz=1" + uriend();
            gameinfo.text = StdGetRequest(uri);
            gameinfo.decompile();
            for (int i = 0; i < gameinfo.i("pveExploreVo").i("levels").Length(); i++)
            {
                gameInfo.exploreinfo.exploreId[i] = gameinfo.i("pveExploreVo").i("levels").
                    i(i).i("exploreId").s();
                gameInfo.exploreinfo.fleetId[i] = gameinfo.i("pveExploreVo").i("levels").
                    i(i).i("fleetId").s();
                gameInfo.exploreinfo.startTime[i] = (long) gameinfo.i("pveExploreVo").i("levels").
                    i(i).i("startTime").n();
                gameInfo.exploreinfo.endTime[i] = (long)gameinfo.i("pveExploreVo").i("levels").
                    i(i).i("endTime").n();
                gameInfo.exploreinfo.exploreCount = i + 1;
            }
        }
    }


    
    internal abstract class iniItem
    {
        public enum Type
        {
            list,
            array,
            str,
            num,
            boolean
        }

        public Type type;

        public string text;
        public abstract void decompile();
        public abstract string compile();
        public abstract string display(int indent);
        public virtual iniItem i(string s)
        {
            throw new NotImplementedException();
        }

        public virtual iniItem i(int n)
        {
            throw new NotImplementedException();
        }
        public virtual double n()
        {
            throw new NotImplementedException();
        }
        public virtual string s()
        {
            throw new NotImplementedException();
        }

        public virtual int Length()
        {
            throw new NotImplementedException();
        }
    }

    class iniListItem : iniItem
    {
        public Dictionary<string, iniItem> items = new Dictionary<string, iniItem>();

        public override void decompile()
        {
            type = Type.list;
            List<string> tmp = text.Substring(1, text.Length - 2).splitWithEscape(',');
            foreach (string str in tmp)
            {
                List<string> key_item = str.splitWithEscape(':');
                string key = key_item[0].Substring(1, key_item[0].Length - 2);
                string item = key_item[1];


                if (item.ToLower() == "true" || item.ToLower() == "false")
                    items.Add(key, new iniBoolItem());
                else
                    switch (item[0])
                    {
                        case '\"':
                            items.Add(key, new iniStrItem());
                            break;
                        case '[':
                            items.Add(key, new iniArrayItem());
                            break;
                        case '{':
                            items.Add(key, new iniListItem());
                            break;
                        default:
                            items.Add(key, new iniNumItem());
                            break;
                    }
                items[key].text = item;
                items[key].decompile();
            }
        }

        public override string compile()
        {
            text = items.Keys.Aggregate("{",
                (current, key) =>
                    current + "\"" + key + "\":" + items[key].compile() + ","
                ).subStrPos(1, -2)
                   + "}";
            return text;
        }

        public override string display(int indent)
        {
            return items.Keys.Aggregate("\n" + new string('\t', indent) + "{\n",
                (current, key) =>
                    current + new string('\t', indent + 1) + "\"" + key + "\":" +
                    items[key].display(indent + 1).TrimStart() + ",\n"
                ).subStrPos(1, -3) + "\n"
                   + new string('\t', indent) + "}";
        }

        public override iniItem i(string s)
        {
            return items[s];
        }
    }

    class iniArrayItem : iniItem
    {
        public List<iniItem> items = new List<iniItem>();

        public override void decompile()
        {
            type = Type.array;
            List<string> tmp = text.subStrPos(2, -2).splitWithEscape(',');
            if (tmp[0] == "")
            {
                items = null;
                return;
            }
            foreach (string item in tmp)
            {
                if(item.ToLower()=="true"||item.ToLower()=="false")
                    items.Add(new iniBoolItem());
                else
                    switch (item[0])
                    {
                        case '\"':
                            items.Add(new iniStrItem());
                            break;
                        case '[':
                            items.Add(new iniArrayItem());
                            break;
                        case '{':
                            items.Add(new iniListItem());
                            break;
                        default:
                            items.Add(new iniNumItem());
                            break;
                    }
                items.Last().text = item;
                items.Last().decompile();
            }
        }
        public override string compile()
        {
            text = items?.Aggregate("[",
                (current, item) =>
                    current + item.compile() + ","
                    ).subStrPos(1, -2)
                   + "]";
            return text;
        }

        public override string display(int indent)
        {
            return items?.Aggregate("\n" + new string('\t', indent) + "[\n",
                (current, item) =>
                    current + item.display(indent + 1) + ",\n"
                    ).subStrPos(1, -3) + "\n"
                   + new string('\t', indent) + "]";
        }

        public override iniItem i(int n)
        {
            return items[n];
        }

        public override int Length()
        {
            return items.Count;
        }
    }

    internal class iniStrItem : iniItem
    {
        public string str;

        public override void decompile()
        {
            type = Type.str;
            str = text.subStrPos(2, -2).Trim().DecodeEncodedNonAsciiCharacters();
        }
        public override string compile()
        {
            return text = "\"" + str + "\"";
        }

        public override string display(int indent)
        {
            return new string('\t', indent) + "\"" + str + "\"";
        }

        public override string s()
        {
            return str;
        }

        public override double n()
        {
            return Convert.ToDouble(str);
        }
    }

    internal class iniNumItem : iniItem
    {
        public double num;

        public override void decompile()
        {
            type = Type.num;
            num = double.Parse(text);
        }
        public override string compile()
        {
            return text = num.ToString();
        }

        public override string display(int indent)
        {
            return new string('\t', indent) + num.ToString();
        }

        public override double n()
        {
            return num;
        }
    }

    internal class iniBoolItem : iniItem
    {
        public bool val;

        public override void decompile()
        {
            type = Type.boolean;
            if (text.ToLower() == "true")
                val = true;
            else if (text.ToLower() == "false")
                val = false;
            else
                throw new Exception("Unknown Type!");
        }
        public override string compile()
        {
            return text = val ? "true" : "false";
        }

        public override string display(int indent)
        {
            return new string('\t', indent) + (val ? "true" : "false");
        }
    }
}


/*
GET http://s101.jr.moefantasy.com/pvp/getChallengeList/&t=1479296328750&e=26bc24e12d8310f0a0fc53a1ba3f7d78&gz=1&market=4&channel=0&version=2.7.0 HTTP/1.1
Accept-Encoding: identity
Cookie: hf_skey=1155974.1155974..1479296124.1.4dcc62be36a3547583d808d139cfce4f; path=/;QCLOUD=7743373c675546349b8c122773b96687
User-Agent: Dalvik/1.6.0 (Linux; U; Android 4.4.2; NoxW Build/KOT49H)
Host: s101.jr.moefantasy.com
Connection: Keep-Alive



GET http://s101.jr.moefantasy.com/active/getUserData/&t=1479296481020&e=b557feb0ca7b471d32315eb0402cfbd3&gz=1&market=4&channel=0&version=2.7.0 HTTP/1.1
Accept-Encoding: identity
Cookie: hf_skey=1155974.1155974..1479296124.1.4dcc62be36a3547583d808d139cfce4f; path=/;QCLOUD=7743373c675546349b8c122773b96687
User-Agent: Dalvik/1.6.0 (Linux; U; Android 4.4.2; NoxW Build/KOT49H)
Host: s101.jr.moefantasy.com
Connection: Keep-Alive


GET http://s101.jr.moefantasy.com/pve/getUserData/&t=1479296481559&e=774466808d678b51488deab2d29e27d3&gz=1&market=4&channel=0&version=2.7.0 HTTP/1.1
Accept-Encoding: identity
Cookie: hf_skey=1155974.1155974..1479296124.1.4dcc62be36a3547583d808d139cfce4f; path=/;QCLOUD=7743373c675546349b8c122773b96687
User-Agent: Dalvik/1.6.0 (Linux; U; Android 4.4.2; NoxW Build/KOT49H)
Host: s101.jr.moefantasy.com
Connection: Keep-Alive

GET http://s101.jr.moefantasy.com/campaign/getUserData/&t=1479296481314&e=968b968dee7c8dda8254278fb4d9e941&gz=1&market=4&channel=0&version=2.7.0 HTTP/1.1
Accept-Encoding: identity
Cookie: hf_skey=1155974.1155974..1479296124.1.4dcc62be36a3547583d808d139cfce4f; path=/;QCLOUD=7743373c675546349b8c122773b96687
User-Agent: Dalvik/1.6.0 (Linux; U; Android 4.4.2; NoxW Build/KOT49H)
Host: s101.jr.moefantasy.com
Connection: Keep-Alive

*/