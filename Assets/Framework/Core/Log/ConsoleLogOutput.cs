using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    [RequireComponent(typeof(GameDebug))]
    public class ConsoleLogOutput : BaseLogOutput
    {
        private static readonly Color[] logTypeColors = new Color[]
        {
            Color.red,          // error
            Color.green,        // assert
            Color.yellow,       // warning
            Color.white,        // log
            Color.magenta,      // exception
        };

        struct LogInfo
        {
            public string logString;
            public string stackTrace;
            public LogType type;
        }
        private LinkedList<LogInfo> m_Logs = new LinkedList<LogInfo>();

        public int      MaxCountOfLog = 999;
        public bool     ShakeToOpen = true;
        public float    ShakeAcceleration = 100f;
        [HideInInspector] public bool     StackLog = false;
        [HideInInspector] public bool     Collapse;
        [HideInInspector] public float    ScrollbarSize = 25;
        [HideInInspector] public bool     AutoScroll;

        private Rect    m_WindowRect = new Rect(20, 40, 800, 600);
        private Vector2 m_ScrollPosition;        
        public bool     m_isShow;
        private Rect    m_ResizerRect;
        private bool    m_Resizing;        

//         protected override void OnEnable()
//         {
// #if ENABLE_PROFILER
//             base.OnEnable();
// #endif
//         }

//         protected override void OnDisable()
//         {
// #if ENABLE_PROFILER
//             base.OnDisable();
// #endif            
//         }

        public override void Output(string logString, string stackTrace, LogType type)
        {
            m_Logs.AddLast(new LogInfo() {logString = "<size=20>" + logString + "</size>", stackTrace = "<size=14>" + stackTrace + "</size>", type = type });

            if(m_Logs.Count > MaxCountOfLog)
            {
                m_Logs.RemoveFirst();
            }
        }

        public override void Flush() {}

        public override void Dispose() {}

        // [System.Diagnostics.Conditional("ENABLE_PROFILER")]
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                m_isShow = !m_isShow;
            }
            if (ShakeToOpen && Input.acceleration.sqrMagnitude > ShakeAcceleration)
            {
                m_isShow = true;
            }
            if(Input.touchCount > 3)
            {
                m_isShow = true;
            }
        }

        void OnGUI()
        {
            if(!m_isShow)
                return;

            HandleResize();
            m_WindowRect = GUI.Window(0, m_WindowRect, DrawConsoleWindow, "Debug Window");
        }

        void DrawConsoleWindow(int windowID)
        {            
            DrawLogItems();
            DrawToolbar();
       
            GUI.DragWindow(new Rect(0, 0, 10000, 60));
        }
        
        void HandleResize()
        {
            float size = 25;
            m_ResizerRect = new Rect(m_WindowRect.position.x + m_WindowRect.width - size, m_WindowRect.position.y + m_WindowRect.height - size, size, size);

            if(Event.current.type == EventType.MouseDown && m_ResizerRect.Contains(Event.current.mousePosition))
            {
                m_Resizing = true;                
            }

            if (Event.current.type == EventType.MouseUp)
            {
                m_Resizing = false;
            }

            if(m_Resizing)
            {
                m_WindowRect.width = Event.current.mousePosition.x - m_WindowRect.position.x;
                m_WindowRect.height = Event.current.mousePosition.y - m_WindowRect.position.y;
            }
        }

        void DrawLogItems()
        {
            GUIStyle gs_vertica = GUI.skin.verticalScrollbar;
            GUIStyle gs1_vertica = GUI.skin.verticalScrollbarThumb;

            gs_vertica.fixedWidth = ScrollbarSize;
            gs1_vertica.fixedWidth = ScrollbarSize;

            GUIStyle gs_horizontal = GUI.skin.horizontalScrollbar;
            GUIStyle gs1_horizontal = GUI.skin.horizontalScrollbarThumb;

            gs_horizontal.fixedHeight = ScrollbarSize;
            gs1_horizontal.fixedHeight = ScrollbarSize;

            if(AutoScroll)
                m_ScrollPosition.y += 20;
            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition, false, false);

            LinkedListNode<LogInfo> cur = m_Logs.First;
            while(cur != null)
            {
                if(Collapse)
                {
                    LinkedListNode<LogInfo> prev = cur.Previous;
                    if(prev != null && prev.Value.logString == cur.Value.logString)
                    {
                        cur = cur.Next;
                        continue;
                    }
                }
                GUI.contentColor = logTypeColors[(int)cur.Value.type];
                GUILayout.Label(cur.Value.logString);
                if (StackLog)
                {
                    GUILayout.Label(cur.Value.stackTrace);
                }
                cur = cur.Next;
            }

            GUILayout.EndScrollView();

            GUI.contentColor = Color.white;
        }

        private void DrawToolbar()
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Clear", GUILayout.Height(20)))
            {
                m_Logs.Clear();
            }

            if (GUILayout.Button("Close", GUILayout.Height(20)))
            {
                m_isShow = false;
            }
            AutoScroll = GUILayout.Toggle(AutoScroll, "Auto Scroll", GUILayout.Width(100), GUILayout.Height(20));
            Collapse = GUILayout.Toggle(Collapse, "Collapse", GUILayout.Width(100), GUILayout.Height(20));
            StackLog = GUILayout.Toggle(StackLog, "Show Stack", GUILayout.Width(100), GUILayout.Height(20));
            GameDebug.EnableLog = GUILayout.Toggle(GameDebug.EnableLog, "Enable Log", GUILayout.Width(100), GUILayout.Height(20));
            GUILayout.Space(50);

            GUILayout.EndHorizontal();
        }
    }
}