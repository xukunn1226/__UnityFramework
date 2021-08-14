using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace AnimationInstancingModule.Runtime
{
    public class AnimationData : MonoBehaviour
    {
        public TextAsset                manifest;
        public Texture2D                animationTexture;
        public int                      textureBlockWidth   { get; set; }
        public int                      textureBlockHeight  { get; set; }
        // public bool                     isReady             { get; set; }
        public List<AnimationInfo>      aniInfos            { get; private set; }
        public ExtraBoneInfo            extraBoneInfo       { get; private set; }
        private AnimationInfo           m_SearchInfo;
        private ComparerHash            m_Comparer;

        void Awake()
        {
            m_SearchInfo = new AnimationInfo();
            m_Comparer = new ComparerHash();

            // 1、数据轻量级；2、且涉及到动画逻辑数据，所以同步加载最佳
            // isDone = false;
            using(BinaryReader reader = new BinaryReader(new MemoryStream(manifest.bytes)))
            {
                aniInfos = ReadAnimationInfo(reader);
                extraBoneInfo = ReadExtraBoneInfo(reader);
                ReadAnimationTexture(reader);
            }
            // isDone = true;
        }

        private List<AnimationInfo> ReadAnimationInfo(BinaryReader reader)
        {
            int count = reader.ReadInt32();
            List<AnimationInfo>  listInfo = new List<AnimationInfo>();
            for (int i = 0; i != count; ++i)
            {
                AnimationInfo info = new AnimationInfo();
                info.name = reader.ReadString();
                info.nameHash = info.name.GetHashCode();
                info.startFrameIndex = reader.ReadInt32();
                info.totalFrame = reader.ReadInt32();
                info.fps = reader.ReadInt32();
                info.wrapMode = (WrapMode)reader.ReadInt32();
                
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
                listInfo.Add(info);
            }
            listInfo.Sort(m_Comparer);      // 提高后续二分法查找效率
            return listInfo;
        }

        private ExtraBoneInfo ReadExtraBoneInfo(BinaryReader reader)
        {
            ExtraBoneInfo info = null;
            if (reader.ReadBoolean())
            {
                info = new ExtraBoneInfo();
                int count = reader.ReadInt32();
                info.extraBone = new string[count];
                info.extraBindPose = new Matrix4x4[count];
                for (int i = 0; i != info.extraBone.Length; ++i)
                {
                    info.extraBone[i] = reader.ReadString();
                }
                for (int i = 0; i != info.extraBindPose.Length; ++i)
                {
                    for (int j = 0; j != 16; ++j)
                    {
                        info.extraBindPose[i][j] = reader.ReadSingle();
                    }
                }
            }
            return info;
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
    }
}