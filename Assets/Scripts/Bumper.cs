using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bumper {
    GameObject bumperObject;

    public Bumper(Vector3 pos, Color color)
    {
        bumperObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bumperObject.transform.position = pos;
        bumperObject.GetComponent<Renderer>().material.SetColor("_Color", color);
    }
}
