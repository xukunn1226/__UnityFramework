using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderController : MonoBehaviour
{
    [SerializeField]
    private float m_velocity = 6.0f;

    [SerializeField]
    private Vector3[] m_destinations;

    [SerializeField]
    private Transform[] m_predefineDest;

    [SerializeField]
    private Vector3 m_movementBounds = new Vector3(30, 30, 30);

    private int m_currentDestinationIndex;
    // Start is called before the first frame update
    void Start()
    {
        m_destinations = new Vector3[m_predefineDest.Length];
        for (int i = 0; i < m_destinations.Length; i++)
        {
            m_destinations[i] = m_predefineDest[i].position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(m_destinations[m_currentDestinationIndex], transform.position) > 3f)
        {
            var dir = (m_destinations[m_currentDestinationIndex] - transform.position).normalized;
            transform.position += dir * m_velocity * Time.deltaTime;
            transform.localRotation = Quaternion.LookRotation(dir, transform.up);
        }
        else
        {
            m_currentDestinationIndex++;
            if (m_currentDestinationIndex >= m_destinations.Length)
            {
                m_currentDestinationIndex = 0;
            }
        }
    }
}
