using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoofer : MonoBehaviour
{
    public Vector3 poofFromVector;
    private Vector3 endPosition;

    public AnimationCurve moveCurve;

    public float poofDuration = 1;
    public float delay = 0;
    private float animationTime;

    // Start is called before the first frame update
    void Start()
    {
        endPosition = this.transform.localPosition;
        this.transform.localPosition = poofFromVector;

        this.gameObject.SetActive(false);
    }

    public void StartPoofIn()
    {
        StartCoroutine("PoofIn");
    }

    public void StartPoofOut()
    {
        StartCoroutine("PoofOut");
    }

    IEnumerator PoofIn()
    {
        animationTime = 0f;

        yield return new WaitForSeconds(delay);

        while (animationTime <= poofDuration)
        {
            animationTime += Time.deltaTime;
            float percent = Mathf.Clamp01(animationTime / poofDuration);

            transform.localPosition = Vector3.LerpUnclamped(poofFromVector, endPosition, moveCurve.Evaluate(percent));

            yield return null;
        }
    }

    IEnumerator PoofOut()
    {
        animationTime = 0f;

        while (this.gameObject.GetComponentInChildren<Checkpoint>() != null && this.gameObject.GetComponentInChildren<Checkpoint>().getActive() == true)
        {
            Debug.Log("Can't poof out yet, there's an active checkpoint");
            yield return new WaitForSeconds(1.0f);
        }

        yield return new WaitForSeconds(delay);

        Debug.Log("Starting actual depoof");

        while (animationTime <= poofDuration)
        {
            animationTime += Time.deltaTime;
            float percent = 1 - Mathf.Clamp01(animationTime / poofDuration);

            transform.localPosition = Vector3.LerpUnclamped(poofFromVector, endPosition, moveCurve.Evaluate(percent));

            yield return null;
        }

        this.gameObject.SetActive(false);
    }
}
