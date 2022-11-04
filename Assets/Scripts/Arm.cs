using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//Script used to control player 1
public class Arm : MonoBehaviour
{
    public Material material;

    public GameObject child;

    public GameObject control;
    private GameObject wrist;
    private GameObject upperArm;
    private GameObject lowerArm;
    private GameObject bottom;

    public Vector3 jointLocation;
    public Vector3 jointOffset;

    public float angle;

    public float lastAngle;

    public Vector3[] limbVertexLocations;

    public Mesh mesh;

    private int bobCount;
    private int fallCount;
    private int jumpTimer;
    private Vector3[] globalVert;

    private bool right = true;
    private bool jumpBool = false;
    private bool outsideRight = false;
    private bool outsideLeft = false;
    private bool fallBool = false;


    //method used to make the objects visible ie draw them
    private void DrawLimb()
    {

        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<MeshRenderer>();

        mesh = GetComponent<MeshFilter>().mesh;

        GetComponent<MeshRenderer>().material = material;

        mesh.Clear();
        //creates vertices using the vertex locations established in the editor
        mesh.vertices = new Vector3[] {
            new Vector3(limbVertexLocations[0].x, limbVertexLocations[0].y, limbVertexLocations[0].z),
            new Vector3(limbVertexLocations[1].x, limbVertexLocations[1].y, limbVertexLocations[1].z),
            new Vector3(limbVertexLocations[2].x, limbVertexLocations[2].y, limbVertexLocations[2].z),
            new Vector3(limbVertexLocations[3].x, limbVertexLocations[3].y, limbVertexLocations[3].z)
        };
        //gives the colours
        mesh.colors = new Color[] {
            new Color(0.8f, 0.3f, 0.3f, 1.0f),
            new Color(0.8f, 0.3f, 0.3f, 1.0f),
            new Color(0.8f, 0.3f, 0.3f, 1.0f),
            new Color(0.8f, 0.8f, 0.3f, 1.0f)
        };
        //triangles using vertices
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };


    }

    //this method simply allows the vertices to be moved by an offset
    public void MoveByOffset(Vector3 offset)
    {

        Matrix3x3 T = Translate(offset);

        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = T.MultiplyPoint(vertices[i]);
        }

        mesh.vertices = vertices;
        jointLocation = T.MultiplyPoint(jointLocation);

        if (child != null)
        {
            child.GetComponent<Arm>().MoveByOffset(offset);
        }
    }

    void Awake()
    {
        DrawLimb();
    }
    // Start is called before the first frame update
    void Start()
    {
        //some setup at the start of the scene
        // Ensuring the variables reference the correct game object
        Application.targetFrameRate = 60;
        wrist = GameObject.Find("Wrist");
        lowerArm = GameObject.Find("Lower arm");
        upperArm = GameObject.Find("Upper arm");
        bottom = GameObject.Find("Base");
        if (child != null)
        {
            child.GetComponent<Arm>().MoveByOffset(jointOffset);
        }

        globalVert = mesh.vertices;
    }

    //method to rotate the vertices 
    public void RotateAroundPoint(Vector3 point, float angle, float lastAngle)
    {

        Matrix3x3 T1 = Translate(-point);

        Matrix3x3 R1 = Rotate(-lastAngle);

        Matrix3x3 T2 = Translate(point);

        Matrix3x3 R2 = Rotate(angle);

        Matrix3x3 M = T2 * R2 * R1 * T1;

        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = M.MultiplyPoint(vertices[i]);
        }

        mesh.vertices = vertices;

        jointLocation = M.MultiplyPoint(jointLocation);

        if (child != null)
        {
            child.GetComponent<Arm>().RotateAroundPoint(point, angle, lastAngle);
        }
    }

    //the rotation matrix to assist in the previous method
    Matrix3x3 Rotate(float angle)
    {

        Matrix3x3 matrix = new Matrix3x3();
        matrix.SetRow(0, new Vector3(Mathf.Cos(angle), -Mathf.Sin(angle), 0.0f));
        matrix.SetRow(1, new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0.0f));
        matrix.SetRow(2, new Vector3(0.0f, 0.0f, 1.0f));

        return matrix;

    }

    //another matrix method, this time for translating
    public static Matrix3x3 Translate(Vector3 offset)
    {
        Matrix3x3 matrix = new Matrix3x3();

        matrix.SetRow(0, new Vector3(1.0f, 0.0f, offset.x));
        matrix.SetRow(1, new Vector3(0.0f, 1.0f, offset.y));
        matrix.SetRow(2, new Vector3(0.0f, 0.0f, 1.0f));

        return matrix;
    }


    //method used to make the objects jump
    void Jump()
    {
        //getting vertices and previous angle
        Vector3[] vertices = mesh.vertices;
        lastAngle = angle;
        //moves the base up
        if (control == wrist)
        {

            if (jumpTimer < 11)
            {
                MoveByOffset(new Vector3(0.0f, 0.1f, 0.0f));
                jumpTimer++;
            }
            if (jumpTimer > 10 && jumpTimer <= 21)
            {
                MoveByOffset(new Vector3(0.0f, -0.1f, 0.0f));
                jumpTimer++;
            }
            if (jumpTimer == 22)
            {
                jumpTimer = 0;
                jumpBool = false;
            }
        }
        //makes sure everything else follows
        if (child != null)
        {
            child.GetComponent<Arm>().RotateAroundPoint(jointLocation, angle, lastAngle);
        }

        mesh.RecalculateBounds();
    }

    //method to move everything
    void Move(float angle1, float angle2, float angle3, float direction)
    {
        //getting vertices and angle 
        Vector3[] vertices = mesh.vertices;
        lastAngle = angle;


        //these all check what the object is and controls it accordingly
        if (control == wrist)
        {
            if (bobCount < 15)
            {
                angle += angle1;
                bobCount++;
            }
            if (bobCount >= 15 && bobCount < 30)
            {
                angle -= angle1;
                bobCount++;
            }
            if (bobCount == 30)
            {
                bobCount = 0;
            }

            MoveByOffset(new Vector3(direction, 0.0f, 0.0f));
        }

        if (control == lowerArm)
        {
            if (bobCount < 15)
            {
                angle -= angle2;
                bobCount++;
            }
            if (bobCount >= 15 && bobCount < 30)
            {
                angle += angle2;
                bobCount++;
            }
            if (bobCount == 30)
            {
                bobCount = 0;
            }
        }

        if (control == upperArm)
        {
            if (bobCount < 15)
            {
                angle -= angle3;
                bobCount++;
            }
            if (bobCount >= 15 && bobCount < 30)
            {
                angle += angle3;
                bobCount++;
            }
            if (bobCount == 30)
            {
                bobCount = 0;
            }
        }
        //makes sure the child adjusts accordingly
        if (child != null)
        {
            child.GetComponent<Arm>().RotateAroundPoint(jointLocation, angle, lastAngle);
        }

        mesh.RecalculateBounds();
    }

    //the method used to make it play dead. works very similarly
    void fallOver(float angle1, float angle2, float angle3)
    {
        Vector3[] vertices = mesh.vertices;
        lastAngle = angle;

        if (control == wrist)
        {
            if (fallCount < 15)
            {
                angle += angle1;
                fallCount++;
            }
            if (fallCount > 14 && fallCount < 76)
            {
                fallCount++;
                return;
            }
            if (fallCount >= 76 && fallCount < 91)
            {
                angle -= angle1;
                fallCount++;
            }
            if (fallCount == 91)
            {
                fallCount = 0;
                fallBool = false;
            }

        }

        if (control == lowerArm)
        {
            if (fallCount < 15)
            {
                angle -= angle2;
                fallCount++;
            }
            if (fallCount > 14 && fallCount < 76)
            {
                fallCount++;
                return;
            }
            if (fallCount >= 76 && fallCount < 91)
            {
                angle += angle2;
                fallCount++;
            }
            if (fallCount == 91)
            {
                fallCount = 0;
                fallBool = false;
            }
        }

        if (control == upperArm)
        {
            if (fallCount < 15)
            {
                angle += angle3;
                fallCount++;
            }
            if (fallCount > 14 && fallCount < 76)
            {
                fallCount++;
                return;
            }
            if (fallCount >= 76 && fallCount < 91)
            {
                angle -= angle3;
                fallCount++;
            }
            if (fallCount == 91)
            {
                fallCount = 0;
                fallBool = false;
            }
        }

        if (child != null)
        {
            child.GetComponent<Arm>().RotateAroundPoint(jointLocation, angle, lastAngle);
        }

        mesh.RecalculateBounds();
    }

    // Update is called once per frame
    void Update()
    {
        //here the vertices are recalculated and checked if they're inside the boundary
        Vector3[] vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].x < -18)
            {
                outsideLeft = true;
            }
            else
            {
                outsideLeft = false;
            }
            if (vertices[i].x > 18)
            {
                outsideRight = true;
            }
            else
            {
                outsideRight = false;
            }
        }
        // the control to make it flop
        if (Input.GetKeyDown("x"))
        {
            if (child != null)
            {
                child.GetComponent<Arm>().RotateAroundPoint(jointLocation, 0.0f, lastAngle);
            }
            angle = 0.0f;
            fallCount = 0;
            fallBool = true;
        }
        //the general movements of the object the player has control over
        if (fallBool == false)
        {
            if (Input.GetKeyDown("d"))
            {
                if (child != null)
                {
                    child.GetComponent<Arm>().RotateAroundPoint(jointLocation, 0.0f, lastAngle);
                }
                angle = 0.0f;
                bobCount = 0;
                right = true;
            }

            if (Input.GetKeyDown("a"))
            {
                if (child != null)
                {
                    child.GetComponent<Arm>().RotateAroundPoint(jointLocation, 0.0f, lastAngle);
                }
                bobCount = 0;
                angle = 0.0f;
                right = false;
            }
            //the constant movement rate and angles given it's not outside the boundary
            if (right == true && outsideRight == false)
            {
                Move(0.1f, 0.17f, 0.1f, 0.1f);
            }
            if (right == false && outsideLeft == false)
            {
                Move(-0.1f, -0.17f, -0.1f, -0.1f);
            }

            //jump control
            if (Input.GetKeyDown("w"))
            {
                jumpBool = true;
            }
            if (jumpBool == true)
            {
                Jump();
            }
        }
        else if (fallBool == true)
        {
            //if the fall command is pushed it will be doing this instead, so can't be controlled
            if (right == true)
            {
                fallOver(0.1f, 0.1f, 0.06f);
            }
            else if (right == false)
            {
                fallOver(-0.1f, -0.1f, -0.06f);
            }
        }

    }
}