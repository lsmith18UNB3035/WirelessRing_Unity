using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class process : StandaloneInputModule
{ 
    public void Click()
    {
        GameObject Cursor = GameObject.FindWithTag("cursor");
        Debug.Log("cursor: " + Cursor);
        /*
        Input.simulateMouseWithTouches = true;
        var pointerData = GetTouchPointerEventData(new Touch()
        {
            position = new Vector2(x, y),
        }, out bool b, out bool bb);

        ProcessTouchPress(pointerData, true, true);
        */

    }

}
