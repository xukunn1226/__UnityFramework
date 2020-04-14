using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MeshParticleSystem
{
    public static class FX_Const
    {
        public static Keyframe[] defaultKeyFrames           = { new Keyframe(0, 0), new Keyframe(1, 1) };

        // usual shader property id
        public static int PropID_MainTexST                  = Shader.PropertyToID("_MainTex_ST");                       // Vector4      0
        public static int PropID_TintColor                  = Shader.PropertyToID("_TintColor");                        // Color        1
        public static int PropID_Color                      = Shader.PropertyToID("_Color");                            // Color        2
        public static int PropID_PowerColor                 = Shader.PropertyToID("_PowerColor");                       // Color        3
        public static int PropID_Transparency               = Shader.PropertyToID("_Transparency");                     // Float        4
        public static int PropID_Intensity                  = Shader.PropertyToID("_Intensity");                        // Float        5
        public static int PropID_EmissionGain               = Shader.PropertyToID("_EmissionGain");                     // Float        6
        public static int PropID_Params                     = Shader.PropertyToID("_Params");                           // Vector4      7
        public static int PropID_Area                       = Shader.PropertyToID("_Area");                             // Vector4      8

        // DissolutionAddGlow.shader
        public static int PropID_FanZhuan_Mask              = Shader.PropertyToID("_FanZhuan_Mask");                    // Float        9
        public static int PropID_XR_Sceen                   = Shader.PropertyToID("_XR_Sceen");                         // Float        10
        public static int PropID_XR_color                   = Shader.PropertyToID("_XR_color");                         // Color        11
        public static int PropID_XiaoRong                   = Shader.PropertyToID("_XiaoRong");                         // Float        12
        public static int PropID_XiaoRong_Bian              = Shader.PropertyToID("_XiaoRong_Bian");                    // Float        13
        public static int PropID_ZiFaGuang                  = Shader.PropertyToID("_ZiFaGuang");                        // Float        14

        // HeatDistortion.shader
        public static int PropID_HeatForce                  = Shader.PropertyToID("_HeatForce");                        // Float        15
        public static int PropID_HeatIntensity              = Shader.PropertyToID("_HeatIntensity");                    // Float        16
        public static int PropID_HeatTime                   = Shader.PropertyToID("_HeatTime");                         // Float        17

        // niudong.shader && niudong1.shader && niudong_trail 
        public static int PropID_dissolve_on                = Shader.PropertyToID("_dissolve_on");                      // Float        18
        public static int PropID_RotaSpeed                  = Shader.PropertyToID("_RotaSpeed");                        // Float        19
        public static int PropID_Distortion                 = Shader.PropertyToID("_Distortion");                       // Vector4      20

        // Particle_mesh_dissolve_glow_edge.shader
        public static int PropID_EdgeParams                 = Shader.PropertyToID("_EdgeParams");                       // Vector4      21
        public static int PropID_EdgeTintColor              = Shader.PropertyToID("_EdgeTintColor");                    // Color        22

        // s_dissolution_add.shader
        public static int PropID_C_BYcolor                  = Shader.PropertyToID("_C_BYcolor");                        // Color        23
        public static int PropID_N_BY_KD                    = Shader.PropertyToID("_N_BY_KD");                          // Float        24
        public static int PropID_N_BY_QD                    = Shader.PropertyToID("_N_BY_QD");                          // Float        25
        public static int PropID_N_mask                     = Shader.PropertyToID("_N_mask");                           // Float        26


        // s_Fresnel_Blend.shader && s_Fresnel_Opaque.shader
        public static int PropID_DiffuseColor               = Shader.PropertyToID("_DiffuseColor");                     // Color        27
        public static int PropID_DiffuseQD                  = Shader.PropertyToID("_DiffuseQD");                        // Float        28
        public static int PropID_FnlColor                   = Shader.PropertyToID("_FnlColor");                         // Color        29
        public static int PropID_FnlExp                     = Shader.PropertyToID("_FnlExp");                           // Float        30
        public static int PropID_FnlQD                      = Shader.PropertyToID("_FnlQD");                            // Float        31
        public static int PropID_MaskExp                    = Shader.PropertyToID("_MaskExp");                          // Float        32
        public static int PropID_MaskGB                     = Shader.PropertyToID("_MaskGB");                           // Float        33
        public static int PropID_MaskGBColor                = Shader.PropertyToID("_MaskGBColor");                      // Color        34
        public static int PropID_MaskGBExp                  = Shader.PropertyToID("_MaskGBExp");                        // Float        35

        // s_RimLight.shader
        public static int PropID_RimColor                   = Shader.PropertyToID("_RimColor");                         // Color        36
        public static int PropID_RimWidth                   = Shader.PropertyToID("_RimWidth");                         // Float        37

        // s_TransparentRim.shader
        public static int PropID_AllPower                   = Shader.PropertyToID("_AllPower");                         // Float        38
        public static int PropID_AlphaPower                 = Shader.PropertyToID("_AlphaPower");                       // Float        39
        public static int PropID_InnerColor                 = Shader.PropertyToID("_InnerColor");                       // Color        40
        public static int PropID_InnerColorPower            = Shader.PropertyToID("_InnerColorPower");                  // Float        41
        public static int PropID_RimPower                   = Shader.PropertyToID("_RimPower");                         // Float        42

        // s_UVAnim_Add.shader
        public static int PropID_U                          = Shader.PropertyToID("_U");                                // Float        43
        public static int PropID_V                          = Shader.PropertyToID("_V");                                // Float        44

        // s_UVSwingAddMask.shader
        public static int PropID_SwingPower                 = Shader.PropertyToID("_SwingPower");                       // Float        45
        public static int PropID_Swing_Power                = Shader.PropertyToID("_Swing_Power");                      // Float        46
        public static int PropID_WaterCoeff                 = Shader.PropertyToID("_WaterCoeff");                       // Vector4      47

        // SoftAdditiveGenerated.shader
        public static int PropID_InvFade                    = Shader.PropertyToID("_InvFade");                          // Float        48

        // ParallaxGenerated.shader
        public static int PropID_ParallaxScale              = Shader.PropertyToID("_ParallaxScale");                    // Float        49

        public static int PropID_ClipToLocal                = Shader.PropertyToID("_ClipToLocal");                      // Matrix       50



        // PropID动态生成，不唯一，不可序列化，所以需要映射
        public static int[] SerializedIDToPropID =
            {
                PropID_MainTexST,                           // 0
                PropID_TintColor,                           // 1
                PropID_Color,                               // 2
                PropID_PowerColor,                          // 3
                PropID_Transparency,                        // 4
                PropID_Intensity,                           // 5
                PropID_EmissionGain,                        // 6
                PropID_Params,                              // 7
                PropID_Area,                                // 8
                PropID_FanZhuan_Mask,                       // 9
                PropID_XR_Sceen,                            // 10
                PropID_XR_color,                            // 11
                PropID_XiaoRong,                            // 12
                PropID_XiaoRong_Bian,                       // 13
                PropID_ZiFaGuang,                           // 14
                PropID_HeatForce,                           // 15
                PropID_HeatIntensity,                       // 16
                PropID_HeatTime,                            // 17
                PropID_dissolve_on,                         // 18
                PropID_RotaSpeed,                           // 19
                PropID_Distortion,                          // 20
                PropID_EdgeParams,                          // 21
                PropID_EdgeTintColor,                       // 22
                PropID_C_BYcolor,                           // 23
                PropID_N_BY_KD,                             // 24
                PropID_N_BY_QD,                             // 25
                PropID_N_mask,                              // 26
                PropID_DiffuseColor,                        // 27
                PropID_DiffuseQD,                           // 28
                PropID_FnlColor,                            // 29
                PropID_FnlExp,                              // 30
                PropID_FnlQD,                               // 31
                PropID_MaskExp,                             // 32
                PropID_MaskGB,                              // 33
                PropID_MaskGBColor,                         // 34
                PropID_MaskGBExp,                           // 35
                PropID_RimColor,                            // 36
                PropID_RimWidth,                            // 37
                PropID_AllPower,                            // 38
                PropID_AlphaPower,                          // 39
                PropID_InnerColor,                          // 40
                PropID_InnerColorPower,                     // 41
                PropID_RimPower,                            // 42
                PropID_U,                                   // 43
                PropID_V,                                   // 44
                PropID_SwingPower,                          // 45
                PropID_Swing_Power,                         // 46
                PropID_WaterCoeff,                          // 47
                PropID_InvFade,                             // 48
                PropID_ParallaxScale,                       // 49
                PropID_ClipToLocal,                         // 50
            };



#if UNITY_EDITOR
        public static string[] colorDisplayedOptions =
        {
            "_TintColor",
            "_Color",
            "_PowerColor",
            "_XR_color",
            "_EdgeTintColor",
            "_C_BYcolor",
            "_DiffuseColor",
            "_FnlColor",
            "_MaskGBColor",
            "_RimColor",
            "_InnerColor",
        };

        public static int[] colorOptionValues =
        {
            1,
            2,
            3,
            11,
            22,
            23,
            27,
            29,
            34,
            36,
            40,
        };

        public static string[] floatDisplayedOptions =
        {
            "_Transparency",
            "_Intensity",
            "_EmissionGain",
            "_FanZhuan_Mask",
            "_XR_Sceen",
            "_XiaoRong",
            "_XiaoRong_Bian",
            "_ZiFaGuang",
            "_HeatForce",
            "_HeatIntensity",
            "_HeatTime",
            "_dissolve_on",
            "_RotaSpeed",
            "_N_BY_KD",
            "_N_BY_QD",
            "_N_mask",
            "_DiffuseQD",
            "_FnlExp",
            "_FnlQD",
            "_MaskExp",
            "_MaskGB",
            "_MaskGBExp",
            "_RimWidth",
            "_AllPower",
            "_AlphaPower",
            "_InnerColorPower",
            "_RimPower",
            "_U",
            "_V",
            "_SwingPower",
            "_Swing_Power",
            "_InvFade",
            "_ParallaxScale",
        };

        public static int[] floatOptionValues =
        {
            4,
            5,
            6,
            9,
            10,
            12,
            13,
            14,
            15,
            16,
            17,
            18,
            19,
            24,
            25,
            26,
            28,
            30,
            31,
            32,
            33,
            35,
            37,
            38,
            39,
            41,
            42,
            43,
            44,
            45,
            46,
            48,
            49,
        };
        
        public static string[] vector4DisplayedOptions =
        {
            "_Params",
            "_Area",
            "_Distortion",
            "_EdgeParams",
            "_WaterCoeff",
        };

        public static int[] vector4OptionValues =
        {
            7,
            8,
            20,
            21,
            47,
        };

        public static int floatDisplayedNameToID(string name)
        {
            int index = Array.IndexOf<string>(floatDisplayedOptions, name);
            if (index != -1 && index < floatOptionValues.Length)
                return floatOptionValues[index];
            return -1;
        }

        public static int colorDisplayedNameToID(string name)
        {
            int index = Array.IndexOf<string>(colorDisplayedOptions, name);
            if (index != -1 && index < colorOptionValues.Length)
                return colorOptionValues[index];
            return -1;
        }

        public static int vector4DisplayedNameToID(string name)
        {
            int index = Array.IndexOf<string>(vector4DisplayedOptions, name);
            if (index != -1 && index < vector4OptionValues.Length)
                return vector4OptionValues[index];
            return -1;
        }
#endif
    }
}