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
        endPosition = this.transform.position;
        this.transform.position = poofFromVector;

        this.gameObject.SetActive(false);
    }

    public void StartPoofIn()
    {
        StartCoroutine("PoofIn");
    }

    IEnumerator PoofIn()
    {
        animationTime = 0f;

        yield return new WaitForSeconds(delay);

        while (animationTime <= poofDuration)
        {
            animationTime += Time.deltaTime;
            float percent = Mathf.Clamp01(animationTime / poofDuration);

            transform.position = Vector3.LerpUnclamped(poofFromVector, endPosition, moveCurve.Evaluate(percent));

            yield return null;
        }
    }
}
