// using Google.Protobuf;
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Application.Runtime;

// namespace Application.Logic
// {
//     public class NetModuleBattleVerify : NetBaseModule
//     {
//         public NetModuleBattleVerify(NetModuleManager manager) : base(manager)
//         {
//         }

//         public override int GetModuleID()
//         {
//             return (int)ModuleType.ModuleBattleVerify;
//         }

//         public override void InitMsgFunc()
//         {
//             //dicDelegates.Add((int)BattleVerifyMsgID.NodeObjectInfos, OnNodeObjectInfosAck);
//             //dicDelegates.Add((int)BattleVerifyMsgID.TriggerNodeObject, OnTriggerObjectAck);
//             //dicDelegates.Add((int)BattleVerifyMsgID.NotifyNodeObjectChanged, OnNodeObjectsChanged);

//             AddReceiver(NetMsgIds.BattleVerifyModule.NodeObjectInfos, OnNodeObjectInfosAck);
//             //AddReceiver(NetMsgIds.BattleVerifyModule.TriggerNodeObject, OnTriggerObjectAck);
//             //AddReceiver(NetMsgIds.BattleVerifyModule.NotifyNodeObjectChanged, OnNodeObjectsChanged);
//             AddReceiver(NetMsgIds.BattleVerifyModule.InteractionCompleted, OnInteractionCompleted);
//         }

//         private void OnInteractionCompleted(IMessage msg, NetMsgData data)
//         {
//             var ack = (InteractionCompletedAck)msg;
//             if (ack.ErrorCode != ErrorCode.None)
//             {
//                 Debug.LogError(ack.ErrorCode);
//                 //显示提示信息
//                 return;
//             }
//             Debug.Log("OnInteractionCompleted");
//         }

//         //已完成的或是触发过的节点的信息（没有触发过的不发送）
//         private void OnNodeObjectInfosAck(IMessage msg, NetMsgData data)
//         {
//             var ack = (NodeObjectInfosAck)msg;
//             if (ack.ErrorCode != ErrorCode.None)
//             {
//                 Debug.LogError(ack.ErrorCode);
//                 //显示提示信息
//                 return;
//             }

//             //TriggerManager.Instance.Init();
//             // GameData.Instance.dungeonData.Updata(ack);
//         }

//         //触发某个节点的返回
//         private void OnTriggerObjectAck(IMessage msg, NetMsgData data)
//         {
//             //TriggerNodeObjectAck ack = new TriggerNodeObjectAck();
//             //ack.MergeFrom(data.MsgData, 0, data.MsgLen);

//             var ack = (TriggerNodeObjectAck)msg;

//             if (ack.ErrorCode != ErrorCode.None)
//             {
//                 //显示提示信息
//                 Debug.LogError(ack.ErrorCode);
//                 return;
//             }
//             //EventManager.Allocate<EventArgs_Trigger>().Set(SceneTriggerEvent.OnTriggerObjectAck, ack).Dispatch();
//         }

//         //哪些节点状态发生变化了（如，打怪计数完成后，触发了哪个节点机关的开启 等）
//         private void OnNodeObjectsChanged(IMessage msg,NetMsgData data)
//         {
//             //NotifyNodeObjectChangedAck ack = new NotifyNodeObjectChangedAck();
//             //ack.MergeFrom(data.MsgData, 0, data.MsgLen);
//             var ack = (NotifyNodeObjectChangedAck)msg;

//             // EventManager.Allocate<EventArgs_ObjChange>().Set(SceneTriggerEvent.OnNodeObjectsChanged, ack).Dispatch();
//         }
//     }

// }
