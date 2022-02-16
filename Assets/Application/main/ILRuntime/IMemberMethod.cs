using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public interface IMemberMethod
    {
        System.Object Exec(object inst);
        System.Object Exec(object inst, object p);
        System.Object Exec(object inst, object p1, object p2);
        System.Object Exec(object inst, object p1, object p2, object p3);
    }
}