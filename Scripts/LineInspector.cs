/* -*- C# -*-------------------------------------------------------------
 *  CylindrLine.cs
 *  
 *  
 *  Copyright version 2.0 (2018/12) Chiang Yuan
 *  
 *      v_2.0   |   add Input.TouchCount & Input.GetTouch condition in 
 *                  Update()
 * ---------------------------------------------------------------------- */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.XR.iOS;

public class LineInspector : MonoBehaviour {

    public GameObject Line;
    public Plane Paper;

    public void Begin()
    {
        // Call TimedUpdate immediately, repeat every 0.01 seconds
        InvokeRepeating("Draw", 0f, 0.01f);
    }

    public void End()
    {
        // Call TimedUpdate immediately, repeat every 0.1 seconds
        CancelInvoke();
    }

    void Draw()
    {
        Debug.Log("You have clicked the button!");

        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || (Input.GetMouseButtonDown(0)))
        {
            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Ray, out hit))
            {
                Instantiate(Line, hit.point, Quaternion.identity, transform);
            }
        }
    }
}

