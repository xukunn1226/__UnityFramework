using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationInstancingModule.Runtime.Tests
{
    public class AttachmentSampler : MonoBehaviour
    {
        private AnimationInstancing inst;
        public GameObject attachment;
        private GameObject attachmentInst;

        // Start is called before the first frame update
        void Awake()
        {
            inst = GetComponent<AnimationInstancing>();
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.F1))
            {
                Debug.Log("F1");
                inst.PlayAnimation("attack02");
            }

            if(Input.GetKeyDown(KeyCode.F2))
            {
                Debug.Log("F2");
                inst.PlayAnimation("attack03", 0.5f);
            }

            if(Input.GetKeyDown(KeyCode.A))
            {
                attachmentInst = Instantiate<GameObject>(attachment);
                Sword sword = attachmentInst.AddComponent<Sword>();

                // method 1
                // inst.Attach("ik_hand_r", sword);

                // method 2
                sword.Attach(inst, "ik_hand_r");
            }

            if(Input.GetKeyDown(KeyCode.Q))
            {
                Sword sword = attachmentInst.GetComponent<Sword>();

                // method 1
                // inst.Detach("ik_hand_r", sword);

                // method 2
                sword.Detach();

                Destroy(attachmentInst.gameObject);
            }
        }
    }
}