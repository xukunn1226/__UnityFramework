using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationInstancingModule.Runtime.Tests
{
    public class Sword : MonoBehaviour, IAttachmentToInstancing
    {
        public AnimationInstancing  owner { get; set; }
        public string extraBoneName { get; set; }
        public void SetParent(Transform parent)
        {
            transform.parent = parent;
        }

        public void SetPosition(Vector3 pos)
        {
            transform.position = pos;
        }

        public void SetRotation(Quaternion rot)
        {
            transform.rotation = rot;
        }
        
        public void Detach()
        {
            if(owner == null)
                return;

            owner.Detach(extraBoneName, this);
        }

        public void Attach(AnimationInstancing owner, string extraBoneName)
        {
            this.owner = owner;
            this.extraBoneName = extraBoneName;

            owner.Attach(extraBoneName, this);
        }
    }
}