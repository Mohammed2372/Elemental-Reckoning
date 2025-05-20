using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class moon_light : MonoBehaviour
{
    [SerializeField] private GameObject follow;
    // Start is called before the first frame update
    void Start()
    {
        
    }


    void FixedUpdate()
    {
        if (follow.transform.position.y < 4.9f)
        {
            transform.position = new Vector3(follow.transform.position.x - 2, transform.position.y, transform.position.z);
        }
        else
        {
            transform.position = new Vector3(follow.transform.position.x - 2, follow.transform.position.y + 7, transform.position.z);
        }
    }
}
