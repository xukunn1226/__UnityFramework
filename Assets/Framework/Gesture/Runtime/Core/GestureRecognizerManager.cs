﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Gesture.Runtime
{
    internal static class GestureRecognizerManager
    {
        private static readonly List<GestureRecognizer> s_Recognizers = new List<GestureRecognizer>();

        internal static void AddRecognizer(GestureRecognizer recognizer)
        {
            if (s_Recognizers.Contains(recognizer))
                return;

            s_Recognizers.Add(recognizer);
            s_Recognizers.Sort((lhs, rhs) => {return lhs.Priority.CompareTo(rhs.Priority); });
        }

        public static List<GestureRecognizer> GetRecognizers()
        {
            return s_Recognizers;
        }

        internal static void RemoveRecognizer(GestureRecognizer recognizer)
        {
            if (!s_Recognizers.Contains(recognizer))
                return;
            s_Recognizers.Remove(recognizer);
        }

        internal static void Update()
        {
            foreach(var recognizer in s_Recognizers)
            {
                recognizer.InternalUpdate();
            }
        }
    }
}