using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnimationInstancingModule.Runtime;

namespace Application.Runtime
{
    public class AnimationInstancingRenderable : Renderable
    {
        private AnimationInstancing m_Inst;
        
        public AnimationInstancingRenderable(ZActor actor) : base(actor) {}

        public override void Load(string assetPath)
        {
            base.Load(assetPath);
            m_Inst = renderer.GetComponent<AnimationInstancing>();
            PlayAnimation("idle");
        }

        public void PlayAnimation(string name, float transitionDuration = 0)
        {
            m_Inst?.PlayAnimation(name, transitionDuration);
        }

        public void SetRendererSpeed(float speed, float scale)
        {
            if(m_Inst != null)
            {
                m_Inst.speed = speed;
                m_Inst.speedScale = scale;
            }
        }
    }
}