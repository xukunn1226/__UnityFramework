using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NetProtocol;
using Google.Protobuf;

namespace Tests
{
    public class TestProtobufScript
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
            using (var output = System.IO.File.Create("assets/application/protocol/tests/editor/test_proto.bytes"))
            {
                toServer.WriteTo(output);
            }
            return data;
        }

        void Parse(byte[] data)
        {
            StoreRequest toClient = new StoreRequest();
            toClient.MergeFrom(data);
            CodedInputStream
            StoreRequest toClient1 = StoreRequest.Parser.ParseFrom(data);

            using (var input = System.IO.File.OpenRead("assets/application/protocol/tests/editor/test_proto.bytes"))
            {
                StoreRequest client2 = StoreRequest.Parser.ParseFrom(input);
            }
        }
    }
}
