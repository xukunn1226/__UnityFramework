using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using System.Threading.Tasks;
using NetProtocol;
using Framework.NetWork;

public class NetCode : MonoBehaviour
{
    private NetManager<IMessage> m_NetManager;
    private bool m_Quit;

    async void Awake()
    {
        m_NetManager = new NetManager<IMessage>(new PacketProtobuf());
        await m_NetManager.Connect("192.168.5.3", 11000);
        // await m_NetManager.Connect("192.168.1.6", 11000);
        await AutoSending();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        m_NetManager?.Tick();
    }

    void OnDisable()
    {
        m_Quit = true;
        m_NetManager.Close(true);
    }

    async Task AutoSending()
    {
        int index = 0;
        while (!m_Quit && m_NetManager.state == ConnectState.Connected)
        {
            string data = "Hello world..." + index++;
            // Debug.Log("\n Sending...:" + data);
            StoreRequest msg = new StoreRequest();
            msg.Name = "1233";
            msg.Num = 3;
            msg.Result = 4;
            if (index % 2 == 0)
                msg.MyList.Add("22222222222");
            if (index % 3 == 0)
                msg.MyList.Add("33333333333333");

            if(!m_NetManager.SendData(msg))
                break;
            
            await Task.Delay(10);
        }
    }
}
