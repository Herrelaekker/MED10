using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousFeedback : MonoBehaviour
{
    float confidence = -1;

    public ParticleSystem[] particles;
    ParticleSystem.MainModule[] psMains;
    bool isActive = true;

    float classficationThreshold;

    public float minAlpha = 0.25f, maxAlpha = 1;

    private void Start()
    {
        SimBCIInput fabBCIInput = FindObjectOfType<SimBCIInput>();
        if (fabBCIInput.enabled)
        {
            classficationThreshold = fabBCIInput.classificationThreshold;
        }
        else
        {
            OpenBCIInput openBCIInput = FindObjectOfType<OpenBCIInput>();
            if (openBCIInput.enabled)
            {
                classficationThreshold = openBCIInput.classificationThreshold;
            }
        }
        print(classficationThreshold);
        psMains = new ParticleSystem.MainModule[particles.Length];
        for ( int i = 0; i < psMains.Length; i++)
            psMains[i] = particles[i].main;
    }

    public void SetConfidence(float confidence)
    {
        this.confidence = confidence;
    }

    public void SetIsActive(bool isActive)
    {
        this.isActive = isActive;
    }

    private void Update()
    {
        if (isActive)
        {
            SetAlpha();
        }
    }

    void SetAlpha()
    {
        Color col = Color.white;
        float t = confidence / classficationThreshold;
        float alphaVal = Mathf.Lerp(minAlpha, maxAlpha, t);
        for(int i = 0; i < psMains.Length; i++)
            psMains[i].startColor = new Color(col.r, col.g, col.b, alphaVal);
    }

}
