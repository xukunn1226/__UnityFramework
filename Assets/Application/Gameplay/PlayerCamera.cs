using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform        mainCamera;
    private Vector3         m_DragVelocity;
    public float            SmoothTime;

    void Start()
    {
        if(GamePlayerInput.Instance == null)
            throw new System.Exception("GamePlayerInput.Instance == null");
        
        if(mainCamera == null)
            throw new System.Exception("mainCamera == null");
    }

    void LateUpdate()
    {
        if(GamePlayerInput.Instance.isDragging)
        {
            Vector3 delta = GamePlayerInput.Instance.dragEndPoint - GamePlayerInput.Instance.dragStartPoint;
            Debug.Log($"{delta}");
            mainCamera.transform.position = Vector3.SmoothDamp(mainCamera.transform.position, mainCamera.transform.position - delta, ref m_DragVelocity, SmoothTime);
            // mainCamera.transform.position += delta;

            // delta = Vector3.SmoothDamp(Vector3.zero, delta * -1, ref m_DragVelocity, 0.1f);
        }        
    }
}
