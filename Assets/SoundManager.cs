using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public enum SoundState {
    RejectSound,
    CorrectSound,
    None
}

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource correctSound;
    [SerializeField]
    private AudioSource wrongSound;
    private SoundState soundState = SoundState.None;
    private bool madeDecision = false;


    AudioSource curTrack;

    public AudioSource buildBG, defendBG;
    float maxVolBuild, maxVolDefend;

    public AudioSource conjuringSound;

    // Start is called before the first frame update
    void Start()
    {
        curTrack = defendBG;
        defendBG.Play();
        maxVolBuild = buildBG.volume;
        maxVolDefend = defendBG.volume;
    }

    public void PlayConjuringSound()
    {
        conjuringSound.Play();
        StartCoroutine(StopConjuringSound());
    }

    IEnumerator StopConjuringSound()
    {
        yield return new WaitForSeconds(5f);
        conjuringSound.Stop();
    }

    public void SwapTrack(bool buildSound)
    {
        if (buildSound)
        {
            StartCoroutine(FadeTrack(buildBG, maxVolDefend, maxVolBuild));
        }else
        {
            StartCoroutine(FadeTrack(defendBG, maxVolBuild, maxVolDefend));
        }
    }

    IEnumerator FadeTrack(AudioSource nextTrack, float curMaxVol, float nextMaxVol)
    {
        float timeToFade = 1f;
        float timeElapsed = 0f;

        if (curTrack != nextTrack)
        {

            nextTrack.Play();

            while(timeElapsed < timeToFade)
            {
                curTrack.volume = Mathf.Lerp(curMaxVol, 0, timeElapsed / timeToFade);
                nextTrack.volume = Mathf.Lerp(0, nextMaxVol, timeElapsed / timeToFade);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            curTrack.Stop();
            curTrack = nextTrack;
        }
    }

    public void OnGameDecision(GameDecisionData decisionData) {
        if (decisionData.decision == TrialType.AccInput) {
            correctSound.Play();
            soundState = SoundState.CorrectSound;
        } else if (decisionData.decision == TrialType.FabInput) {
            correctSound.Play();
            soundState = SoundState.CorrectSound;
        } else {
            soundState = SoundState.RejectSound;
            wrongSound.Play();
            soundState = SoundState.None;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
