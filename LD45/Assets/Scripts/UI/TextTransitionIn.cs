using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextTransitionIn : MonoBehaviour
{
    private enum AnimState
    {
        WaitingToBlink,
        WaitingToStart,
        TypingText,
        Complete
    }

    public TextTransitionIn waitFor;
    public float pauseBeforeTypingLength = 3.0f;
    public float cursorBlinkRate = 1.0f;
    public float typeRate = 0.1f;

    private Text textObject;
    private string fullText;
    private float blinkTimer;
    private float stateTimer;
    private AnimState curState;
    private bool shouldBlink;
    private bool blinkOn;
    private int curStringPos;
    public bool animate = true;

    public float spaceAccelFactor = 10;

    private void Start()
    {
        textObject = GetComponent<Text>();
        fullText = textObject.text;
        textObject.text = "";

        blinkTimer = 0;
        stateTimer = 0;
        curStringPos = 0;
        shouldBlink = true;
        blinkOn = false;
        if(waitFor != null) curState = AnimState.WaitingToBlink;
        else curState = AnimState.WaitingToStart;
    }

    private void DoCursorBlink()
    {
        float speedFactor = Input.GetButton("Jump") ? spaceAccelFactor : 1;
        blinkTimer += Time.deltaTime * speedFactor;

        if(shouldBlink && blinkTimer > cursorBlinkRate)
        {
            blinkTimer = 0;
            SetBlinkness(!blinkOn);
        }
    }

    private void SetBlinkness(bool state)
    {
        textObject.text = textObject.text.Substring(0, curStringPos) + (state ? "█" : " ");
        blinkOn = state;
    }

    private void Update()
    {
        if(Input.GetButtonDown("Fire1"))
        {
            textObject.text = fullText;
            stateTimer = 0;
            curState = AnimState.Complete;
            shouldBlink = false;
        }

        float speedFactor = Input.GetButton("Jump") ? spaceAccelFactor : 1;

        if(animate)
        {
            switch(curState)
            {
                case AnimState.WaitingToBlink:
                    SetBlinkness(false);
                    shouldBlink = false;
                    if(waitFor.curState == AnimState.Complete)
                    {
                        stateTimer = 0;
                        curState = AnimState.WaitingToStart;
                        shouldBlink = true;
                    }
                    break;
                case AnimState.WaitingToStart:
                    stateTimer += Time.deltaTime * speedFactor;
                    DoCursorBlink();
                    if(speedFactor > 2 || stateTimer > pauseBeforeTypingLength )
                    {
                        stateTimer = 0;
                        curState = AnimState.TypingText;
                    }
                    break;
                case AnimState.TypingText:
                    stateTimer += Time.deltaTime * speedFactor;
                    while(stateTimer > typeRate)
                    {
                        curStringPos++;
                        SetBlinkness(true);
                        if(curStringPos >= fullText.Length)
                        {
                            textObject.text = fullText;
                            stateTimer = 0;
                            curState = AnimState.Complete;
                            shouldBlink = false;
                        }
                        else
                        {
                            stateTimer = 0;
                            textObject.text = fullText.Substring(0, curStringPos) + "█";
                            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/UI/Keystroke", transform.position);
                        }
                    }
                    break;
            }
        }
    }
}
