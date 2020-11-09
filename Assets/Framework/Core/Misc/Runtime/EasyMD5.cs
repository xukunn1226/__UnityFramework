using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    static public class EasyMD5
    {
        static private string GetHash(HashAlgorithm hashAlgorithm, byte[] data)
        {
            byte[] hash = hashAlgorithm.ComputeHash(data);

            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < hash.Length; ++i)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        static private string GetHash(HashAlgorithm hashAlgorithm, Stream data)
        {
            byte[] hash = hashAlgorithm.ComputeHash(data);

            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < hash.Length; ++i)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        static private bool VerifyHash(HashAlgorithm hashAlgorithm, byte[] data, string hash)
        {
            string hashOfData = GetHash(hashAlgorithm, data);
            return StringComparer.OrdinalIgnoreCase.Compare(hashOfData, hash) == 0;
        }

        static private bool VerifyHash(HashAlgorithm hashAlgorithm, Stream data, string hash)
        {
            string hashOfData = GetHash(hashAlgorithm, data);
            return StringComparer.OrdinalIgnoreCase.Compare(hashOfData, hash) == 0;
        }

        static public string Hash(string data)
        {
            using(MD5 md5 = MD5.Create())
            {
                return GetHash(md5, Encoding.UTF8.GetBytes(data));
            }
        }
        
        static public bool Verify(string data, string hash)
        {
            using(MD5 md5 = MD5.Create())
            {
                return VerifyHash(md5, Encoding.UTF8.GetBytes(data), hash);
            }
        }

        static public string Hash(Stream data)
        {
            using(MD5 md5 = MD5.Create())
            {
                return GetHash(md5, data);
            }
        }

        static public bool Verify(Stream data, string hash)
        {
            using(MD5 md5 = MD5.Create())
            {
                return VerifyHash(md5, data, hash);
            }
        }
    }
}