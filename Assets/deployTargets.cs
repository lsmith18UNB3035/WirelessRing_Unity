using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class deployTargets : MonoBehaviour
{
    public GameObject targetPrefab;
    public float respawnTime = 10.0f;
    private Vector2 screenBounds;
    private int tally;
    private float deltaX;
    private float deltaY;
    private float timeElapsed;
    private int misses;
    private bool timerOn;
    private float timerTime;
    private float startTime;
    private GameObject curTarget;


    // Start is called before the first frame update
    void Start()
    {
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 3.0f));
        misses = 0;
        tally = 0;
        deltaX = 0.00f;
        deltaY = 0.00f;
        curTarget = GameObject.FindWithTag("Target");
        startTimer();
        //StartCoroutine(setUpSpawn());
    }

    private void spawnTarget()
    {
        GameObject init = Instantiate(targetPrefab) as GameObject;
        CharacterController cc = init.GetComponent<CharacterController>();
        Vector3 newPos = new Vector3(Random.Range(-screenBounds.x, screenBounds.x), Random.Range(-screenBounds.y, screenBounds.y), 1.5f);
        init.transform.position = newPos;
        curTarget = init;
        startTimer();
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
    private void resetTarget(bool success)
    {
        if (success)
        {
            timeElapsed += getTimeElapsed();
        }
        resetTimer();
        Destroy(curTarget);
        spawnTarget();
    }
    /**
    IEnumerator setUpSpawn()
    {
        while (true)
        {
            yield return new WaitForSeconds(respawnTime);
            spawnTarget();
        }
    }
    */
    void Update()
    {
        if (timerOn)
        {
            timerTime = Time.time - startTime;
        }
        if(getTimeElapsed() > 3.0f)
        {
            Debug.Log("MISS");
            misses += 1;
            resetTarget(false);
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Mouse Position: " + Input.mousePosition);
                Ray toMouse = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit rhInfo;
                bool didHit = Physics.Raycast(toMouse, out rhInfo, 500.0f);
                if (didHit)
                {
                    tally += 1;
                    if(tally < 10)
                    {
                        Transform clickedPos = rhInfo.collider.GetComponent<Transform>();
                        Vector3 objInPix = Camera.main.WorldToScreenPoint(clickedPos.position);

                        Debug.Log("Hit Data: Mouse Clicked at: (" + Input.mousePosition.x + "," + Input.mousePosition.y + "), and Object Centered At: (" + objInPix.x + "," + objInPix.y + ")");
                        deltaX += Mathf.Abs(Input.mousePosition.x - objInPix.x);
                        deltaY += Mathf.Abs(Input.mousePosition.y - objInPix.y);
                        TargetControl objectScript = rhInfo.collider.GetComponent<TargetControl>();
                        resetTarget(true);
                    }
                    else
                    {
                        Debug.Log("Game Over. Total Misses: " + misses + ", Average miss x: " + deltaX/10 + ", average miss y: " + deltaY/10 + ", average time to click: " + timeElapsed/10.0f);
                    }
                
                }
                else
                {
                    Debug.Log("MISS");
                    misses += 1;
                    resetTarget(false);
                }
            } 
        }
    }
}
