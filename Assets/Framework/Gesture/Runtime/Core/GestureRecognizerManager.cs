using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Gesture.Runtime
{
    internal static class GestureRecognizerManager
    {
        private static readonly List<GestureRecognizer> s_Recognizers = new List<GestureRecognizer>();

        public static void AddRecognizer(GestureRecognizer recognizer)
        {
            if (s_Recognizers.Contains(recognizer))
                return;

            s_Recognizers.Add(recognizer);
        }

        public static List<GestureRecognizer> GetRecognizers()
        {
            return s_Recognizers;
        }

        public static void RemoveRecognizer(GestureRecognizer recognizer)
        {
            if (!s_Recognizers.Contains(recognizer))
                return;
            s_Recognizers.Remove(recognizer);
        }

        public static void Update()
        {
            foreach(var recognizer in s_Recognizers)
            {
                recognizer.InternalUpdate();
            }
        }
    }
}