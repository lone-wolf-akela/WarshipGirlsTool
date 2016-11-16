using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

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
    }
    internal static class Extension
    {
        public static long ToUTC(this DateTime vDate)
        {
            //TimeZone tz = TimeZone.CurrentTimeZone;
            vDate = vDate.ToUniversalTime();
            DateTime dtZone = new DateTime(1970, 1, 1, 0, 0, 0);
            return (long)vDate.Subtract(dtZone).TotalMilliseconds;
        }
        public static List<string> splitWithEscape(this string s, char c)
        {
            var result = new List<string>();
            int Last = 0;
            bool qutation = false;
            int square = -1, brace = -1;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '\"' && s[i == 0 ? 0 : i - 1] != '\\')
                    qutation = !qutation;
                if (!qutation)
                {
                    switch (s[i])
                    {
                        case '[':
                            square++;
                            break;
                        case ']':
                            square--;
                            break;
                        case '{':
                            brace++;
                            break;
                        case '}':
                            brace--;
                            break;
                    }

                }
                if (qutation || square >= 0 || brace >= 0 || s[i] != c) continue;
                result.Add(s.Substring(Last, i - Last));
                Last = i + 1;
            }
            result.Add(s.Substring(Last));
            return result;
        }
        public static string EncodeNonAsciiCharacters(this string value)
        {
            var sb = new StringBuilder();
            foreach (char c in value)
            {
                if (c > 127)
                {
                    // This character is too big for ASCII
                    string encodedValue = "\\u" + ((int)c).ToString("x4");
                    sb.Append(encodedValue);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private static readonly Regex DECODING_REGEX = new Regex(@"\\u(?<Value>[a-fA-F0-9]{4})", RegexOptions.Compiled);
        private const string PLACEHOLDER = @"#!#";
        public static string DecodeEncodedNonAsciiCharacters(this string value)
        {
            return DECODING_REGEX.Replace(
                value.Replace(@"\\", PLACEHOLDER),
                m => ((char)int.Parse(m.Groups["Value"].Value, NumberStyles.HexNumber)).ToString())
                .Replace(PLACEHOLDER, @"\\");
        }

        public static string subStrPos(this string s, int beg, int end)
        {
            beg = beg > 0 ? beg - 1 : s.Length + beg;
            end = end > 0 ? end - 1 : s.Length + end;
            return s.Substring(beg, end - beg + 1);
        }
    }
}