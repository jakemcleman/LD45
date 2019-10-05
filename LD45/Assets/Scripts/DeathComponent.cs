using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class DeathComponent : MonoBehaviour
{
    void Start()
    {
        Health healthComp = GetComponent<Health>();
    }

}
