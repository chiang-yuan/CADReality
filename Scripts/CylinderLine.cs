/* -*- C# -*-------------------------------------------------------------
 *  CylindrLine.cs
 *  
 *  
 *  Copyright version 2.0 (2018/12) Chiang Yuan
 *  
 *      v_2.0   |   add Input.TouchCount & Input.GetTouch condition in 
 *                  Update()
 *      v_2.1   |   some bugs fixed
 * ---------------------------------------------------------------------- */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CylinderLine : MonoBehaviour {
    
    private float SegmentHeight = 2.0f; // Cylindrical Height of LineSegment Prefabs
    private Vector3 posStr, posEnd;

    private bool drawFlag = true;
    private int touchCount = 0;

    /* --------------------------------------------------
     * Public Function
     * -------------------------------------------------- */

public Vector3 startPosition ()
    {
        return posStr;
    }

    public Vector3 endPosition()
    {
        return posEnd;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (drawFlag == true)
        {
            Draw();
        }
    }

    /* --------------------------------------------------
     * Private Function
     * -------------------------------------------------- */

    private void Draw()
    {
        
        if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            || (Input.GetMouseButton(0))) && touchCount == 0)
        {
            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Ray, out hit))
            {
                posStr = hit.point;
                posEnd = posStr;
                touchCount++;
            }
        }

        else if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            || (Input.GetMouseButton(0))))
        {
            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Ray, out hit))
            {
                posEnd = hit.point;
            }
        }

        if (posEnd != posStr)
        {
            var dir = posEnd - posStr;
            transform.position = posStr + dir / 2.0f;
            transform.localScale = new Vector3(transform.localScale.x, dir.magnitude / SegmentHeight, transform.localScale.z);
            transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        }

        if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
            || (Input.GetMouseButtonUp(0))))
        {
            drawFlag = false;
        }
    }
}
