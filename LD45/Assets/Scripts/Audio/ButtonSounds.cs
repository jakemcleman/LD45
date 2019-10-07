using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSounds : MonoBehaviour
{
    public void ButtonClickSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/UI/UI_ButtonClick", transform.position);
    }
}
