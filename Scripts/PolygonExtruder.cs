using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using UnityEngine.UI;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PolygonExtruder : MonoBehaviour
{
    public GameObject meshExtrusion;
    public Material Wood;
    public Material Concrete;
    public Material Carbon;
    public Material Brick;
    public Material Metal;
    public GenerateAnchorPlane anchorplane;

    Material MaterialChange;

    private bool editone = false;
    private ButtonFlag buttonFlag = ButtonFlag.DESELECT;
    private MaterialFlag materialflag;

    public void ClickConcreteButton()
    {
        materialflag = MaterialFlag.CONCRETE;
    }

    public void ClickWoodButton()
    {
        materialflag = MaterialFlag.WOOD;
    }

    public void ClickCarbonButton()
    {
        materialflag = MaterialFlag.CARBON;
    }

    public void ClickBrickButton()
    {
        materialflag = MaterialFlag.BRICK;
    }

    public void ClickMetalButton()
    {

        materialflag = MaterialFlag.METAL;
    }
    public void clickExtrudeButton()
    {
        buttonFlag = ButtonFlag.EXTRUDE;
        foreach (Transform iter in transform)
        {
            iter.GetComponent<MeshExtrude>().setState("ACTIVE");
        }
    }

    public void clickDoneButton()
    {
        buttonFlag = ButtonFlag.DESELECT;
        foreach (Transform iter in transform)
        {
            iter.GetComponent<MeshExtrude>().setState("INACTIVE");
        }
    }

    public void clickEraserButton()
    {
        buttonFlag = ButtonFlag.ERASER;

    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (buttonFlag == ButtonFlag.EXTRUDE && editone == false) Extrude(MaterialChange);
        if (buttonFlag == ButtonFlag.DESELECT) editone = false;
        if (materialflag == MaterialFlag.WOOD) MaterialChange = Wood;
        if (materialflag == MaterialFlag.CARBON) MaterialChange = Carbon;
        if (materialflag == MaterialFlag.BRICK) MaterialChange = Brick;
        if (materialflag == MaterialFlag.METAL) MaterialChange = Metal;
        if (materialflag == MaterialFlag.CONCRETE) MaterialChange = Concrete;
        if (buttonFlag == ButtonFlag.ERASER) Eraser();

        if (FindObjectsOfType<MeshExtrude>() != null && anchorplane.isUpdate())
        {
            Debug.Log("Select Lines");

            foreach (MeshExtrude gameObj in FindObjectsOfType<MeshExtrude>())
            {
                Debug.Log("Transforming Lines");
                gameObj.transform.position += anchorplane.getPosition();
                gameObj.transform.rotation = anchorplane.getQuaternion();
            }
        }
    }


    /* --------------------------------------------------
     *  Private Function
     * -------------------------------------------------- */

    private void Extrude(Material material)
    {
        if (((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            || (Input.GetMouseButton(0))) && buttonFlag == ButtonFlag.EXTRUDE)
        {
            var Ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(Ray, out hit)
                && (hit.collider.gameObject.name == "QuadPolygon" || hit.collider.gameObject.name == "QuadCircle"|| hit.collider.gameObject.name == "CylinderLine"))
            {
                GameObject newExtrusion = Instantiate(meshExtrusion, transform);
                newExtrusion.GetComponent<Renderer>().material = material;
                editone = true;
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
                hit.collider.GetComponent<MeshExtrude>().setState("DELETE");
            }
        }
    }
}