using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beat {
    GameObject beatBall;
    private Rigidbody body;

    public Beat(Vector3 pos, Vector3 vel, Color color)
    {
        beatBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body = beatBall.AddComponent<Rigidbody>();
        //body = beatBall.GetComponent<Rigidbody>();
        beatBall.transform.position = pos;
        body.velocity = vel;
        beatBall.GetComponent<Renderer>().material.SetColor("_Color", color);
    }

    public GameObject getGameObject()
    {
        return beatBall;
    }
}
