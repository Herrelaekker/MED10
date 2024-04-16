using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousFeedback : MonoBehaviour
{
    float confidence = -1;

    public ParticleSystem particles;
    ParticleSystem.MainModule psMain;
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
        psMain = particles.main;
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
        Color col = Color.red;
        float t = confidence / classficationThreshold;
        float alphaVal = Mathf.Lerp(minAlpha, maxAlpha, t);
        psMain.startColor = new Color(col.r, col.g, col.b, alphaVal);
    }

}
