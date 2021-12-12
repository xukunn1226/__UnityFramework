using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf;
using System.Threading.Tasks;

namespace Application.Runtime.Tests
{
    public class TestNetListener : MonoBehaviour, INetManagerListener<NetMsgData>
    {
        // Start is called before the first frame update
        async void Start()
        {
            NetManager.Instance.SetListener(this);
            await NetManager.Instance.Connect();
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                // NetManager.Instance.Connect();
            }
        }

        public void OnPeerConnectSuccess()
        {
            Debug.Log($"TestNetListener: connect success!");
        }

        public void OnPeerConnectFailed(System.Exception e)
        {
            Debug.Log($"TestNetListener: connect failed {e.Message}");
        }

        public void OnPeerDisconnected(System.Exception e)
        {
            Debug.Log($"TestNetListener: connect disconnected {e.Message}");
        }

        public void OnPeerClose()
        {
            Debug.Log($"TestNetListener: connect close!");
        }

        public void OnNetworkReceive(in List<NetMsgData> msgs)
        {}

    }
}