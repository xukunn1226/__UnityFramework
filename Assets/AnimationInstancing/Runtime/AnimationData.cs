using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Application.Runtime;
using Framework.AssetManagement.Runtime;

namespace AnimationInstancingModule.Runtime
{
    public class AnimationData : MonoBehaviour
    {
        public TextAsset                            manifest;
        public int                                  textureBlockWidth   { get; set; }
        public int                                  textureBlockHeight  { get; set; }
        public List<AnimationInfo>                  aniInfos            { get; private set; }
        public Dictionary<string, ExtraBoneInfo>    extraBoneInfos      { get; private set; }
        private AnimationInfo                       m_SearchInfo;
        private ComparerHash                        m_Comparer;
        [SoftObject] public SoftObject              animTexSoftObject;
        private Texture2D                           m_AnimTexture;      // 注意：异步加载，可能为NULL

        void Awake()
        {
            m_SearchInfo = new AnimationInfo();
            m_Comparer = new ComparerHash();
            
            // 1、数据轻量级；2、且涉及到动画逻辑数据，所以同步加载最佳
            using(BinaryReader reader = new BinaryReader(new MemoryStream(manifest.bytes)))
            {
                aniInfos = ReadAnimationInfo(reader);
                aniInfos.Sort(m_Comparer);      // 提高后续二分法查找效率
                PostprocessExtraBoneInfos();
                ReadAnimationTexture(reader);
            }
            manifest = null;        // 已无用，可以立即回收
        }

        IEnumerator Start()
        {
            // 动画贴图数据量大，异步加载
            AssetLoaderAsync<Object> loader = animTexSoftObject.LoadAssetAsync();
            yield return loader;
            TextAsset asset = (TextAsset)loader.asset;

            using(BinaryReader reader = new BinaryReader(new MemoryStream(asset.bytes)))
            {
                int textureWidth = reader.ReadInt32();
                int textureHeight = reader.ReadInt32();
                int byteSize = reader.ReadInt32();
                byte[] bytes = new byte[byteSize];
                bytes = reader.ReadBytes(byteSize);
                m_AnimTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBAHalf, false, true);
                m_AnimTexture.filterMode = FilterMode.Point;
                m_AnimTexture.LoadRawTextureData(bytes);
                m_AnimTexture.Apply();
            }

            animTexSoftObject?.UnloadAsset();       // 已无用，可以立即回收
        }

        public Texture2D GetAnimTexture()
        {
            return m_AnimTexture;
        }

#if UNITY_EDITOR
        public List<string> EditorLoadAnimationInfo()
        {
            List<AnimationInfo> list;
            using(BinaryReader reader = new BinaryReader(new MemoryStream(manifest.bytes)))
            {
                list = ReadAnimationInfo(reader);
            }

            List<string> names = new List<string>();
            foreach(var item in list)
            {
                names.Add(item.name);
            }
            return names;
        }
#endif        

        private List<AnimationInfo> ReadAnimationInfo(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            List<AnimationInfo>  listInfo = new List<AnimationInfo>();
            for (int i = 0; i != count; ++i)
            {
                // base
                AnimationInfo info = new AnimationInfo();
                info.name = reader.ReadString();
                info.nameHash = info.name.GetHashCode();
                info.startFrameIndex = reader.ReadInt32();
                info.totalFrame = reader.ReadInt32();
                info.fps = reader.ReadInt32();
                info.wrapMode = (WrapMode)reader.ReadInt32();
                
                // event
                int evtCount = reader.ReadInt32();
                info.eventList = new List<AnimationEvent>();
                for (int j = 0; j != evtCount; ++j)
                {
                    AnimationEvent evt = new AnimationEvent();
                    evt.function = reader.ReadString();
                    evt.floatParameter = reader.ReadSingle();
                    evt.intParameter = reader.ReadInt32();
                    evt.stringParameter = reader.ReadString();
                    evt.time = reader.ReadSingle();
                    evt.objectParameter = reader.ReadString();
                    info.eventList.Add(evt);
                }

                // extra bones
                int extraCount = reader.ReadInt32();
                info.extraBoneMatrix = new Dictionary<string, Matrix4x4[]>(extraCount);
                for(int j = 0; j < extraCount; ++j)
                {
                    string boneName = reader.ReadString();
                    int matrixCount = reader.ReadInt32();
                    Matrix4x4[] matrixs = new Matrix4x4[matrixCount];
                    for(int k = 0; k < matrixCount; ++k)
                    {
                        matrixs[k][0, 0] = reader.ReadSingle();
                        matrixs[k][0, 1] = reader.ReadSingle();
                        matrixs[k][0, 2] = reader.ReadSingle();
                        matrixs[k][0, 3] = reader.ReadSingle();
                        matrixs[k][1, 0] = reader.ReadSingle();
                        matrixs[k][1, 1] = reader.ReadSingle();
                        matrixs[k][1, 2] = reader.ReadSingle();
                        matrixs[k][1, 3] = reader.ReadSingle();
                        matrixs[k][2, 0] = reader.ReadSingle();
                        matrixs[k][2, 1] = reader.ReadSingle();
                        matrixs[k][2, 2] = reader.ReadSingle();
                        matrixs[k][2, 3] = reader.ReadSingle();
                        matrixs[k][3, 0] = reader.ReadSingle();
                        matrixs[k][3, 1] = reader.ReadSingle();
                        matrixs[k][3, 2] = reader.ReadSingle();
                        matrixs[k][3, 3] = reader.ReadSingle();
                    }
                    info.extraBoneMatrix.Add(boneName, matrixs);
                }

                listInfo.Add(info);
            }            
            return listInfo;
        }

        private void PostprocessExtraBoneInfos()
        {
            extraBoneInfos = new Dictionary<string, ExtraBoneInfo>();

            for(int i = 0; i < aniInfos.Count; ++i)
            {
                AnimationInfo info = aniInfos[i];
                foreach(var item in info.extraBoneMatrix)
                {
                    ExtraBoneInfo extraBoneInfo;
                    if (!extraBoneInfos.TryGetValue(item.Key, out extraBoneInfo))
                    {
                        extraBoneInfo = new ExtraBoneInfo();
                        extraBoneInfo.boneName = item.Key;
                        extraBoneInfo.boneMatrix = new List<Matrix4x4[]>();
                        extraBoneInfos.Add(item.Key, extraBoneInfo);
                    }
                    extraBoneInfo.boneMatrix.Add(item.Value);
                }                
            }
        }

        private void ReadAnimationTexture(BinaryReader reader)
        {
            textureBlockWidth = reader.ReadInt32();
            textureBlockHeight = reader.ReadInt32();            
        }

        internal int FindAnimationInfoIndex(int hash)
        {
            m_SearchInfo.nameHash = hash;
            return aniInfos.BinarySearch(m_SearchInfo, m_Comparer);
        }

        internal AnimationInfo FindAnimationInfo(int hash)
        {
            int index = FindAnimationInfoIndex(hash);
            if(index >= 0)
            {
                return aniInfos[index];
            }
            return null;
        }

        internal int GetAnimationCount()
        {
            return aniInfos.Count;
        }

        internal ExtraBoneInfo GetExtraBoneInfo(string extraBoneName)
        {
            ExtraBoneInfo boneInfo;
            extraBoneInfos.TryGetValue(extraBoneName, out boneInfo);
            return boneInfo;
        }
    }
}