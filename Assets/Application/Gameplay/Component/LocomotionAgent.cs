using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public class LocomotionAgent : ZComp
    {
        public int id { get; private set; }
        private LinkedList<Vector3> m_PathNodeList = new LinkedList<Vector3>();

        public LocomotionAgent(ZActor actor) : base(actor) {}

        public override void Prepare(IData data)
        {
            base.Prepare(data);
        }

        public override void Start()
        {
            base.Start();
            id = LocomotionManager.AddInstance(this);
            EnterView(Vector3.zero);
        }

        public override void Destroy()
        {
            LeaveView();
            LocomotionManager.RemoveInstance(this);
            base.Destroy();
        }

        public void EnterView(Vector3 pos)
        {}

        public void LeaveView()
        {}

        public void AddPathNode(Vector3 pos)
        {
            m_PathNodeList.AddLast(pos);
        }
    }
}