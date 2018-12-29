/* -*- C# -*-------------------------------------------------------------
 *  PolygonInspector.cs
 *  
 *  
 *  Copyright version 1.0 (2018/12) Chiang Yuan & EricChiuBird
 *  
 *      v_1.0   |   build this script
 *      
 * ---------------------------------------------------------------------- */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
//using UnityEngine.XR.iOS;
using UnityEngine.UI;

//enum ButtonFlag { ERASER, PENCIL, DESELECT, DRAWRED, DRAWBLUE, DRAWGREEN, POLYGON };

public class PolygonInspector : MonoBehaviour
{
    public GameObject Polygon;
    public GameObject Circle;

    private bool drawone = false;
    private ButtonFlag buttonFlag = ButtonFlag.DESELECT;

    /* --------------------------------------------------
     * Public Function
     * -------------------------------------------------- */

    public void clickPolygonButton()
    {
        buttonFlag = ButtonFlag.POLYGON;
    }

    public void clickCircleButton()
    {
        buttonFlag = ButtonFlag.CIRCLE;
    }
   
    public void clickEraserButton()
    {
        buttonFlag = ButtonFlag.ERASER;
    }
    public void clickDoneButton()
    {
        buttonFlag = ButtonFlag.DESELECT;
        foreach (Transform iter in transform)
        {
            iter.GetComponent<QuadPolygon>().setState("INACTIVE");
        }
        drawone = false;
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (buttonFlag == ButtonFlag.POLYGON && drawone == false) CreatePolygon();
        if (buttonFlag == ButtonFlag.CIRCLE) CreateCircle();
        if (buttonFlag == ButtonFlag.ERASER) Eraser();
    }

    /* --------------------------------------------------
    * Private Function
    * -------------------------------------------------- */

    private void CreatePolygon()
    {
        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            || (Input.GetMouseButtonDown(0)))
        {
            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Ray, out hit))
            {
                //GameObject newPolygon = Instantiate(Polygon, transform);
                Instantiate(Polygon, transform);
                //newPolygon.transform.tag = "QuadPolygon";
                drawone = true;
            }
        }
    }

    private void CreateCircle()
    {
        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            || (Input.GetMouseButtonDown(0)))
        {
            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Ray, out hit))
            {
                GameObject newPolygon = Instantiate(Circle, transform);
                newPolygon.transform.tag = "QuadCircle";
                drawone = true;
            }
        }
    }

    private void Eraser()
    {
        if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            || (Input.GetMouseButton(0))))
        {
            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Ray, out hit))
            {
                hit.collider.GetComponent<QuadPolygon>().setState("DELETE");
            }
        }
    }
}
