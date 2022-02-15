using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public interface IStaticMethod
    {
        System.Object Exec();
        System.Object Exec(object p);
        System.Object Exec(object p1, object p2);
        System.Object Exec(object p1, object p2, object p3);
    }
}