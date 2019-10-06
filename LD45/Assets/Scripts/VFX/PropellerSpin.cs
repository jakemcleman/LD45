using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerSpin : MonoBehaviour
{
    public float spinRate = 360;

    void Update()
    {
        transform.Rotate(Vector3.up, spinRate * Time.deltaTime);
    }
}
