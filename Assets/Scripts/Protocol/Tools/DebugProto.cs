using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetProtocol;
using Google.Protobuf;

public class DebugProto : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        byte[] data = Serialize();
        Parse(data);
    }

    byte[] Serialize()
    {
        StoreRequest toServer = new StoreRequest();
        toServer.Name = "ABC";
        toServer.Num = 3;
        toServer.Result = 1;
        toServer.MyList.Add("中国");
        toServer.MyList.Add("j");
        toServer.Dic.Add("b", 2);
        toServer.Dic.Add("aa", 1);
        byte[] data = toServer.ToByteArray();
        using(var output = System.IO.File.Create("Assets/scripts/protocol/tools/test_proto.bytes"))
        {
            toServer.WriteTo(output);
        }
        return data;
    }

    void Parse(byte[] data)
    {
        StoreRequest toClient = new StoreRequest();
        toClient = StoreRequest.Parser.ParseFrom(data);

        using (var input = System.IO.File.OpenRead("Assets/scripts/protocol/tools/test_proto.bytes"))
        {
            StoreRequest client2 = StoreRequest.Parser.ParseFrom(input);
        }
    }
}
