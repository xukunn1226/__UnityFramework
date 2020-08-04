using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Framework.Gesture.Runtime;

public class TestGesture : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSelect(BaseEventData eventData)
    {
        transform.localScale *= 2;

        PlayerInput.HitEventData data = eventData as PlayerInput.HitEventData;
        if(data != null)
        {
            Debug.Log($"HitInfo: {data.hitInfo.point}");
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        transform.localScale *= 0.5f;
    }
}
