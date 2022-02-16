using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;

namespace Application.Runtime
{
    public class ILMemberMethod : IMemberMethod
    {
        private readonly AppDomain m_AppDomain;
        private readonly IMethod m_Method;
        private readonly object[] m_Params;

        public ILMemberMethod(AppDomain appDomain, string typename, string methodname, int paramCount)
        {
            m_AppDomain = appDomain;
            m_Method = m_AppDomain.GetType(typename).GetMethod(methodname, paramCount);
            m_Params = new object[paramCount];
        }

        public System.Object Exec(object inst)
        {
            return m_AppDomain.Invoke(m_Method, inst, m_Params);
        }

        public System.Object Exec(object inst, object p)
        {
            m_Params[0] = p;
            return m_AppDomain.Invoke(m_Method, inst, m_Params);
        }

        public System.Object Exec(object inst, object p1, object p2)
        {
            m_Params[0] = p1;
            m_Params[1] = p2;
            return m_AppDomain.Invoke(m_Method, inst, m_Params);
        }

        public System.Object Exec(object inst, object p1, object p2, object p3)
        {
            m_Params[0] = p1;
            m_Params[1] = p2;
            m_Params[2] = p3;
            return m_AppDomain.Invoke(m_Method, inst, m_Params);
        }
    }
}