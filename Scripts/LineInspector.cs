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
using UnityEngine.Windows;
using UnityEditor;

public class LineInspector : MonoBehaviour {

    public GameObject Line;
    public Plane Paper;
    
    // Use this for initialization
    void Start()
    {
        Paper = gameObject.GetComponent<Plane>();
    }

    // Update is called once per frame
    void Update()
    {
        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            || (Input.GetMouseButtonDown(0)))
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
