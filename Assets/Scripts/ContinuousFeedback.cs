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

    [SerializeField]
    private Gradient gradient = new Gradient();

    public float minScale = .25f, maxScale = .5f;
    float curScaleVal;

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

        curScaleVal = minScale;
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
            SetScale();
        }
    }

    void SetScale()
    {
        /*if (confidence >= classficationThreshold)
        {
            if (curScaleVal <= maxScale)
                curScaleVal += Time.deltaTime;
        }
        else
        {
            if (curScaleVal >= minScale )
                curScaleVal -= Time.deltaTime;
        }
        for (int i = 0; i < psMains.Length; i++)
        {
            particles[i].transform.localScale = new Vector3(curScaleVal, curScaleVal, curScaleVal);
        }*/
        float scaleVal = Mathf.Lerp(minScale, maxScale, confidence);
        for (int i = 0; i < psMains.Length; i++)
        {
            particles[i].transform.localScale = new Vector3(scaleVal, scaleVal, scaleVal);
        }
    }

    void SetAlpha()
    {
        Color col = Color.white;
        float t = confidence / classficationThreshold;
        float alphaVal = Mathf.Lerp(minAlpha, maxAlpha, t);
        //float scaleVal = Mathf.Lerp(minSize, maxSize, t);
        Color color = gradient.Evaluate(t);

        for (int i = 0; i < psMains.Length; i++)
        {
            psMains[i].startColor = color;
            //particles[i].transform.localScale = new Vector3(scaleVal, scaleVal, scaleVal);
        }
    }

}
