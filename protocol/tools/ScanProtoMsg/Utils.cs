using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ScanProtoMsg
{
    public class Utils
    {
        public static void ListDirectoryRecursively(string dir, List<FileInfo> list,string pattern = "")
        {
            DirectoryInfo d = new DirectoryInfo(dir);
            FileInfo[] files = d.GetFiles(pattern);
            DirectoryInfo[] directories = d.GetDirectories();
            foreach (FileInfo f in files)
            {
                list.Add(f);
            }
            foreach (DirectoryInfo dd in directories)
            {
                ListDirectoryRecursively(dd.FullName, list);
            }
        }

        public static void ListDirectory(string dir, List<FileInfo> list, string pattern = "")
        {
            DirectoryInfo d = new DirectoryInfo(dir);
            FileInfo[] files = d.GetFiles(pattern);
            DirectoryInfo[] directories = d.GetDirectories();
            foreach (FileInfo f in files)
            {
                list.Add(f);
            }

        }
    }
}
