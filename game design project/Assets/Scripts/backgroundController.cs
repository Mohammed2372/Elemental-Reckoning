using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class backgroundController : MonoBehaviour
{
    // Start is called before the first frame update
    private float startPos, len;
    public GameObject cam;
    public float parallaxEffect = 0.2f;
    void Start()
    {
        startPos = transform.position.x;
        len = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float distance = cam.transform.position.x * parallaxEffect;
        float move = cam.transform.position.x * (1 - parallaxEffect);


        transform.position = new Vector3(startPos + distance, cam.transform.position.y, transform.position.z);


        if (move > startPos + len)
        {
            startPos += len;
        }
        else if (move < startPos - len)
        {
            startPos -= len;
        }


    }
}
