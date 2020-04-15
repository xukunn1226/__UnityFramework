using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NetProtocol;
using Google.Protobuf;

namespace Tests
{
    public class NewTestScript
    {
        [Test]
        public void TestProto()
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
            using (var output = System.IO.File.Create("Assets/scripts/protocol/tests/editor/test_proto.bytes"))
            {
                toServer.WriteTo(output);
            }
            return data;
        }

        void Parse(byte[] data)
        {
            StoreRequest toClient = new StoreRequest();
            toClient = StoreRequest.Parser.ParseFrom(data);

            using (var input = System.IO.File.OpenRead("Assets/scripts/protocol/tests/editor/test_proto.bytes"))
            {
                StoreRequest client2 = StoreRequest.Parser.ParseFrom(input);
            }
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator NewTestScriptWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
