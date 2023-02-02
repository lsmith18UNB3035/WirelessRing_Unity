using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    Vector3 newPos;
    public float offset = 16.0f;

    // Update is called once per frame
    void Update()
    {
        newPos = Input.mousePosition;
        newPos.z = offset;
        transform.position = Camera.main.ScreenToWorldPoint(newPos);
    }
}
