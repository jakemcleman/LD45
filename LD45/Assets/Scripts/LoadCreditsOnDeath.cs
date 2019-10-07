using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


[RequireComponent(typeof(Health))]
public class LoadCreditsOnDeath : MonoBehaviour
{
    void Start()
    {
        Health healthComp = GetComponent<Health>();

        healthComp.onDeath.AddListener(OnDeath);
    }

    private void OnDeath() 
    {
        SceneManager.LoadScene("EndScreen");
    }
}
