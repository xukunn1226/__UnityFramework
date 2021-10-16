using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Application.Runtime
{
    [RequireComponent(typeof(FPSCounter))]
    public class FPSDisplay : MonoBehaviour
    {
        static private string[] s_StringsFromNumber = {
            "00", "01", "02", "03", "04", "05", "06", "07", "08", "09",
            "10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
            "20", "21", "22", "23", "24", "25", "26", "27", "28", "29",
            "30", "31", "32", "33", "34", "35", "36", "37", "38", "39",
            "40", "41", "42", "43", "44", "45", "46", "47", "48", "49",
            "50", "51", "52", "53", "54", "55", "56", "57", "58", "59",
            "60", "61", "62", "63", "64", "65", "66", "67", "68", "69",
            "70", "71", "72", "73", "74", "75", "76", "77", "78", "79",
            "80", "81", "82", "83", "84", "85", "86", "87", "88", "89",
            "90", "91", "92", "93", "94", "95", "96", "97", "98", "99",
        };

        [System.Serializable]
        private struct FPSColor
        {
            public Color color;
            public int minimumFPS;
            public FPSColor(Color clr, int fps)
            {
                color = clr;
                minimumFPS = fps;
            }
        }

        private FPSCounter  m_Counter;
        public Text         m_FPSLabel;
        public Text         m_HighestFPSLabel;
        public Text         m_LowestFPSLabel;
        [SerializeField]
        private FPSColor[]  m_Colors = new FPSColor[] {};

        void Awake()
        {
            m_Counter = GetComponent<FPSCounter>();
        }

        void Update()
        {
            Display(m_FPSLabel,         m_Counter.FPS);
            Display(m_HighestFPSLabel,  m_Counter.HighestFPS);
            Display(m_LowestFPSLabel,   m_Counter.LowestFPS);
        }

        private void Display(Text label, int fps)
        {
            label.text = s_StringsFromNumber[Mathf.Clamp(fps, 0, 99)];
            for(int i = 0; i < m_Colors.Length; ++i)
            {
                if(fps >= m_Colors[i].minimumFPS)
                {
                    label.color = m_Colors[i].color;
                    break;
                }
            }
        }
    }
}