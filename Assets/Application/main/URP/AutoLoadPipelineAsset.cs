using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Application.Runtime
{
    /// <summary>
    /// 自动载入管线脚本，可用此方法分离管线与引擎，以便于做管线的热更新
    /// </summary>
    [ExecuteAlways]
    public class AutoLoadPipelineAsset : MonoBehaviour
    {
        [SerializeField]
        private UniversalRenderPipelineAsset m_PipelineAsset;
        private RenderPipelineAsset m_PreviousPipelineAsset;
        private bool m_overrideQualitySettings;

        void OnEnable()
        {
            //Debug.Log("AutoLoadPipelineAsset::OnEnable");
            UpdatePipeline();
        }

        void OnDisable()
        {
            //Debug.Log("AutoLoadPipelineAsset::OnDisable");
            ResetPipeline();
        }

        private void UpdatePipeline()
        {
            if (m_PipelineAsset)
            {
                if (QualitySettings.renderPipeline != null && QualitySettings.renderPipeline != m_PipelineAsset)
                {
                    m_PreviousPipelineAsset = QualitySettings.renderPipeline;
                    QualitySettings.renderPipeline = m_PipelineAsset;
                    m_overrideQualitySettings = true;
                }
                else if (GraphicsSettings.renderPipelineAsset != m_PipelineAsset)
                {
                    m_PreviousPipelineAsset = GraphicsSettings.renderPipelineAsset;
                    GraphicsSettings.renderPipelineAsset = m_PipelineAsset;
                    m_overrideQualitySettings = false;
                }
            }
            //Debug.Log($"UpdatePipeline: m_PipelineAsset [{m_PipelineAsset}]\n   m_PreviousPipelineAsset [{m_PreviousPipelineAsset}]\n   m_overrodeQualitySettings [{m_overrideQualitySettings}]");
        }

        private void ResetPipeline()
        {
            if (m_PreviousPipelineAsset)
            {
                if (m_overrideQualitySettings)
                {
                    QualitySettings.renderPipeline = m_PreviousPipelineAsset;
                    //Debug.Log($"ResetPipeline: {QualitySettings.renderPipeline}");
                }
                else
                {
                    GraphicsSettings.renderPipelineAsset = m_PreviousPipelineAsset;
                    //Debug.Log($"ResetPipeline: {GraphicsSettings.renderPipelineAsset}");
                }
            }
        }
    }
}