using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ScanProtoMsg
{
    public class ScanJob
    {
        private static Action<string> Log => Console.WriteLine;

        public class ProtoMsgInfo
        {
            public int msgId = 0;// global id

            public int moduleId = 0;

            public int msgInModuleId = 0;

            public string moduleName;

            public string msgName;

            public bool hasReq;

            public string reqName;

            public bool hasAck;

            public string ackName;
        }

        public class ProtoInfo
        {
            public string moduleName;

            public FileInfo fileInfo;

            public string fileContent;

            public string[] lines;

            public Dictionary<string, ProtoMsgInfo> msgids = new Dictionary<string, ProtoMsgInfo>();

            public bool isModule = false;
        }

        public class ModuleInfo
        {
            public string moduleName;

            public int moduleId;

            public List<ProtoMsgInfo> msgids = new List<ProtoMsgInfo>();
        }

        private ScanOption m_option;

        private List<ProtoInfo> m_protoInfos = new List<ProtoInfo>();

        public Dictionary<string, ModuleInfo> m_modules = new Dictionary<string, ModuleInfo>();



        public ScanJob(ScanOption option)
        {
            this.m_option = option;
        }


        public void Do()
        {
            GatherAllProtoFiles();

            ParseProtoFiles();

            HandleMsgs();

            Output();

            Log($"done");

        }

        private void GatherAllProtoFiles()
        {
            List<FileInfo> fileInfos = new List<FileInfo>();
            Utils.ListDirectory(m_option.src, fileInfos,"*.proto");

            foreach (var finfo in fileInfos)
            {
                var protoInfo = new ProtoInfo();

                using (var s = finfo.OpenText())
                {
                    protoInfo.fileContent = s.ReadToEnd();
                    protoInfo.lines = Regex.Split(protoInfo.fileContent,"\r\n|\r|\n");
                }
                protoInfo.fileInfo = finfo;
                m_protoInfos.Add(protoInfo);
            }
        }

        private void ParseProtoFiles()
        {
            foreach (var protoInfo in m_protoInfos)
            {
                ParseSingleProtoFiles(protoInfo);
            }

        }
        private void ParseSingleProtoFiles(ProtoInfo protoInfo)
        {
            Log($"Parsing {protoInfo.fileInfo.Name}");
            if(protoInfo.fileInfo.Name == "common.proto")
            {
                ScanCommonProto(protoInfo);
            }
            ScanModuleName(protoInfo);
            ScanMsgIds(protoInfo);
            ScanMsgReqAndAck(protoInfo);

        }

        private static string kScanModuleDefinePattern = @"enum\s+ModuleType";
        private static string kScanModuleIdsPattern = @"Module(\w+)\s+=\s+(\d+)";

        private void ScanCommonProto(ProtoInfo protoInfo)
        {
            bool isBeginModuleIds = false;
            foreach (var line in protoInfo.lines)
            {
                if (Regex.IsMatch(line, kScanModuleDefinePattern))
                {
                    isBeginModuleIds = true;
                }
                if (isBeginModuleIds)
                {
                    var match = Regex.Match(line, kScanModuleIdsPattern);
                    if (match.Success)
                    {
                        var results = match.Groups;
                        if (results.Count != 3)
                        {
                            Log($"Invalid module id in line \"{line}\"\n--file:{protoInfo.fileInfo.Name}");
                        }
                        int moduleId = int.Parse(results[2].Value);
                        string moduleName = results[1].Value;
                        var module = new ModuleInfo();
                        module.moduleId = moduleId;
                        module.moduleName = moduleName;
                        m_modules[moduleName] = module;
                    }

                    if (line.Contains("}"))
                    {
                        isBeginModuleIds = false;
                    }
                }
            }
        }

        private static string kScanModuleNamePattern = @"enum\s+(\w+)MsgID";
        private void ScanModuleName(ProtoInfo protoInfo)
        {
            var match = Regex.Match(protoInfo.fileContent, kScanModuleNamePattern);
            if (match.Success)
            {
                protoInfo.isModule = true;
                var results = match.Groups;
                protoInfo.moduleName = results[1].Value;
            }
            else
            {
                Log($"[{protoInfo.fileInfo.Name}] is not a net module");
            }
        }

        private static string kScanMsgIdsContentPatternBegin = @"enum\s+\w+MsgID";
        private static string kScanMsgIdsPattern = @"(\w+)\s+=\s+(\d+)";

        private void ScanMsgIds(ProtoInfo protoInfo)
        {
            bool isBeginMsgIds = false;
            foreach(var line in protoInfo.lines)
            {
                if(Regex.IsMatch(line, kScanMsgIdsContentPatternBegin))
                {
                    isBeginMsgIds = true;
                }
                if (isBeginMsgIds)
                {
                    var match = Regex.Match(line, kScanMsgIdsPattern);
                    if (match.Success)
                    {
                        var results = match.Groups;
                        if(results.Count != 3)
                        {
                            Log($"Invalid msg id in line \"{line}\"\n--file:{protoInfo.fileInfo.Name}");
                        }
                        int msgId = int.Parse(results[2].Value);
                        string msgName = results[1].Value;
                        var msg = new ProtoMsgInfo();
                        msg.msgInModuleId = msgId;
                        msg.msgName = msgName;
                        protoInfo.msgids[msgName] = msg;
                    }

                    if (line.Contains("}"))
                    {
                        isBeginMsgIds = false;
                    }
                }
            }
        }

        private static string kScanMsgReqAckPattern = @"message (\w+)(Req|Ack)";
        private void ScanMsgReqAndAck(ProtoInfo protoInfo)
        {
            foreach (var line in protoInfo.lines)
            {
                var match = Regex.Match(line, kScanMsgReqAckPattern);
                if (match.Success)
                {
                    var results = match.Groups;
                    if (results.Count != 3)
                    {
                        Log($"Invalid msg req|ack in line \"{line}\"\n--file:{protoInfo.fileInfo.Name}");
                    }
                    string msgName = results[1].Value;
                    string reqOrAck = results[2].Value;
                    if(!protoInfo.msgids.TryGetValue(msgName,out ProtoMsgInfo msg))
                    {
                        Log($"Can not found msg id define with msg name [{msgName}]");
                    }
                    else
                    {
                        if(reqOrAck == "Req")
                        {
                            msg.hasReq = true;
                            msg.reqName = $"{msgName}Req";
                        }else if (reqOrAck == "Ack")
                        {
                            msg.hasAck = true;
                            msg.ackName = $"{msgName}Ack";
                        }
                        else
                        {
                            Log($"Invalid msg req|ack in line \"{line}\"\n--file:{protoInfo.fileInfo.Name}");
                        }
                    }
                }
            }
        }

        private void HandleMsgs()
        {
            foreach (var protoInfo in m_protoInfos)
            {
                if (protoInfo.isModule)
                {
                    HandleSingleProto(protoInfo);
                }
            }

            foreach(var moduleInfo in m_modules.Values)
            {
                foreach (var protoInfo in m_protoInfos)
                {
                    if (protoInfo.isModule)
                    {
                        foreach(var msgInfo in protoInfo.msgids.Values)
                        {
                            if(msgInfo.moduleId == moduleInfo.moduleId)
                            {
                                msgInfo.moduleName = moduleInfo.moduleName;
                                moduleInfo.msgids.Add(msgInfo);
                            }
                        }
                    }
                }
            }
        }

        private void HandleSingleProto(ProtoInfo protoInfo)
        {
            if (m_modules.TryGetValue(protoInfo.moduleName,out ModuleInfo module))
            {
                foreach (var msg in protoInfo.msgids.Values)
                {
                    msg.moduleId = module.moduleId;
                    msg.msgId = module.moduleId << 16 | msg.msgInModuleId;
                }
            }
            else
            {
                Log($"Can not found module info for msg [{protoInfo.moduleName}]");

            }

        }

        const string kOutputFileName = "NetMsgDefines";
        private void Output()
        {
            BuildContent();

        }

        private StringBuilder BuildContent()
        {
            StringBuilder builder = new StringBuilder();

            OutputNetMsgDefine(builder);
            WriteFile(builder, "NetMsgDefines");
            builder.Clear();

            OutputNetMsgIds(builder);
            WriteFile(builder, "NetMsgIds");
            builder.Clear();

            OutputNetReqs(builder);
            WriteFile(builder, "NetReqs");
            builder.Clear();

            OutputNetAcks(builder);
            WriteFile(builder, "NetAcks");
            builder.Clear();


            return builder;
        }

        private void WriteFile(StringBuilder builder,string fileName)
        {
            var path = $"{m_option.dst}/{fileName}.cs";

            using (var f = File.CreateText(path))
            {
                f.Write(builder);

            }
        }

        private void OutputFrameHeader(StringBuilder builder)
        {
            builder.Append(@"
using System;
using System.Collections.Generic;
namespace Application.Runtime
{");
            builder.Append("\n\n");
        }

        private void OutputFrameFooter(StringBuilder builder)
        {
            builder.Append("}// end of namespace\n");

        }

        // 
        private void OutputNetMsgDefine(StringBuilder builder)
        {
            OutputFrameHeader(builder);

            builder.Append("\n\n");

            builder.Append("#region NetMsgDefines\n");

            builder.Append(@"
    public class NetMsgDefines
    {
        public class NetMsgDefineInfo
        {
            public int msgId;

            public int moduleId;

            public int msgInModuleId;

            public string moduleName;

            public string msgName;

            public NetMsgDefineInfo(int moduleId, int msgInModuleId, string moduleName, string msgName)
            {
                this.moduleId = moduleId;
                this.msgInModuleId = msgInModuleId;
                this.msgId = moduleId << 16 | msgInModuleId;
                this.moduleName = moduleName;
                this.msgName = msgName;

            }
        }// end of NetMsgDefineInfo");

            builder.Append("\n");
            builder.Append($"       public static List<NetMsgDefineInfo> msgs = new List<NetMsgDefineInfo>()\n");
            builder.Append("        {\n");

            foreach (var module in m_modules.Values)
            {
                builder.Append($"        // const msg id for module {module.moduleName}\n");

                foreach (var msg in module.msgids)
                {
                    builder.Append($"		new NetMsgDefineInfo({msg.moduleId},{msg.msgInModuleId},\"{msg.moduleName}\",\"{msg.msgName}\"),\n");
                }
                builder.Append("\n");
            }
            builder.Append("\n      };\n");
            builder.Append("  }// end of NetMsgsDefines\n");
            builder.Append("#endregion\n\n");

            OutputFrameFooter(builder);

        }

        private void OutputNetMsgIds( StringBuilder builder)
        {
            OutputFrameHeader(builder);


            builder.Append("#region NetMsgIds\n");

            builder.Append("   public static class NetMsgIds\n");
            builder.Append("   {\n");


            foreach (var module in m_modules.Values)
            {
                builder.Append($"       public static class {module.moduleName}Module\n");
                builder.Append($"       {{\n");

                foreach (var msg in module.msgids)
                {
                    builder.Append($"           public static int {msg.msgName} = {msg.msgId};//module {msg.moduleId},msg {msg.msgInModuleId} \n");
                }
                builder.Append($"       }}\n");
            }
            builder.Append("  }// end of NetMsgIds\n");

            builder.Append("#endregion\n\n");

            OutputFrameFooter(builder);

        }

        private void OutputNetReqs(StringBuilder builder)
        {
            OutputFrameHeader(builder);


            builder.Append("#region NetReqs\n");
            builder.Append("   public static class NetReqs\n");
            builder.Append("   {\n");

            // common send
            builder.Append($"        public static void Send(int msgId)\n        {{\n");
            builder.Append($"          NetModuleManager.Instance.SendData(msgId);\n");
            builder.Append($"        }}\n");

            foreach (var module in m_modules.Values)
            {
                builder.Append($"        // sendings for module {module.moduleName}\n");
                foreach (var msg in module.msgids)
                {
                    if (msg.hasReq)
                    {
                        builder.Append($"        public static void Send({msg.msgName}Req req)\n        {{\n");
                        builder.Append($"          const int msgId = {msg.msgId};\n");
                        builder.Append($"          NetModuleManager.Instance.SendData(msgId, req);\n");
                        builder.Append($"        }}\n");
                    }

                }
            }
            builder.Append("  }// end of NetReqs\n");

            builder.Append("#endregion\n\n");

            OutputFrameFooter(builder);

        }

        private void OutputNetAcks(StringBuilder builder)
        {
            builder.Append("using Google.Protobuf;");

            OutputFrameHeader(builder);

            builder.Append("#region NetAcks\n");
            builder.Append("   public static class NetAcks\n");
            builder.Append("   {\n");

            builder.Append("        public delegate IMessage Creator();\n");


            builder.Append(@"
        public static IMessage Convert(NetMsgData data)
        {
            int msgId = data.MsgID;
            if(m_converters.TryGetValue(msgId,out Creator creator))
            {
                var ack = creator();
                ack.MergeFrom(data.MsgData, 0, data.MsgLen);
                return ack;
            }
            return null;
        }

");

            builder.Append("        private static Dictionary<int, Creator> m_converters = new Dictionary<int, Creator>()\n");
            builder.Append("        {\n");
            foreach (var module in m_modules.Values)
            {
                builder.Append($"        // acks for module {module.moduleName}\n");
                foreach (var msg in module.msgids)
                {
                    if (msg.hasAck)
                    {
                        builder.Append($"        {{ {msg.msgId},Create{msg.msgName}Ack}},\n");
                    }
                }
            }
            builder.Append("        };\n");


            foreach (var module in m_modules.Values)
            {
                builder.Append($"        // acks for module {module.moduleName}\n");
                foreach (var msg in module.msgids)
                {
                    if (msg.hasAck)
                    {
                        builder.Append($"        public static IMessage Create{msg.msgName}Ack()\n        {{\n");
                        builder.Append($"          var ack = new {msg.msgName}Ack();\n");
                        builder.Append($"          return ack;\n");
                        builder.Append($"        }}\n");
                    }

                }
            }
            builder.Append("  }// end of NetAcks\n");

            builder.Append("#endregion\n\n");

            OutputFrameFooter(builder);

        }


    }
}
