using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SyncStreamingAssetsLoader
{
    private struct Entry
    {
        public Entry(long index, long size)
        {
            this.Index = index;
            this.Size = size;
        }
        public long Index;
        public long Size;
    }

    static Dictionary<string, Entry> _Entries;
    static FileStream _FS = null;
    static ZipFile _ZipFile = null;

    public static void Init()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
		InitForAndroid();
#endif
    }

    static void InitForAndroid()
    {
        _Entries = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);

        _FS = File.OpenRead(Application.dataPath);
        try
        {
            //System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            _ZipFile = new ZipFile(_FS);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            return;
        }

        var e = _ZipFile.GetEnumerator();
        while (e.MoveNext())
        {
            ZipEntry zipEntry = e.Current as ZipEntry;

            if (zipEntry.Name.StartsWith("assets/"))
            {
                _Entries.Add(zipEntry.Name, new Entry(zipEntry.ZipFileIndex, zipEntry.Size));
            }
        }
    }

    public static void Close()
    {
        //#if !UNITY_EDITOR && UNITY_ANDROID
        if (_ZipFile != null)
        {
            _ZipFile.Close();
            _ZipFile = null;
        }

        if (_FS != null)
        {
            _FS.Dispose();
            _FS = null;
        }
        _Entries = null;
        //#endif
    }

    public static byte[] LoadBytes(string filePath)
    {
#if !UNITY_EDITOR && UNITY_ANDROID
		return LoadBytesForAndroid(filePath);
#else
        return LoadBytesForOther(filePath);
#endif
    }

    public static string LoadText(string filePath)
    {
        byte[] bytes = LoadBytes(filePath);
        if (bytes == null) return null;
        return System.Text.Encoding.UTF8.GetString(bytes);
    }

    static byte[] LoadBytesForAndroid(string filePath)
    {
        string path = "assets/" + filePath;
        Entry entry;
        if (!_Entries.TryGetValue(path, out entry))
        {
            return null;
        }
        byte[] bytes = null;
        using (Stream s = _ZipFile.GetInputStream(entry.Index))
        {
            bytes = new byte[entry.Size];
            s.Read(bytes, 0, (int)entry.Size);
        }
        return bytes;
    }
    static byte[] LoadBytesForOther(string filePath)
    {
        string path = Path.Combine(Application.streamingAssetsPath, filePath);

        if (!File.Exists(path))
        {
            return null;
        }

        byte[] bytes = File.ReadAllBytes(path);
        return bytes;
    }
}
