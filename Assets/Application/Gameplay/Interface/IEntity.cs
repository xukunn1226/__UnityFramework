using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Application.Runtime
{
    public interface IEntity
    {
        string  name    { get; set; }
        int     id      { get; set; }
        void    Init();
        void    Uninit();
    }
}