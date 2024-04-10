using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CinemachineSwitcher : MonoBehaviour
{
    private Animator animator;

    public CinemachineVirtualCamera buildCam;

    CinemachineBrain brain;

    float curBlendDuration = 2f;

    bool blending = false;
    bool stoppedBlending = false;

    private void Awake()
    {
        brain = GetComponent<CinemachineBrain>();
        animator = GetComponent<Animator>();
    }

    public void BuildFollowTarget(Transform targetTrans)
    {
        buildCam.Follow = targetTrans;
    }

    public void SwitchState(string phase)
    {
        blending = true;
        animator.Play(phase);
        StartMovingCamera();
    }

    public float GetBlendDuration()
    {
        curBlendDuration = brain.ActiveBlend.Duration;
        return curBlendDuration;
    }

    public void StartMovingCamera()
    {
        stoppedBlending = false;
        blending = true;

        print("Blend Duration: " + curBlendDuration);

        StartCoroutine(StoppedBlending(curBlendDuration));
    }

    IEnumerator StoppedBlending(float duration)
    {
        yield return new WaitForSeconds(duration);
        blending = false;
        stoppedBlending = true;

    }

    public bool DoneBlending()
    {
        return stoppedBlending;
    }

}
