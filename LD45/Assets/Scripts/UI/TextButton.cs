using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextButton : MonoBehaviour
{
    private Text textObject;
    private string fullText;

    public Font normalFont;
    public Font selectedFont;

    private void Start()
    {
        textObject = GetComponent<Text>();
        fullText = textObject.text;
        OnDeselect();
    }
    public void OnSelect()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/UI/UI_ButtonHover", transform.position);
        textObject.text = ">>" + fullText;
        textObject.font = selectedFont;
    }

    public void OnDeselect()
    {
        textObject.text = "  " + fullText;
        textObject.font = normalFont;
    }
}
