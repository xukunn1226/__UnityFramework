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
  ///0��ʾû�д���
  /// </summary>
  [pbr::OriginalName("None")] None = 0,
  /// <summary>
  ///ͨ�ô����������Ϊ1000~2000
  /// </summary>
  [pbr::OriginalName("ServerInternalError")] ServerInternalError = 1000,
  /// <summary>
  ///���ñ�����
  /// </summary>
  [pbr::OriginalName("ConfigError")] ConfigError = 1001,
  /// <summary>
  ///��������֤ʧ��
  /// </summary>
  [pbr::OriginalName("ServerCheckValidError")] ServerCheckValidError = 1002,
  [pbr::OriginalName("ServerCommonErrorMax")] ServerCommonErrorMax = 2000,
  /// <summary>
  ///Login��ش�����
  /// </summary>
  [pbr::OriginalName("ServerLoginErrorMin")] ServerLoginErrorMin = 20001,
  [pbr::OriginalName("ServerLoginErrorMax")] ServerLoginErrorMax = 20100,
  /// <summary>
  ///Lobby��ش�����
  /// </summary>
  [pbr::OriginalName("ServerLobbyErrorMin")] ServerLobbyErrorMin = 20101,
  /// <summary>
  ///û�н�ɫ��Ϣ
  /// </summary>
  [pbr::OriginalName("ServerLobbyHasnotRole")] ServerLobbyHasnotRole = 20102,
  /// <summary>
  ///��ɫ�Ѵ���
  /// </summary>
  [pbr::OriginalName("RoleAlreadExist")] RoleAlreadExist = 20103,
  [pbr::OriginalName("ServerLobbyErrorMax")] ServerLobbyErrorMax = 20200,
  /// <summary>
  ///Dungeon��ش�����
  /// </summary>
  [pbr::OriginalName("ServerDungeonErrorMin")] ServerDungeonErrorMin = 20201,
  /// <summary>
  ///û���ҵ���ҵĵ��²�����
  /// </summary>
  [pbr::OriginalName("ServerDungeonNotFoundDungeionInfo")] ServerDungeonNotFoundDungeionInfo = 20202,
  /// <summary>
  ///���³�����δ�ҵ�
  /// </summary>
  [pbr::OriginalName("ServerDungeonNotFoundTask")] ServerDungeonNotFoundTask = 20203,
  /// <summary>
  ///���³�nodeδ�ҵ�
  /// </summary>
  [pbr::OriginalName("ServerDungeonNotFoundNode")] ServerDungeonNotFoundNode = 20204,
  /// <summary>
  ///�ڵ㴥������������
  /// </summary>
  [pbr::OriginalName("ServerDungeonConditionNotMet")] ServerDungeonConditionNotMet = 20205,
  /// <summary>
  ///��Ч�ļ���ID
  /// </summary>
  [pbr::OriginalName("ServerDungeonInvalidSkillID")] ServerDungeonInvalidSkillId = 20206,
  /// <summary>
  ///��Ч��λ��
  /// </summary>
  [pbr::OriginalName("ServerDungeonInvalidPos")] ServerDungeonInvalidPos = 20207,
  /// <summary>
  ///����Ŀ��̫��
  /// </summary>
  [pbr::OriginalName("ServerDungeonSkillTarNumSoMuch")] ServerDungeonSkillTarNumSoMuch = 20208,
  /// <summary>
  ///����ʧ��
  /// </summary>
  [pbr::OriginalName("ServerDungeonChangeBulletError")] ServerDungeonChangeBulletError = 20209,
  /// <summary>
  ///û���㹻���ӵ�
  /// </summary>
  [pbr::OriginalName("ServerDungeonNotEnoughBullet")] ServerDungeonNotEnoughBullet = 20210,
  /// <summary>
  ///����CDû����ȴ
  /// </summary>
  [pbr::OriginalName("ServerDungeonSkillNotCoolDown")] ServerDungeonSkillNotCoolDown = 20211,
  [pbr::OriginalName("ServerDungeonErrorMax")] ServerDungeonErrorMax = 20300,
}

#endregion


#endregion Designer generated code