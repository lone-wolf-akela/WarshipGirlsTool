using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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

        public static bool valueadd(this Pietschsoft.ProgressDialog pd)
        {
            if (pd.HasUserCancelled)
            {
                throw new Exception("用户取消了下载！");
            }
            pd.Value++;
            pd.Line2 = string.Format("已完成{0:0.00}%", 100*(double) pd.Value/(double) pd.Maximum);
            return true;
        }
    }
}