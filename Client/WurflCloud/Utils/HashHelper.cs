/**
 * Copyright (c) 2015 ScientiaMobile Inc.
 *
 * The WURFL Cloud Client is intended to be used in both open-source and
 * commercial environments. To allow its use in as many situations as possible,
 * the WURFL Cloud Client is dual-licensed. You may choose to use the WURFL
 * Cloud Client under either the GNU GENERAL PUBLIC LICENSE, Version 2.0, or
 * the MIT License.
 *
 * Refer to the COPYING.txt file distributed with this package.
 */

using System;
using System.Security.Cryptography;
using System.Text;

namespace ScientiaMobile.WurflCloud.Utils
{
    public class HashHelper
    {
        public static String GetHash(String input)
        {
            var algo = MD5.Create();
            var hashValue = algo.ComputeHash(Encoding.ASCII.GetBytes(input));

            var sb = new StringBuilder();
            for (var i = 0; i < hashValue.Length; i++)
            {
                sb.Append(hashValue[i].ToString("X2"));
            }

            // For some reason, this method produces uppercase letters, but they need to be lowercase
            return sb.ToString().ToLower();

        }
    }
}