using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;

namespace Application.Runtime
{   
    public class IEnumerable_1_ObjectAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(System.Collections.Generic.IEnumerable<System.Object>);
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adapter);
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adapter(appdomain, instance);
        }

        public class Adapter : System.Collections.Generic.IEnumerable<System.Object>, CrossBindingAdaptorType
        {
            CrossBindingFunctionInfo<System.Collections.Generic.IEnumerator<System.Object>> mGetEnumerator_0 = new CrossBindingFunctionInfo<System.Collections.Generic.IEnumerator<System.Object>>("GetEnumerator");

            bool isInvokingToString;
            ILTypeInstance instance;
            ILRuntime.Runtime.Enviorment.AppDomain appdomain;

            public Adapter()
            {

            }

            public Adapter(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance { get { return instance; } }

            public System.Collections.Generic.IEnumerator<System.Object> GetEnumerator()
            {
                return mGetEnumerator_0.Invoke(this.instance);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                // throw new System.NotImplementedException();
                return GetEnumerator();
            }

            public override string ToString()
            {
                IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
                m = instance.Type.GetVirtualMethod(m);
                if (m == null || m is ILMethod)
                {
                    if (!isInvokingToString)
                    {
                        isInvokingToString = true;
                        string res = instance.ToString();
                        isInvokingToString = false;
                        return res;
                    }
                    else
                        return instance.Type.FullName;
                }
                else
                    return instance.Type.FullName;
            }
        }
    }
}
