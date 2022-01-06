// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using UnityEngine;

// namespace Application.Logic
// {
//     public static class NetModuleUtility
//     {
//         public static Vector2 toVec2(this Vector2Float vec)
//         {
//             return new Vector2(vec.X, vec.Y);
//         }
//         public static Vector3 toVec3(this Vector3Float vec)
//         {
//             return new Vector3(vec.X, vec.Y, vec.Z);
//         }

//         public static Vector2Float toVec2(this Vector2 vec)
//         {
//             var vecNew = new Vector2Float();
//             vecNew.X = vec.x;
//             vecNew.Y = vec.y;
//             return vecNew;
//         }
//         public static Vector3Float toVec3(this Vector3 vec)
//         {
//             var vecNew = new Vector3Float();
//             vecNew.X = vec.x;
//             vecNew.Y = vec.y;
//             vecNew.Z = vec.z;
//             return vecNew;
//         }

//         public static Vector3Float toVec3(this float y)
//         {
//             var vecNew = new Vector3Float();
//             vecNew.X = 0;
//             vecNew.Y = y;
//             vecNew.Z = 0;
//             return vecNew;
//         }

//     }
// }
      