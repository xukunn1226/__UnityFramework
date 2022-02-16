using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRBindings
    {

//will auto register in unity
#if UNITY_5_3_OR_NEWER
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        static private void RegisterBindingAction()
        {
            ILRuntime.Runtime.CLRBinding.CLRBindingUtils.RegisterBindingAction(Initialize);
        }

        internal static ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector3> s_UnityEngine_Vector3_Binding_Binder = null;
        internal static ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector2> s_UnityEngine_Vector2_Binding_Binder = null;
        internal static ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Quaternion> s_UnityEngine_Quaternion_Binding_Binder = null;

        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            System_Collections_Generic_Dictionary_2_Int32_ILTypeInstance_Binding.Register(app);
            System_Int32_Binding.Register(app);
            System_String_Binding.Register(app);
            UnityEngine_Random_Binding.Register(app);
            UnityEngine_Vector3_Binding.Register(app);
            System_ArgumentException_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_ILTypeInstance_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_Int32_ILTypeInstance_Binding.Register(app);
            System_IDisposable_Binding.Register(app);
            UnityEngine_Debug_Binding.Register(app);
            UnityEngine_Object_Binding.Register(app);
            UnityEngine_GameObject_Binding.Register(app);
            UnityEngine_Transform_Binding.Register(app);
            System_Collections_Generic_List_1_ILTypeInstance_Binding.Register(app);
            System_Activator_Binding.Register(app);
            System_Collections_Generic_List_1_ILTypeInstance_Binding_Enumerator_Binding.Register(app);
            System_Object_Binding.Register(app);
            System_Type_Binding.Register(app);
            AnimationInstancingModule_Runtime_AnimationInstancing_Binding.Register(app);
            System_Threading_Interlocked_Binding.Register(app);
            System_Action_Binding.Register(app);
            UnityEngine_Mathf_Binding.Register(app);
            UnityEngine_Quaternion_Binding.Register(app);
            System_ArgumentNullException_Binding.Register(app);
            Framework_AssetManagement_Runtime_AssetManager_Binding.Register(app);
            Framework_AssetManagement_Runtime_GameObjectLoader_Binding.Register(app);
            Application_Runtime_SqlData_Binding.Register(app);
            System_Data_Common_DbDataReader_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Dictionary_2_Int32_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Dictionary_2_String_Dictionary_2_String_Dictionary_2_Int32_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Dictionary_2_String_Dictionary_2_Int32_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Dictionary_2_Int32_ILTypeInstance_Binding.Register(app);
            Application_Runtime_Launcher_Binding.Register(app);
            Application_Runtime_ConfigBuilderSetting_Binding.Register(app);
            UnityEngine_Application_Binding.Register(app);
            Framework_Core_Utility_Binding.Register(app);
            System_IO_Path_Binding.Register(app);
            System_Array_Binding.Register(app);
            System_Byte_Binding.Register(app);
            Framework_Core_DownloadTask_Binding.Register(app);
            Framework_Core_DownloadTaskInfo_Binding.Register(app);
            System_Uri_Binding.Register(app);
            System_Collections_Generic_List_1_String_Binding.Register(app);
            System_Collections_Generic_List_1_Int32_Binding.Register(app);
            System_Single_Binding.Register(app);
            System_Collections_Generic_List_1_Single_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Int32_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_String_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Single_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Int32_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_String_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Single_Binding.Register(app);
            UnityEngine_LayerMask_Binding.Register(app);
            Application_Runtime_StarterAssetsInputs_Binding.Register(app);
            UnityEngine_Physics_Binding.Register(app);
            Application_Runtime_MyPlayerBehaviour_Binding.Register(app);
            UnityEngine_Color_Binding.Register(app);
            UnityEngine_Gizmos_Binding.Register(app);
            UnityEngine_Vector2_Binding.Register(app);
            UnityEngine_Time_Binding.Register(app);
            Application_Runtime_CoroutineExtension_Binding.Register(app);
            Application_Runtime_CoroutineHandler_Binding.Register(app);
            UnityEngine_Events_UnityEvent_1_Boolean_Binding.Register(app);
            Application_Runtime_CodeLoader_Binding.Register(app);
            System_NotSupportedException_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Enum_Action_1_ILTypeInstance_Binding.Register(app);
            System_Action_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_ILTypeInstance_Binding.Register(app);
            UnityEngine_Rect_Binding.Register(app);
            UnityEngine_GUI_Binding.Register(app);
            System_Random_Binding.Register(app);
            System_GC_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_WeakReference_Binding.Register(app);
            System_WeakReference_Binding.Register(app);
            Framework_Core_StreamingLevelManager_Binding.Register(app);
            Framework_Core_StreamingLevelManager_Binding_LevelContext_Binding.Register(app);
            Framework_Core_SingletonMono_1_StreamingLevelManager_Binding.Register(app);
            Cinemachine_CinemachineVirtualCamera_Binding.Register(app);
            UnityEngine_QualitySettings_Binding.Register(app);
            UnityEngine_Screen_Binding.Register(app);
            UnityEngine_Resolution_Binding.Register(app);
            UnityEngine_Rendering_OnDemandRendering_Binding.Register(app);
            System_Collections_Generic_LinkedList_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_LinkedList_1_ILTypeInstance_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_LinkedListNode_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_List_1_Type_Binding.Register(app);
            System_Linq_Enumerable_Binding.Register(app);
            Framework_Cache_LRUQueue_2_String_RectTransform_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding_Enumerator_Binding.Register(app);
            UnityEngine_Component_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_RectTransform_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_RectTransform_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_String_RectTransform_Binding.Register(app);
            System_Collections_IEnumerator_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_String_ILTypeInstance_Binding.Register(app);
            Framework_Core_SingletonMono_1_AsyncLoaderManager_Binding.Register(app);
            Application_Runtime_AsyncLoaderManager_Binding.Register(app);
            Framework_Core_GameDebug_Binding.Register(app);
            UnityEngine_Behaviour_Binding.Register(app);
            Application_Runtime_WorldPlayerController_Binding.Register(app);
            Application_Runtime_PlayerController_Binding.Register(app);

            ILRuntime.CLR.TypeSystem.CLRType __clrType = null;
            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof(UnityEngine.Vector3));
            s_UnityEngine_Vector3_Binding_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector3>;
            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof(UnityEngine.Vector2));
            s_UnityEngine_Vector2_Binding_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector2>;
            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof(UnityEngine.Quaternion));
            s_UnityEngine_Quaternion_Binding_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Quaternion>;
        }

        /// <summary>
        /// Release the CLR binding, please invoke this BEFORE ILRuntime Appdomain destroy
        /// </summary>
        public static void Shutdown(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            s_UnityEngine_Vector3_Binding_Binder = null;
            s_UnityEngine_Vector2_Binding_Binder = null;
            s_UnityEngine_Quaternion_Binding_Binder = null;
        }
    }
}
