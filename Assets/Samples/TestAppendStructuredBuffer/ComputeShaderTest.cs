using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeShaderTest : MonoBehaviour
{
    public ComputeShader computeShader;


    void Awake()
    {
        //reference: https://answers.unity.com/questions/1035132/how-can-i-read-in-the-actual-elements-from-an-appe.html

        var kernelID = computeShader.FindKernel("CSMain");
        var appendBuffer = new ComputeBuffer(64, sizeof(int), ComputeBufferType.Append);
        appendBuffer.SetCounterValue(0);
        //定义了缓冲区长度为64*结构大小的appendBuffer.

        var consumeBuffer = new ComputeBuffer(64, sizeof(int), ComputeBufferType.Append);
        consumeBuffer.SetCounterValue(0);
        consumeBuffer.SetData(new int[] { 97, 98, 99 });
        consumeBuffer.SetCounterValue(3);
        //consume类型结构相当于栈，所以取到的第一个值是99。

        computeShader.SetBuffer(kernelID, "appendBuffer", appendBuffer);
        computeShader.SetBuffer(kernelID, "consumeBuffer", consumeBuffer);
        computeShader.Dispatch(kernelID, 1, 1, 1);
        //单个线程组的大小为8，定义了1个线程组。也就是说会返回8个数据。

        var countBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        ComputeBuffer.CopyCount(appendBuffer, countBuffer, 0);
        //通过这个方法拿到第一个数据。

        int[] counter = new int[1] { 0 };
        countBuffer.GetData(counter);

        int count = counter[0];

        Debug.Log("count: " + count);

        var data = new int[count];
        appendBuffer.GetData(data);

        Debug.Log("data length: " + data.Length);

        for (int i = 0; i < data.Length; i++)
        {
            Debug.Log(data[i]);
        }

        consumeBuffer.Release();
        consumeBuffer.Dispose();

        appendBuffer.Release();
        appendBuffer.Dispose();

        countBuffer.Release();
        countBuffer.Dispose();
    }
}
