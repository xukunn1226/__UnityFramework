using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Application.HotFix
{
    public class TestEventManager : MonoBehaviour
    {
        public TestPlayer m_Player;
        private WeakReference m_weakRef;

        void Start()
        {
        }

        void OnGUI()
        {
            if(GUI.Button(new Rect(100, 100, 200, 60), "Add Listener"))
            {
                m_Player.AddListener();
            }
            if(GUI.Button(new Rect(100, 300, 200, 60), "Dispatch"))
            {
                EventManager.Allocate<EventArgs_HP>().Set(HPEvent.HPChange, 2).Dispatch();
            }
            if(GUI.Button(new Rect(100, 500, 200, 60), "Remove Listener"))
            {
                m_Player.RemoveListener();
            }

            if(GUI.Button(new Rect(100, 700, 200, 60), "Test WeakReference"))
            {
                int cacheSize = 50;
                System.Random r = new System.Random();
                Cache c = new Cache(cacheSize);

                string DataName = "";
                GC.Collect();

                // Randomly access objects in the cache.
                for (int i = 0; i < c.Count; i++)
                {
                    int index = r.Next(c.Count);

                    // Access the object by getting a property value.
                    DataName = c[index].Name;
                }
                // Show results.
                double regenPercent = c.RegenerationCount / (double)c.Count;
                Debug.Log($"Cache size: {c.Count}, Regenerated: {regenPercent:P2}%");
            }
        }
    }

    
    public class Cache
    {
        // Dictionary to contain the cache.
        static Dictionary<int, WeakReference> _cache;

        // Track the number of times an object is regenerated.
        int regenCount = 0;

        public Cache(int count)
        {
            _cache = new Dictionary<int, WeakReference>();

            // Add objects with a short weak reference to the cache.
            for (int i = 0; i < count; i++)
            {
                _cache.Add(i, new WeakReference(new Data(i), false));
            }
        }

        // Number of items in the cache.
        public int Count
        {
            get { return _cache.Count; }
        }

        // Number of times an object needs to be regenerated.
        public int RegenerationCount
        {
            get { return regenCount; }
        }

        // Retrieve a data object from the cache.
        public Data this[int index]
        {
            get
            {
                Data d = _cache[index].Target as Data;
                if (d == null)
                {
                    // If the object was reclaimed, generate a new one.
                    Debug.Log($"Regenerate object at {index}: Yes");
                    d = new Data(index);
                    _cache[index].Target = d;
                    regenCount++;
                }
                else
                {
                    // Object was obtained with the weak reference.
                    Debug.Log($"Regenerate object at {index}: No");
                }

                return d;
            }
        }
    }

    // This class creates byte arrays to simulate data.
    public class Data
    {
        private byte[] _data;
        private string _name;

        public Data(int size)
        {
            _data = new byte[size * 1024];
            _name = size.ToString();
        }

        // Simple property.
        public string Name
        {
            get { return _name; }
        }
    }
}