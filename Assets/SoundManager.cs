using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public enum SoundState {
    RejectSound,
    CorrectSound,
    BestSound,
    None
}

[System.Serializable]
public class BackgroundSound
{
    public AudioSource track;
    public float maxVol;
}

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource bestSound;
    [SerializeField]
    private AudioSource correctSound;
    [SerializeField]
    private AudioSource wrongSound;

    [SerializeField]
    private AudioSource uiBtnSound;
    private SoundState soundState = SoundState.None;
    private bool madeDecision = false;


    BackgroundSound[] curTrack;

    public BackgroundSound[] buildBG, defendBG;
   // public AudioSource[] buildBG, defendBG;
   // float maxVolBuild, maxVolDefend;

    public AudioSource conjuringSound;

    public AudioSource placingSound;

    // Start is called before the first frame update
    void Start()
    {
        curTrack = defendBG;

        foreach (BackgroundSound bgSnd in defendBG)
        {
            bgSnd.track.Play();
            bgSnd.maxVol = bgSnd.track.volume;
        }
        foreach (BackgroundSound bgSnd in buildBG)
        {
            bgSnd.maxVol = bgSnd.track.volume;
        }
    }

    public void PlayConjuringSound()
    {
        conjuringSound.Play();
        StartCoroutine(StopConjuringSound());
    }

    public void PlayPlacingSound()
    {
        placingSound.Play();
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
            StartCoroutine(FadeTrack(buildBG));
        }else
        {
            StartCoroutine(FadeTrack(defendBG));
        }
    }

    public void PlayUISound()
    {
        uiBtnSound.pitch = Random.Range(0.85f, 1.15f);
        uiBtnSound.Play();
    }

    IEnumerator FadeTrack(BackgroundSound[] nextTrack)
    {
        float timeToFade = 1f;
        float timeElapsed = 0f;

        if (curTrack != nextTrack)
        {
            foreach(BackgroundSound bgSnd in nextTrack)
                bgSnd.track.Play();

            while(timeElapsed < timeToFade)
            {
                foreach (BackgroundSound bgSnd in curTrack)
                    bgSnd.track.volume = Mathf.Lerp(bgSnd.maxVol, 0, timeElapsed / timeToFade);

                foreach (BackgroundSound bgSnd in nextTrack)
                    bgSnd.track.volume = Mathf.Lerp(0, bgSnd.maxVol, timeElapsed / timeToFade);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            foreach (BackgroundSound bgSnd in curTrack)
                bgSnd.track.Stop();
            curTrack = nextTrack;
        }
    }

    public void OnGameDecision(GameDecisionData decisionData) {
        if (decisionData.classification == MotorImageryEvent.GoldenMotorImagery)
        {
            bestSound.Play();
            soundState = SoundState.BestSound;
        }
        if (decisionData.classification == MotorImageryEvent.MotorImagery)
        {
            correctSound.Play();
            soundState = SoundState.CorrectSound;
        }
        else {
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
