using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class HitEffect : MonoBehaviour
{
    public float FlashDuration = 0.2f;
    private bool isFlashing;
    private Health health;

    private float flashTimer;

    public MeshRenderer[] toFlash;
    private Color[][] initialColors;
    private Color[][] initialEmission;
    private bool[][] initialEmissionOn;

    private Vector3 shakeBasePos;
    private Vector3 shakeDir;

    public float ShakeMagnitude = 0.1f;

    public Color flashColor = new Color(1.0f, 0.25f, 0.25f, 0.5f);

    private void Start()
    {
        health = GetComponent<Health>();
        health.onHealthChange.AddListener(OnHealthChange);
        isFlashing = false;

        initialColors = new Color[toFlash.Length][];
        initialEmission = new Color[toFlash.Length][];
        initialEmissionOn = new bool[toFlash.Length][];

        for(int i = 0; i < toFlash.Length; ++i)
        {
            MeshRenderer renderer = toFlash[i];
            initialColors[i] = new Color[renderer.materials.Length];
            initialEmission[i] = new Color[renderer.materials.Length];
            initialEmissionOn[i] = new bool[renderer.materials.Length];

            for(int j = 0; j < renderer.materials.Length; ++j)
            {
                initialColors[i][j] = renderer.materials[j].color;
                initialEmission[i][j] = renderer.materials[j].GetColor("_EmissionColor");
                initialEmissionOn[i][j] = renderer.materials[j].IsKeywordEnabled("_EMISSION");
            }
        }
    }

    private void Update()
    {
        if(isFlashing)
        {
            flashTimer -= Time.deltaTime;

            float t = 1 - (flashTimer / FlashDuration);
            float shakeMod = Mathf.Sin(t * Mathf.PI);

            for(int i = 0; i < toFlash.Length; ++i)
            {
                transform.localPosition = shakeBasePos + (shakeMod * ShakeMagnitude * shakeDir);
            }

            if(flashTimer <= 0)
            {
                SetFlash(false);
            }
        }
    }

    private void SetFlash(bool on)
    {
        isFlashing = on;

        for(int i = 0; i < toFlash.Length; ++i)
        {
            MeshRenderer renderer = toFlash[i];
            for(int j = 0; j < renderer.materials.Length; ++j)
            {
                renderer.materials[j].color = on ? flashColor : initialColors[i][j];
                renderer.materials[j].SetColor("_EmissionColor", on ? flashColor : initialEmission[i][j]);

                if(on) renderer.materials[j].EnableKeyword("_EMISSION");
                else if(initialEmissionOn[i][j]) renderer.materials[j].DisableKeyword("_EMISSION");
            }
        }
    }

    private void OnHealthChange(HealthChangeEvent hce)
    {
        if (hce.amount < 0)
        {
            flashTimer = FlashDuration;
            shakeBasePos = transform.localPosition;
            shakeDir = Vector3.down;
            SetFlash(true);
        }
    }
}
