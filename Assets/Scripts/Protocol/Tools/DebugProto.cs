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
        StoreRequest toServer = new StoreRequest();
        toServer.Name = "ABC";
        toServer.Num = 3;
        toServer.Result = 1;
        toServer.MyList.Add("x");
        toServer.MyList.Add("j");
        byte[] data = toServer.ToByteArray();

        StoreRequest toClient = new StoreRequest();
        toClient = (StoreRequest)StoreRequest.Descriptor.Parser.ParseFrom(data);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
