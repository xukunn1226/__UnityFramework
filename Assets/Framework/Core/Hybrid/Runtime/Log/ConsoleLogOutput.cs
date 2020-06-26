using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    [RequireComponent(typeof(GameDebug))]
    public class ConsoleLogOutput : BaseLogOutput
    {
        public int m_MaxCountOfLog = 200;
        private const int m_Margin = 20;

        private Rect m_WindowRect = new Rect(m_Margin, m_Margin, 800, 600);

        public override void Output(string logString, string stackTrace, LogType type)
        {}

        public override void Flush()
        {}

        public override void Dispose()
        {}

        void OnGUI()
        {
            m_WindowRect = GUI.Window(0, m_WindowRect, DrawConsoleWindow, "Debug Window");

            // GUI.Button(new Rect(10, 20, 100, 20), "Can't drag me");
        }

        void DrawConsoleWindow(int windowID)
        {
            GUI.Button(new Rect(10, 20, 100, 20), "Can't drag me");
            
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
    }
}