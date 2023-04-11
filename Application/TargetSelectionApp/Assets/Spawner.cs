using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.IO;
using System.Text;

public class Spawner : MonoBehaviour
{
    public GameObject[] gameObjects;
    public GameObject targetPrefab;
    public GameObject targetSphere;
    public TMP_Text scoreText;
    public string[] clickLocations = new string[16];
    public string[] otherData = new string[3];
 

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
    private float deltaX;
    private float deltaY;
    private StreamWriter sw;
    private int index = 0;


    public TCPServer_V2 server;


    // Start is called before the first frame update
    void Start()
    {
        server = FindObjectOfType<TCPServer_V2>();
        Debug.Log("server: " + server.clickFlag);

        for (int i = 0; i < 16; i++)
        {
            clickLocations[i] = "test";
        }
        for (int i = 0; i < 3; i++)
        {
            otherData[i] = "test";
        }
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
            //sphereRenderer.sharedMaterial.color = Color.green;
        }
        sphereRenderer = gameObjects[0].GetComponent<MeshRenderer>();
        //sphereRenderer.material.SetColor("_Color", Color.red);
        sphereRenderer.sharedMaterial.color = Color.red;
        SphereCollider sphereCollider = gameObjects[0].GetComponent<SphereCollider>(); 
        Debug.Log("Radius: " + sphereCollider.radius);
        startTimer();
        deltaX = 0.00f;
        deltaY = 0.00f;
        scoreText.text = "";
        sw = new StreamWriter("ClickData.csv", false, Encoding.ASCII);
        sw.WriteLine("Click Location X,Click Location Y,Center Location X,Center Location Y");
    }

    public void updateDisplay(string toDisplay, bool gameOver)
    {
        if (gameOver)
        {
            scoreText.fontSize = 0.5f;
            scoreText.text = ("Game Over.\nAverage Miss Distance: (" + deltaX / 16.0f + ", " + deltaY / 16.0f + ")\nTotal Misses: " + missedHits);
            //missedHits = 0;
            sw.Close();
            Application.Quit();
        }
        else
        {
            scoreText.text = toDisplay;
        }
    }


    // Update is called once per frame
    void Update()
    {
        timerTime = Time.time - startTime;

        if (Input.GetMouseButtonDown(0))
        {

            Ray ray = GetComponent<Camera>().ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f));
            RaycastHit hit;
            Debug.Log("Mouse click position: (" + Input.mousePosition.x + "," + Input.mousePosition.y + ")");

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log($"{hit.collider.tag} Detected", hit.collider.gameObject);
                //changeSphereColor(hit.transform.gameObject);
                string curTag = "clone" + curHit;
                if (hit.collider.tag == curTag || hit.collider.tag == "Target")
                {
                    hitOnTarget++; //correct target hit
                    Transform clickedPos = hit.collider.GetComponent<Transform>();
                    Debug.Log("Before Conversion: " + clickedPos.position.x + "," + clickedPos.position.y);
                    Vector3 objInPix = Camera.main.WorldToScreenPoint(clickedPos.position);
                    Debug.Log("Hit Data: Mouse Clicked at: (" + Input.mousePosition.x + "," + Input.mousePosition.y + "), and Object Centered At: (" + objInPix.x + "," + objInPix.y + ")");
                    sw.WriteLine(Input.mousePosition.x + "," + Input.mousePosition.y + "," + objInPix.x + "," + objInPix.y);
                    Debug.Log("HitNumber " + index);
                    clickLocations[index] = Input.mousePosition.x + "," + Input.mousePosition.y + "," + objInPix.x + "," + objInPix.y;
                    index++;
                    deltaX += Mathf.Abs(Input.mousePosition.x - objInPix.x);
                    deltaY += Mathf.Abs(Input.mousePosition.y - objInPix.y);
                    if (curHit == 15)
                    {
                        
                        updateDisplay("Gameover", true);
                        timeElapsed = getTimeElapsed();
                        Debug.Log("Movement time: " + timeElapsed);
                        sw = new StreamWriter("MTandERData.csv", false, Encoding.ASCII);
                        sw.WriteLine(timeElapsed + ","+ missedHits + "," + hitOnTarget);
                        otherData[0] = timeElapsed + ",";
                        otherData[1] = missedHits + ",";
                        otherData[2] = hitOnTarget + ",";
                        server.SendClickData();
                        sw.Close();
                        missedHits = 0;

                        /**for(int i = 0; i < 16; i++)
                        {
                            Debug.Log(clickLocations[i]);
                        }*/
                        
                        //GameObject curTarget = GameObject.FindWithTag("Target");
                        targetSphere.SetActive(true);
                        MeshRenderer sphereRenderer = targetSphere.GetComponent<MeshRenderer>();
                        //sphereRenderer.material.SetColor("_Color", Color.red);
                        sphereRenderer.sharedMaterial.color = Color.red;
                        if (hit.collider.tag == "Target")
                        {
                            sphereRenderer = gameObjects[0].GetComponent<MeshRenderer>();
                            //sphereRenderer.material.SetColor("_Color", Color.red);
                            sphereRenderer.sharedMaterial.color = Color.red;
                            for (int i = 1; i < 16; i++)
                            {
                                sphereRenderer = gameObjects[i].GetComponent<MeshRenderer>();
                                //sphereRenderer.material.SetColor("_Color", Color.green);
                                sphereRenderer.sharedMaterial.color = Color.green;

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
                        updateDisplay("HIT!", false);
                        if (add)
                        {
                            MeshRenderer sphereRenderer = gameObjects[curHit + 8].GetComponent<MeshRenderer>();
                            //sphereRenderer.material.SetColor("_Color", Color.red);
                            sphereRenderer.sharedMaterial.color = Color.red;
                            sphereRenderer = gameObjects[curHit].GetComponent<MeshRenderer>();
                            //sphereRenderer.material.SetColor("_Color", Color.blue);
                            sphereRenderer.sharedMaterial.color = Color.blue;
                            startPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f));
                            startPos = new Vector3(startPos.x, startPos.y, 10.0f);
                            Transform nextTarget = gameObjects[curHit + 8].GetComponent<Transform>();
                            destPos = new Vector3(nextTarget.position.x, nextTarget.position.y, 10.0f);
                            pathDraw.SetPosition(1, startPos);
                            pathDraw.SetPosition(0, destPos);
                            Debug.Log(pathDraw.bounds);
                            curHit = curHit + 8;
                            add = false;

                        }
                        else
                        {
                            MeshRenderer sphereRenderer = gameObjects[curHit - 7].GetComponent<MeshRenderer>();
                            //sphereRenderer.material.SetColor("_Color", Color.red);
                            sphereRenderer.sharedMaterial.color = Color.red;
                            sphereRenderer = gameObjects[curHit].GetComponent<MeshRenderer>();
                            //sphereRenderer.material.SetColor("_Color", Color.blue);
                            sphereRenderer.sharedMaterial.color = Color.blue;
                            startPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f));
                            startPos = new Vector3(startPos.x, startPos.y, 10.0f);
                            Transform nextTarget = gameObjects[curHit - 7].GetComponent<Transform>();
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
                updateDisplay("MISS!", false);
            }
        }
        else if (server.clickFlag != 0)
        {
            Debug.Log("Getting click in Spawner");
            Vector3 positionBlah = new Vector3(server.currentX, server.currentY, 5.0f);
            Vector3 tempPos = Camera.main.WorldToScreenPoint(positionBlah);
            Ray ray = GetComponent<Camera>().ScreenPointToRay(new Vector3(tempPos.x, tempPos.y, 10.0f));
            RaycastHit hit;
            Debug.Log("Clicked Pos: (" + tempPos.x + "," + tempPos.y + ")");

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log($"{hit.collider.tag} Detected", hit.collider.gameObject);
                //changeSphereColor(hit.transform.gameObject);
                string curTag = "clone" + curHit;
                if (hit.collider.tag == curTag || hit.collider.tag == "Target")
                {
                    hitOnTarget++; //correct target hit
                    Transform clickedPos = hit.collider.GetComponent<Transform>();
                    Debug.Log("Before Conversion: " + clickedPos.position.x + "," + clickedPos.position.y);
                    Vector3 objInPix = Camera.main.WorldToScreenPoint(clickedPos.position);
                    Debug.Log("Hit Data: Mouse Clicked at: (" + server.currentX + "," + server.currentY + "), and Object Centered At: (" + objInPix.x + "," + objInPix.y + ")");
                    sw.WriteLine(tempPos.x + "," + tempPos.y + "," + objInPix.x + "," + objInPix.y);
                    Debug.Log("HitNumber " + index);
                    clickLocations[index] = tempPos.x + "," + tempPos.y + "," + objInPix.x + "," + objInPix.y;
                    index++;
                    deltaX += Mathf.Abs(tempPos.x - objInPix.x);
                    deltaY += Mathf.Abs(tempPos.y - objInPix.y);
                    if (curHit == 15)
                    {
                        updateDisplay("Gameover", true);
                        timeElapsed = getTimeElapsed();
                        Debug.Log("Movement time: " + timeElapsed);
                        sw = new StreamWriter("MTandERData.csv", false, Encoding.ASCII);
                        sw.WriteLine(timeElapsed + "," + missedHits + "," + hitOnTarget);
                        otherData[0] = timeElapsed + ",";
                        otherData[1] = missedHits + ",";
                        otherData[2] = hitOnTarget + ",";
                        sw.Close();
                        missedHits = 0;

                        /**for(int i = 0; i < 16; i++)
                        {
                            Debug.Log(clickLocations[i]);
                        }*/

                        //GameObject curTarget = GameObject.FindWithTag("Target");
                        targetSphere.SetActive(true);
                        MeshRenderer sphereRenderer = targetSphere.GetComponent<MeshRenderer>();
                        //sphereRenderer.material.SetColor("_Color", Color.red);
                        sphereRenderer.sharedMaterial.color = Color.red;
                        if (hit.collider.tag == "Target")
                        {
                            sphereRenderer = gameObjects[0].GetComponent<MeshRenderer>();
                            //sphereRenderer.material.SetColor("_Color", Color.red);
                            sphereRenderer.sharedMaterial.color = Color.red;
                            for (int i = 1; i < 16; i++)
                            {
                                sphereRenderer = gameObjects[i].GetComponent<MeshRenderer>();
                                //sphereRenderer.material.SetColor("_Color", Color.green);
                                sphereRenderer.sharedMaterial.color = Color.green;

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
                        updateDisplay("HIT!", false);
                        if (add)
                        {
                            MeshRenderer sphereRenderer = gameObjects[curHit + 8].GetComponent<MeshRenderer>();
                            //sphereRenderer.material.SetColor("_Color", Color.red);
                            sphereRenderer.sharedMaterial.color = Color.red;
                            sphereRenderer = gameObjects[curHit].GetComponent<MeshRenderer>();
                            //sphereRenderer.material.SetColor("_Color", Color.blue);
                            sphereRenderer.sharedMaterial.color = Color.blue;
                            startPos = Camera.main.ScreenToWorldPoint(new Vector3(tempPos.x, tempPos.y, 10.0f));
                            startPos = new Vector3(startPos.x, startPos.y, 10.0f);
                            Transform nextTarget = gameObjects[curHit + 8].GetComponent<Transform>();
                            destPos = new Vector3(nextTarget.position.x, nextTarget.position.y, 10.0f);
                            pathDraw.SetPosition(1, startPos);
                            pathDraw.SetPosition(0, destPos);
                            Debug.Log(pathDraw.bounds);
                            curHit = curHit + 8;
                            add = false;

                        }
                        else
                        {
                            MeshRenderer sphereRenderer = gameObjects[curHit - 7].GetComponent<MeshRenderer>();
                            //sphereRenderer.material.SetColor("_Color", Color.red);
                            sphereRenderer.sharedMaterial.color = Color.red;
                            sphereRenderer = gameObjects[curHit].GetComponent<MeshRenderer>();
                            //sphereRenderer.material.SetColor("_Color", Color.blue);
                            sphereRenderer.sharedMaterial.color = Color.blue;
                            startPos = Camera.main.ScreenToWorldPoint(new Vector3(tempPos.x, tempPos.y, 10.0f));
                            startPos = new Vector3(startPos.x, startPos.y, 10.0f);
                            Transform nextTarget = gameObjects[curHit - 7].GetComponent<Transform>();
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
                updateDisplay("MISS!", false);
            }
            server.clickFlag = 0;
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
        //Debug.Log("Radius: " + targetSphere.sphereCollider.radius);

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
