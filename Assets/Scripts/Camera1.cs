using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

public class Camera1 : MonoBehaviour
{
    public Camera cam;
    public float fov = 60f;
    public Arm2 body;
    public Arm base1;
    public float bodyx;
    public float basex;
    public float midpoint;
    public float camx;
    public float distance;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //this script simply calculates the distance and midpoint between the joint locations of the bases as they move along

        bodyx = body.jointLocation.x;
        basex = base1.jointLocation.x;
        midpoint = (bodyx + basex) / 2;
        camx = cam.transform.position.x;
        distance = Mathf.Abs(bodyx - basex);

        //the x position of the camera is then set to the midpoint so it is always exactly between the two objects. 
        camx = midpoint;
        cam.transform.position = new Vector3(camx,0,-10f);

        //the fov is then changed to increase if they are further away and decrease if they're closer by a factor of 3* the distance
        //however if they are too close, the fov is capped to a minimum of 50 so it doesn't zoom in too much as they pass
        fov = 3f * distance;
        if (fov < 50)
        {
            fov = 51;
        }
        cam.fieldOfView = fov;

        


    }
}
