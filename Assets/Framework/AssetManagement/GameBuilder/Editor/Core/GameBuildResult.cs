
namespace Framework.AssetManagement.AssetEditorWindow
{
    /// <summary>
    /// 构建结果
    /// </summary>
    public class GameBuildResult
    {
        /// <summary>
        /// 构建是否成功
        /// </summary>
        public bool Success;

        /// <summary>
        /// 构建失败的任务
        /// </summary>
        public string FailedTask;

        /// <summary>
        /// 构建失败的信息
        /// </summary>
        public string FailedInfo;

        /// <summary>
        /// 输出的补丁包目录
        /// </summary>
        public string OutputPackageDirectory;
    }
}