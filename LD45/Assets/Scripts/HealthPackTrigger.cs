
using UnityEngine;

public class HealthPackTrigger : MonoBehaviour {
    public float healAmount = 50;
    private void OnTriggerEnter(Collider col) {
        if (col.tag == "Player")
        {
            if (col.gameObject.GetComponent<PlayerHealth>()){
                if(col.gameObject.GetComponent<PlayerHealth>().Heal(healAmount)) 
                {
                    FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Player/Pickup_Health", transform.position);
                    Destroy(gameObject);
                }
            }
        }
    }
}