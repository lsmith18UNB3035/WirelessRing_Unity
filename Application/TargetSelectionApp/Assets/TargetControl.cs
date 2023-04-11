using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TargetControl : MonoBehaviour
{
    public Vector3 newPos;
    private Vector3 screenSize;
    private Canvas bounds;
    // Start is called before the first frame update
    void Start()
    {

        bounds = GameObject.FindGameObjectWithTag("Bounds").GetComponent<Canvas>();
        RectTransform rectTransform = bounds.GetComponent<RectTransform>();
        screenSize = Camera.main.ScreenToWorldPoint(new Vector3(rectTransform.position.x, rectTransform.position.y, rectTransform.position.z));
        newPos = new Vector3(Random.Range(-screenSize.x, screenSize.x), Random.Range(-screenSize.y, screenSize.y), 10.0f);
        transform.position = newPos;
    }

    public void DestroyMe()
    {
        Destroy (gameObject);
    }
}
