using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject[] gameObjects;
    public GameObject targetPrefab;
    public GameObject targetSphere;
    
    private int curHit = 0;
    private bool add = true;
    private int missedHits = 0;
    private int hitOnTarget = 0;
    private float timeElapsed;
    private bool timerOn;
    private float timerTime;
    private float startTime;
    private Vector3 startPos;
    private Vector3 destPos;
    private Vector3 idealTravel;
    private LineRenderer pathDraw;


    // Start is called before the first frame update
    void Start()
    {
        GameObject tmpPathDraw = GameObject.FindWithTag("PathDrawer");
        pathDraw = tmpPathDraw.GetComponent<LineRenderer>();
        Debug.Log(pathDraw);
        Transform mainCamTrans = Camera.main.GetComponent<Transform>();
        //mainCamTrans.position = new Vector3(mainCamTrans.position.x, mainCamTrans.position.y, -12.0f);
        SpawnCircleFormation();
        MeshRenderer sphereRenderer;
        for (int i = 0; i < 16; i++)
        {
            sphereRenderer = gameObjects[i].GetComponent<MeshRenderer>();
            sphereRenderer.material.SetColor("_Color", Color.green);
        }
        sphereRenderer = gameObjects[0].GetComponent<MeshRenderer>();
        sphereRenderer.material.SetColor("_Color", Color.red);
        startTimer();
    }


    // Update is called once per frame
    void Update()
    {
        timerTime = Time.time - startTime;

        if (Input.GetMouseButtonDown(0))
        {

            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log($"{hit.collider.tag} Detected", hit.collider.gameObject);
                //changeSphereColor(hit.transform.gameObject);
                string curTag = "clone" + curHit;
                if (hit.collider.tag == curTag || hit.collider.tag == "Target")
                {
                    hitOnTarget++; //correct target hit
                    if(curHit == 15)
                    {
                        timeElapsed = getTimeElapsed();
                        Debug.Log("Movement time: " + timeElapsed);

                        //GameObject curTarget = GameObject.FindWithTag("Target");
                        targetSphere.SetActive(true);
                        MeshRenderer sphereRenderer = targetSphere.GetComponent<MeshRenderer>();
                        sphereRenderer.material.SetColor("_Color", Color.red);
                        if (hit.collider.tag == "Target")
                        {
                            sphereRenderer = gameObjects[0].GetComponent<MeshRenderer>();
                            sphereRenderer.material.SetColor("_Color", Color.red);
                            for(int i = 1; i < 16; i++)
                            {
                                sphereRenderer = gameObjects[i].GetComponent<MeshRenderer>();
                                sphereRenderer.material.SetColor("_Color", Color.green);
                            }
                            targetSphere.SetActive(false);
                            curHit = 0;
                            add = true;
                            resetTimer();
                            startTimer();
                        }
                       
                    }
                    else
                    {
                        if (add)
                        {
                            MeshRenderer sphereRenderer = gameObjects[curHit + 8].GetComponent<MeshRenderer>();
                            sphereRenderer.material.SetColor("_Color", Color.red);
                            sphereRenderer = gameObjects[curHit].GetComponent<MeshRenderer>();
                            sphereRenderer.material.SetColor("_Color", Color.blue);
                            Debug.Log("Before camera: " + Input.mousePosition);
                            startPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f));
                            Debug.Log("Before change: " + startPos);
                            startPos = new Vector3(startPos.x, startPos.y, 10.0f);
                            Debug.Log("AfterChange: " + startPos);
                            Transform nextTarget = gameObjects[curHit + 8].GetComponent<Transform>();
                            Debug.Log(nextTarget.position);
                            destPos = new Vector3(nextTarget.position.x, nextTarget.position.y, 10.0f);
                            pathDraw.SetPosition(1, startPos);
                            pathDraw.SetPosition(0, destPos);
                            curHit = curHit + 8;
                            add = false;

                        }
                        else
                        {
                            MeshRenderer sphereRenderer = gameObjects[curHit - 7].GetComponent<MeshRenderer>();
                            sphereRenderer.material.SetColor("_Color", Color.red);
                            sphereRenderer = gameObjects[curHit].GetComponent<MeshRenderer>();
                            sphereRenderer.material.SetColor("_Color", Color.blue);
                            Debug.Log("Before camera: " + Input.mousePosition);
                            startPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f));
                            Debug.Log("Before Change: " + startPos);
                            startPos = new Vector3(startPos.x, startPos.y, 10.0f);
                            Debug.Log("After Change: " + startPos);
                            Transform nextTarget = gameObjects[curHit - 7].GetComponent<Transform>();
                            Debug.Log(nextTarget.position);
                            destPos = new Vector3(nextTarget.position.x, nextTarget.position.y, 10.0f);
                            pathDraw.SetPosition(1, startPos);
                            pathDraw.SetPosition(0, destPos);
                            curHit = curHit - 7;
                            add = true;
                        }
                    }
                    
                    
                }
                else
                {
                    missedHits++; //wrong target hit
                }

            }
            else
            {
                missedHits++; //no target hit
            }
        }
    }



    private void SpawnCircleFormation()
    {
        Vector2 centerPosition = new Vector2(0, 0);
        float radius = 4.5f;
        float angle = 0.0f;

        gameObjects = new GameObject[16];
        for (int i = 0; i < 16; i++)
        {
            Vector3 spawnPosition = new Vector3(((radius * Mathf.Cos(angle * Mathf.Deg2Rad)) + centerPosition.x), ((radius * Mathf.Sin(angle * Mathf.Deg2Rad)) + centerPosition.y), 10.0f);


            GameObject init = Instantiate(targetPrefab, spawnPosition, Quaternion.identity);
            init.tag = "clone" + i;
            gameObjects[i] = init;
            angle += 22.5f;

        }
        targetSphere = GameObject.FindWithTag("Target");
        targetSphere.SetActive(false);

    }

    private void startTimer()
    {
        if (!(timerOn))
        {
            timerOn = true;
            startTime = Time.time;
        }
    }

    private void resetTimer()
    {
        timerTime = 0.0f;
        startTime = 0.0f;
        timerOn = false;

    }

    private float getTimeElapsed()
    {
        return (timerTime % 60.0f);
    }

}
