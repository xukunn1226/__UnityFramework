using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    private string m_PositionX;

    private Transform m_Host;

    void Start()
    {
        m_Host = GetComponent<Transform>();
    }

    void OnGUI()
    {
        GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
        btnStyle.fontSize = 30;

        m_PositionX = GUI.TextField(new Rect(100, 100, 200, 100), m_PositionX, btnStyle);
        if(GUI.Button(new Rect(350, 100, 200, 100), "Move", btnStyle))
        {
            if(int.TryParse(m_PositionX, out var posX))
            {
                m_Host.position = new Vector3(posX, 0, 0);
            }
        }

        if(GUI.Button(new Rect(100, 250, 200, 100), "0", btnStyle))
        {
            m_Host.position = new Vector3(0, 0, -10);            
        }

        if(GUI.Button(new Rect(100, 400, 200, 100), "20000", btnStyle))
        {
            m_Host.position = new Vector3(20000, 0, -10);            
        }

        if(GUI.Button(new Rect(100, 550, 200, 100), "50000", btnStyle))
        {
            m_Host.position = new Vector3(50000, 0, -10);            
        }

        if(GUI.Button(new Rect(100, 700, 200, 100), "100000", btnStyle))
        {
            m_Host.position = new Vector3(100000, 0, -10);            
        }
    }    
}
