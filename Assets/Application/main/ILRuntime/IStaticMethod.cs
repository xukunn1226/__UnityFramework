using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public interface IStaticMethod
    {
        void Exec();
        void Exec(object p);
        void Exec(object p1, object p2);
        void Exec(object p1, object p2, object p3);
    }
}