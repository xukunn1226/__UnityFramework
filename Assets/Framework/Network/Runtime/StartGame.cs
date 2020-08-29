using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework;
using System.IO;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class StartGame : MonoBehaviour
{
    private bool m_Connected;
    private SemaphoreSlim m_SendBufferSema = new SemaphoreSlim(0, 1);              // 控制消息发送的信号量

    // Start is called before the first frame update
    void Start()
    {
        //SocketServer server = new SocketServer();
        //server.Start(8090);

        //Debug.Log("StartGame");

        //Foo();
        //Foo2();
        
        m_Connected = true;
        RunAsync();

        Debug.Log($"Current thread ID: {Thread.CurrentThread.ManagedThreadId}   Frame: {Time.time}");
    }

    private void OnDestroy()
    {
        m_Connected = false;
    }

    async void RunAsync()
    {
        while (m_Connected)
        {
            //Debug.LogWarning($"Begin Async.....");
            //await Task.Delay(2000);
            //Debug.LogWarning($"Async Current thread ID: {Thread.CurrentThread.ManagedThreadId}");

            await m_SendBufferSema.WaitAsync();
            Debug.Log($"------------: {Time.frameCount}    {Time.time}");
            //await Task.Delay(1000);
            Debug.Log($"=========: {Time.frameCount}    {Time.time}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log($"Update:  {Time.frameCount}");
        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_Connected = false;
            //SocketClient client = new SocketClient();
            //client.Connect("127.0.0.1", 8090);
        }

        if(Input.GetKeyDown(KeyCode.F1))
        {
            // int i = 0;

            if (m_SendBufferSema.CurrentCount == 0)          // The number of remaining threads that can enter the semaphore
            {
                int count = m_SendBufferSema.Release();                 // Sema.CurrentCount += 1
                Debug.Log($"Post Release Sema: {Time.frameCount}    {Time.time}     {count}");
            }
            if (m_SendBufferSema.CurrentCount == 0)          // The number of remaining threads that can enter the semaphore
            {
                int count = m_SendBufferSema.Release();                 // Sema.CurrentCount += 1
                Debug.Log($"Post Release Sema: {Time.frameCount}    {Time.time}     {count}");
            }
            if (m_SendBufferSema.CurrentCount == 0)          // The number of remaining threads that can enter the semaphore
            {
                int count = m_SendBufferSema.Release();                 // Sema.CurrentCount += 1
                Debug.Log($"Post Release Sema: {Time.frameCount}    {Time.time}     {count}");
            }
        }
    }

    void Foo2()
    {
        MemoryStream ms = new MemoryStream();

        byte[] firstString = Encoding.UTF8.GetBytes("abcd234wrrwrwdddd");
        ms.Write(firstString, 0, firstString.Length);
        Debug.Log($"length: {ms.Length}     position: {ms.Position}");
        //ms.SetLength(10);
        byte[] buffer = ms.GetBuffer();
        byte[] buffer2 = ms.ToArray();
        string firstStr = Encoding.UTF8.GetString(buffer2);

        ms.Seek(0, SeekOrigin.Begin);

        byte[] secondString = Encoding.UTF8.GetBytes("123456");
        ms.Write(secondString, 0, secondString.Length);
        Debug.Log($"---length: {ms.Length}     position: {ms.Position}");
        byte[] buf = ms.GetBuffer();
        byte[] buf2 = ms.ToArray();
        string secondStr = Encoding.UTF8.GetString(buf2);
    }

    void Foo()
    {
        int count;
        byte[] byteArray;
        char[] charArray;
        UnicodeEncoding uniEncoding = new UnicodeEncoding();

        // Create the data to write to the stream.
        byte[] firstString = uniEncoding.GetBytes(
            "Invalid file path characters are: ");
        byte[] secondString = uniEncoding.GetBytes(
            Path.GetInvalidPathChars());

        using (MemoryStream memStream = new MemoryStream(100))
        {
            // Write the first string to the stream.
            memStream.Write(firstString, 0, firstString.Length);

            // Write the second string to the stream, byte by byte.
            count = 0;
            while (count < secondString.Length)
            {
                memStream.WriteByte(secondString[count++]);
            }

            // Write the stream properties to the console.
            Console.WriteLine(
                "Capacity = {0}, Length = {1}, Position = {2}\n",
                memStream.Capacity.ToString(),
                memStream.Length.ToString(),
                memStream.Position.ToString());

            // Set the position to the beginning of the stream.
            memStream.Seek(0, SeekOrigin.Begin);

            // Read the first 20 bytes from the stream.
            byteArray = new byte[memStream.Length];
            count = memStream.Read(byteArray, 0, 20);

            // Read the remaining bytes, byte by byte.
            while (count < memStream.Length)
            {
                byteArray[count++] =
                    Convert.ToByte(memStream.ReadByte());
            }

            // Decode the byte array into a char array
            // and write it to the console.
            charArray = new char[uniEncoding.GetCharCount(
                byteArray, 0, count)];
            uniEncoding.GetDecoder().GetChars(
                byteArray, 0, count, charArray, 0);
            Console.WriteLine(charArray);
        }
    }
}
