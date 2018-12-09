/* -*- C# -*-------------------------------------------------------------
 *  CylindrLine.cs
 *  
 *  
 *  Copyright version 3.1 (2018/12) Chiang Yuan
 *  
 *      v_3.1   |   add eraser button
 *      v_3.0   |   add Begin() and End() function
 *      v_2.0   |   add Input.TouchCount & Input.GetTouch condition in 
 *                  Update()
 * ---------------------------------------------------------------------- */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using UnityEditor;

enum ButtonFlag { eraser, pencil, deselect };

public class LineInspector : MonoBehaviour {

    public GameObject Line;
    
    private ButtonFlag buttonFlag;

    /* --------------------------------------------------
     * Public Function
     * -------------------------------------------------- */

    public void clickPencilButton()
    {
        buttonFlag = ButtonFlag.pencil;
        //CancelInvoke();

        //InvokeRepeating("Pencil", 0f, 0.01f);
    }
    public void clickEraserButton()
    {
        buttonFlag = ButtonFlag.eraser;
        //CancelInvoke();
        /*
        foreach (Transform iter in transform)
        {
            iter.GetComponent<CylinderLine>().setState("delete");
        }
        */
        //InvokeRepeating("Eraser", 0f, 0.01f);
    }
    public void clickDoneButton()
    {
        buttonFlag = ButtonFlag.deselect;
        //CancelInvoke();
        foreach (Transform iter in transform)
        {
            iter.GetComponent<CylinderLine>().setState("inactive");
        }
    }

    

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


    // Use this for initialization
    void Start()
    {
    }



    // Update is called once per frame
    void Update()
    {
        if (buttonFlag == ButtonFlag.pencil) Pencil();
        if (buttonFlag == ButtonFlag.eraser) Eraser();
    }

    /* --------------------------------------------------
    * Private Function
    * -------------------------------------------------- */

    private void Pencil()
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

    private void Eraser()
    {
        if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
            || (Input.GetMouseButton(0))))
        {
            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Ray, out hit))
            {
                hit.collider.GetComponent<CylinderLine>().setState("delete");
            }
        }
    }

}
