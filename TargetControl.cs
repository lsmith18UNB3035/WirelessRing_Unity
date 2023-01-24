using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetControl : MonoBehaviour
{
    public Vector3 newPos;
    private Vector2 screenSize;
    // Start is called before the first frame update
    void Start()
    {
        screenSize = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 3.0f));
        newPos = new Vector3(Random.Range(-screenSize.x, screenSize.x), Random.Range(-screenSize.y, screenSize.y), 1.5f);
        transform.position = newPos;
    }

    public void DestroyMe()
    {
        Destroy (gameObject);
    }
}
