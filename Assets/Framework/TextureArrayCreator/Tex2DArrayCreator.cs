using UnityEngine;
using UnityEngine.Rendering;

namespace Framework.Core
{
    public class Tex2DArrayCreator : MonoBehaviour
    {
        public MeshRenderer render;
        public Texture2D[] textures;
        public ECopyTexMethpd copyTexMethod;   // 把Texrure2D信息拷贝到Texture2DArray对象中使用的方式 //
        [Header("线性贴图（不包含颜色数据）")]
        public bool linear;

        public enum ECopyTexMethpd
        {
            CopyTexture = 0,     // 使用 Graphics.CopyTexture 方法 //
            SetPexels = 1,       // 使用 Texture2DArray.SetPixels 方法 //
        }

        private Material m_mat;

        void Start()
        {
            if (textures == null || textures.Length == 0)
            {
                enabled = false;
                return;
            }

            if (SystemInfo.copyTextureSupport == CopyTextureSupport.None ||
                !SystemInfo.supports2DArrayTextures)
            {
                enabled = false;
                return;
            }

            Texture2DArray texArr = null;

            // 结论 //
            // Graphics.CopyTexture耗时(单位:Tick): 5914, 8092, 6807, 5706, 5993, 5865, 6104, 5780 //
            // Texture2DArray.SetPixels耗时(单位:Tick): 253608, 255041, 225135, 256947, 260036, 295523, 250641, 266044 //
            // Graphics.CopyTexture 明显快于 Texture2DArray.SetPixels 方法 //
            // Texture2DArray.SetPixels 方法的耗时大约是 Graphics.CopyTexture 的50倍左右 //
            // Texture2DArray.SetPixels 耗时的原因是需要把像素数据从cpu传到gpu, 原文: Call Apply to actually upload the changed pixels to the graphics card //
            // 而Graphics.CopyTexture只在gpu端进行操作, 原文: operates on GPU-side data exclusively //
            // 考虑使用Graphics.CopyTexture来复制Texture还有一个好处是可不勾选源纹理为可读写的也行。

            if (copyTexMethod == ECopyTexMethpd.CopyTexture)
            {
                for (int i = 0; i < textures.Length; i++)
                {
                    texArr = CreateTexture2DArray(linear);
                }
            }
            else if (copyTexMethod == ECopyTexMethpd.SetPexels)
            {
                texArr = new Texture2DArray(textures[0].width, textures[0].width, textures.Length, textures[0].format, false, false);
                for (int i = 0; i < textures.Length; i++)
                {
                    texArr.SetPixels(textures[i].GetPixels(), i, 0);
                }

                texArr.Apply();
                texArr.wrapMode = TextureWrapMode.Clamp;
                texArr.filterMode = FilterMode.Bilinear;
            }

            m_mat = render.material;

            m_mat.SetTexture("_TexArr", texArr);
            m_mat.SetFloat("_Index", Random.Range(0, textures.Length));
        }

        void OnGUI()
        {
            if (GUI.Button(new Rect(0, 0, 200, 100), "Change Texture"))
            {
                m_mat.SetFloat("_Index", Random.Range(0, textures.Length));
            }
        }

        public Texture2DArray CreateTexture2DArray(bool linear = false)
        {
            Texture2DArray mTex2DArray = new Texture2DArray(textures[0].width, textures[0].height, textures.Length, textures[0].format, true, false);
            for (int index = 0; index < textures.Length; index++)
            {
                for (int m = 0; m < textures[index].mipmapCount; m++)
                {
                    Graphics.CopyTexture(textures[index], 0, m, mTex2DArray, index, m);
                }
            }
            mTex2DArray.wrapMode = TextureWrapMode.Clamp;
            mTex2DArray.filterMode = FilterMode.Bilinear;
            return mTex2DArray;
        }
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(Tex2DArrayCreator))]
    public class Tex2DArrayTestEditor : UnityEditor.Editor
    {
        private Tex2DArrayCreator m_Test;

        private void Awake()
        {
            m_Test = (Tex2DArrayCreator)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Create Texture2DArray"))
            {
                var mTex2DArray = m_Test.CreateTexture2DArray(m_Test.linear);
                UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();                
                UnityEditor.AssetDatabase.CreateAsset(mTex2DArray, scene.path.Substring(0, scene.path.LastIndexOf("/")) + "/New Texture2DArray.asset");
            }
        }
    }
#endif
}