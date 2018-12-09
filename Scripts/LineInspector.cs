/* -*- C# -*-------------------------------------------------------------
 *  CylindrLine.cs
 *  
 *  
 *  Copyright version 3.1 (2018/12) Chiang Yuan & EricChiuBird
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

enum ButtonFlag { ERASER, PENCIL, DESELECT };

public class LineInspector : MonoBehaviour {

    public GameObject Line;
    
    private ButtonFlag buttonFlag;

    /* --------------------------------------------------
     * Public Function
     * -------------------------------------------------- */

    public void clickPencilButton()
    {
        buttonFlag = ButtonFlag.PENCIL;
        //CancelInvoke();

        //InvokeRepeating("Pencil", 0f, 0.01f);
    }
    public void clickEraserButton()
    {
        buttonFlag = ButtonFlag.ERASER;
        //CancelInvoke();
        /*
        foreach (Transform iter in transform)
        {
            iter.GetComponent<CylinderLine>().setState("DELETE");
        }
        */
        //InvokeRepeating("Eraser", 0f, 0.01f);
    }
    public void clickDoneButton()
    {
        buttonFlag = ButtonFlag.DESELECT;
        //CancelInvoke();
        foreach (Transform iter in transform)
        {
            iter.GetComponent<CylinderLine>().setState("INACTIVE");
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
        if (buttonFlag == ButtonFlag.PENCIL) Pencil();
        if (buttonFlag == ButtonFlag.ERASER) Eraser();
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
                hit.collider.GetComponent<CylinderLine>().setState("DELETE");
            }
        }
    }

}
