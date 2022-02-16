using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace Application.Runtime
{
    public class MonoMemberMethod : IMemberMethod
    {
        private readonly MethodInfo m_Method;
        private readonly object[] m_Params;
        public MonoMemberMethod(Assembly asm, string typename, string methodname)
        {
            m_Method = asm.GetType(typename).GetMethod(methodname);
            m_Params = new object[m_Method.GetParameters().Length];
        }

        public System.Object Exec(object inst)
        {
            return m_Method.Invoke(inst, m_Params);
        }

        public System.Object Exec(object inst, object p)
        {
            m_Params[0] = p;
            return m_Method.Invoke(inst, m_Params);
        }

        public System.Object Exec(object inst, object p1, object p2)
        {
            m_Params[0] = p1;
            m_Params[1] = p2;
            return m_Method.Invoke(inst, m_Params);
        }

        public System.Object Exec(object inst, object p1, object p2, object p3)
        {
            m_Params[0] = p1;
            m_Params[1] = p2;
            m_Params[2] = p3;
            return m_Method.Invoke(inst, m_Params);
        }
    }
}