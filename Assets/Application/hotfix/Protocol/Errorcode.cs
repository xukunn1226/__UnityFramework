// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: errorcode.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
/// <summary>Holder for reflection information generated from errorcode.proto</summary>
public static partial class ErrorcodeReflection {

  #region Descriptor
  /// <summary>File descriptor for errorcode.proto</summary>
  public static pbr::FileDescriptor Descriptor {
    get { return descriptor; }
  }
  private static pbr::FileDescriptor descriptor;

  static ErrorcodeReflection() {
    byte[] descriptorData = global::System.Convert.FromBase64String(
        string.Concat(
          "Cg9lcnJvcmNvZGUucHJvdG8quAUKCUVycm9yQ29kZRIICgROb25lEAASGAoT",
          "U2VydmVySW50ZXJuYWxFcnJvchDoBxIQCgtDb25maWdFcnJvchDpBxIaChVT",
          "ZXJ2ZXJDaGVja1ZhbGlkRXJyb3IQ6gcSGQoUU2VydmVyQ29tbW9uRXJyb3JN",
          "YXgQ0A8SGQoTU2VydmVyTG9naW5FcnJvck1pbhChnAESGQoTU2VydmVyTG9n",
          "aW5FcnJvck1heBCEnQESGQoTU2VydmVyTG9iYnlFcnJvck1pbhCFnQESGwoV",
          "U2VydmVyTG9iYnlIYXNub3RSb2xlEIadARIVCg9Sb2xlQWxyZWFkRXhpc3QQ",
          "h50BEhkKE1NlcnZlckxvYmJ5RXJyb3JNYXgQ6J0BEhsKFVNlcnZlckR1bmdl",
          "b25FcnJvck1pbhDpnQESJwohU2VydmVyRHVuZ2Vvbk5vdEZvdW5kRHVuZ2Vp",
          "b25JbmZvEOqdARIfChlTZXJ2ZXJEdW5nZW9uTm90Rm91bmRUYXNrEOudARIf",
          "ChlTZXJ2ZXJEdW5nZW9uTm90Rm91bmROb2RlEOydARIiChxTZXJ2ZXJEdW5n",
          "ZW9uQ29uZGl0aW9uTm90TWV0EO2dARIhChtTZXJ2ZXJEdW5nZW9uSW52YWxp",
          "ZFNraWxsSUQQ7p0BEh0KF1NlcnZlckR1bmdlb25JbnZhbGlkUG9zEO+dARIk",
          "Ch5TZXJ2ZXJEdW5nZW9uU2tpbGxUYXJOdW1Tb011Y2gQ8J0BEiQKHlNlcnZl",
          "ckR1bmdlb25DaGFuZ2VCdWxsZXRFcnJvchDxnQESIgocU2VydmVyRHVuZ2Vv",
          "bk5vdEVub3VnaEJ1bGxldBDynQESIwodU2VydmVyRHVuZ2VvblNraWxsTm90",
          "Q29vbERvd24Q850BEhsKFVNlcnZlckR1bmdlb25FcnJvck1heBDMngFCF1oV",
          "Li4vLi4vc2VydmVyL3Byb3RvY29sYgZwcm90bzM="));
    descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
        new pbr::FileDescriptor[] { },
        new pbr::GeneratedClrTypeInfo(new[] {typeof(global::ErrorCode), }, null, null));
  }
  #endregion

}
#region Enums
public enum ErrorCode {
  /// <summary>
  ///0表示没有错误
  /// </summary>
  [pbr::OriginalName("None")] None = 0,
  /// <summary>
  ///通用错误码的区间为1000~2000
  /// </summary>
  [pbr::OriginalName("ServerInternalError")] ServerInternalError = 1000,
  /// <summary>
  ///配置表错误
  /// </summary>
  [pbr::OriginalName("ConfigError")] ConfigError = 1001,
  /// <summary>
  ///服务器验证失败
  /// </summary>
  [pbr::OriginalName("ServerCheckValidError")] ServerCheckValidError = 1002,
  [pbr::OriginalName("ServerCommonErrorMax")] ServerCommonErrorMax = 2000,
  /// <summary>
  ///Login相关错误码
  /// </summary>
  [pbr::OriginalName("ServerLoginErrorMin")] ServerLoginErrorMin = 20001,
  [pbr::OriginalName("ServerLoginErrorMax")] ServerLoginErrorMax = 20100,
  /// <summary>
  ///Lobby相关错误码
  /// </summary>
  [pbr::OriginalName("ServerLobbyErrorMin")] ServerLobbyErrorMin = 20101,
  /// <summary>
  ///没有角色信息
  /// </summary>
  [pbr::OriginalName("ServerLobbyHasnotRole")] ServerLobbyHasnotRole = 20102,
  /// <summary>
  ///角色已存在
  /// </summary>
  [pbr::OriginalName("RoleAlreadExist")] RoleAlreadExist = 20103,
  [pbr::OriginalName("ServerLobbyErrorMax")] ServerLobbyErrorMax = 20200,
  /// <summary>
  ///Dungeon相关错误码
  /// </summary>
  [pbr::OriginalName("ServerDungeonErrorMin")] ServerDungeonErrorMin = 20201,
  /// <summary>
  ///没有找到玩家的地下层数据
  /// </summary>
  [pbr::OriginalName("ServerDungeonNotFoundDungeionInfo")] ServerDungeonNotFoundDungeionInfo = 20202,
  /// <summary>
  ///地下城任务未找到
  /// </summary>
  [pbr::OriginalName("ServerDungeonNotFoundTask")] ServerDungeonNotFoundTask = 20203,
  /// <summary>
  ///地下城node未找到
  /// </summary>
  [pbr::OriginalName("ServerDungeonNotFoundNode")] ServerDungeonNotFoundNode = 20204,
  /// <summary>
  ///节点触发条件不满足
  /// </summary>
  [pbr::OriginalName("ServerDungeonConditionNotMet")] ServerDungeonConditionNotMet = 20205,
  /// <summary>
  ///无效的技能ID
  /// </summary>
  [pbr::OriginalName("ServerDungeonInvalidSkillID")] ServerDungeonInvalidSkillId = 20206,
  /// <summary>
  ///无效的位置
  /// </summary>
  [pbr::OriginalName("ServerDungeonInvalidPos")] ServerDungeonInvalidPos = 20207,
  /// <summary>
  ///功击目标太多
  /// </summary>
  [pbr::OriginalName("ServerDungeonSkillTarNumSoMuch")] ServerDungeonSkillTarNumSoMuch = 20208,
  /// <summary>
  ///换弹失败
  /// </summary>
  [pbr::OriginalName("ServerDungeonChangeBulletError")] ServerDungeonChangeBulletError = 20209,
  /// <summary>
  ///没有足够的子弹
  /// </summary>
  [pbr::OriginalName("ServerDungeonNotEnoughBullet")] ServerDungeonNotEnoughBullet = 20210,
  /// <summary>
  ///技能CD没有冷却
  /// </summary>
  [pbr::OriginalName("ServerDungeonSkillNotCoolDown")] ServerDungeonSkillNotCoolDown = 20211,
  [pbr::OriginalName("ServerDungeonErrorMax")] ServerDungeonErrorMax = 20300,
}

#endregion


#endregion Designer generated code
