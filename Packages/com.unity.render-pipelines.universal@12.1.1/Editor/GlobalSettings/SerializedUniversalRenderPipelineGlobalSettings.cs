using UnityEngine;
using System.Linq;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

namespace UnityEditor.Rendering.Universal
{
    class SerializedUniversalRenderPipelineGlobalSettings
    {
        public SerializedObject serializedObject;
        private List<UniversalRenderPipelineGlobalSettings> serializedSettings = new List<UniversalRenderPipelineGlobalSettings>();

        public SerializedProperty lightLayerName0;
        public SerializedProperty lightLayerName1;
        public SerializedProperty lightLayerName2;
        public SerializedProperty lightLayerName3;
        public SerializedProperty lightLayerName4;
        public SerializedProperty lightLayerName5;
        public SerializedProperty lightLayerName6;
        public SerializedProperty lightLayerName7;
        public SerializedProperty userLayerName8;
        public SerializedProperty userLayerName9;
        public SerializedProperty userLayerName10;
        public SerializedProperty userLayerName11;
        public SerializedProperty userLayerName12;
        public SerializedProperty userLayerName13;
        public SerializedProperty userLayerName14;
        public SerializedProperty userLayerName15;
        public SerializedProperty userLayerName16;
        public SerializedProperty userLayerName17;
        public SerializedProperty userLayerName18;
        public SerializedProperty userLayerName19;
        public SerializedProperty userLayerName20;
        public SerializedProperty userLayerName21;
        public SerializedProperty userLayerName22;
        public SerializedProperty userLayerName23;
        public SerializedProperty userLayerName24;
        public SerializedProperty userLayerName25;
        public SerializedProperty userLayerName26;
        public SerializedProperty userLayerName27;
        public SerializedProperty userLayerName28;
        public SerializedProperty userLayerName29;
        public SerializedProperty userLayerName30;
        public SerializedProperty userLayerName31;

        public SerializedProperty stripDebugVariants;
        public SerializedProperty stripUnusedPostProcessingVariants;
        public SerializedProperty stripUnusedVariants;

        public SerializedUniversalRenderPipelineGlobalSettings(SerializedObject serializedObject)
        {
            this.serializedObject = serializedObject;

            // do the cast only once
            foreach (var currentSetting in serializedObject.targetObjects)
            {
                if (currentSetting is UniversalRenderPipelineGlobalSettings urpSettings)
                    serializedSettings.Add(urpSettings);
                else
                    throw new System.Exception($"Target object has an invalid object, objects must be of type {typeof(UniversalRenderPipelineGlobalSettings)}");
            }


            lightLayerName0 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.lightLayerName0);
            lightLayerName1 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.lightLayerName1);
            lightLayerName2 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.lightLayerName2);
            lightLayerName3 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.lightLayerName3);
            lightLayerName4 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.lightLayerName4);
            lightLayerName5 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.lightLayerName5);
            lightLayerName6 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.lightLayerName6);
            lightLayerName7 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.lightLayerName7);
            userLayerName8 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName8);
            userLayerName9 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName9);
            userLayerName10 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName10);
            userLayerName11 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName11);
            userLayerName12 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName12);
            userLayerName13 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName13);
            userLayerName14 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName14);
            userLayerName15 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName15);
            userLayerName16 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName16);
            userLayerName17 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName17);
            userLayerName18 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName18);
            userLayerName19 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName19);
            userLayerName20 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName20);
            userLayerName21 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName21);
            userLayerName22 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName22);
            userLayerName23 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName23);
            userLayerName24 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName24);
            userLayerName25 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName25);
            userLayerName26 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName26);
            userLayerName27 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName27);
            userLayerName28 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName28);
            userLayerName29 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName29);
            userLayerName30 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName30);
            userLayerName31 = serializedObject.Find((UniversalRenderPipelineGlobalSettings s) => s.userLayerName31);

            stripDebugVariants = serializedObject.FindProperty("m_StripDebugVariants");
            stripUnusedPostProcessingVariants = serializedObject.FindProperty("m_StripUnusedPostProcessingVariants");
            stripUnusedVariants = serializedObject.FindProperty("m_StripUnusedVariants");
        }
    }
}
