using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCursor : MonoBehaviour
{

    private Vector3 newPos;
    private float offset = 10.0f;

    // Update is called once per frame
    void Update()
    {
        newPos = Input.mousePosition;
        newPos.z = offset;
        transform.position = Camera.main.ScreenToWorldPoint(newPos);
    }
}
