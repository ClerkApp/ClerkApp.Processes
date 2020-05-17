﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Clerk.Processes.WebExtractorToJson
{
    public static class Commons
    {
        public static string RemoveWhitespace(this string str)
        {
            return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

        public static List<string> ToListTrim(this string[] array)
        {
            if (array != null)
            {
                return Array.ConvertAll(array, p => p.Trim()).ToList();
            }

            return new List<string>();
        }

        public static Guid BuildGuid(this string str)
        {
            using var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.Default.GetBytes(str));
            return new Guid(hash);
        }
    }
}