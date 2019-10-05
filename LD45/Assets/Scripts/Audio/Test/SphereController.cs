using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereController : MonoBehaviour
{
    private float speed = 4.0f;

    private bool goRight = true;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (goRight)
            transform.Translate(Vector3.right * Time.deltaTime * speed);
        else
            transform.Translate(Vector3.left * Time.deltaTime * speed);

        if (transform.position.x > -3.0f)
            goRight = false;
        if (transform.position.x < -17.0f)
            goRight = true;
    }
}
