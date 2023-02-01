using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    public GameObject targetPrefab;


    // Start is called before the first frame update
    void Start()
    {
        Transform mainCamTrans = Camera.main.GetComponent<Transform>();
        mainCamTrans.position = new Vector3(mainCamTrans.position.x, mainCamTrans.position.y, -12.0f);
        SpawnCircleFormation();
    }
  

    // Update is called once per frame
    void Update()
    {
  
    }


    private void SpawnCircleFormation()
    {

        Vector2 centerPosition = new Vector2(0, 0);
        float radius = 4.5f;
        float angle = 0.0f;

        for (int i = 0; i < 16; i++)
        {
            Vector2 spawnPosition;

            spawnPosition.x = (radius * Mathf.Cos(angle * Mathf.Deg2Rad)) + centerPosition.x;
            spawnPosition.y = (radius * Mathf.Sin(angle * Mathf.Deg2Rad)) + centerPosition.y;

            Instantiate(targetPrefab, spawnPosition, Quaternion.identity);
            angle += 22.5f;

        }

    }
}
