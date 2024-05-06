using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System;
using System.Diagnostics.Tracing;

public enum Intensity { 
A, B, C
}

public class PhaseManager : MonoBehaviour
{
    BuildMode buildMode;
    BattleMode battleMode;
    CinemachineSwitcher switcher;

    private int mana;
    public TMP_Text manaText;

    public Animator bgAnimator;
    MagicAttack magicAttack;
    WaveSpawner waveSpawner;

    public Intensity intensity;
    public Intensity GetIntensity() { return intensity; }

    public int BCIExercisesPerTriangle = 1;
    public int triangles = 1;
    public int iterations = 1;

    int maxManaPerPhase;

    public GameObject userChosenIntensityUI;

    public int manaAmountPerEnemy = 5;
    LoggingManager loggingManager;
    SoundManager soundManager; 
    GameManager gameManager;
    int maxBCIExercises = 0;

    public GameObject doneUI;

    public GameObject wandUI;
    string filestamp;

    int manaGainedTotal = 0;

    // Update is called once per frame
    void TakeScreenshot(string screenshotName)
    {
        ScreenCapture.CaptureScreenshot(Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments),"screenshot"+ filestamp + screenshotName + ".png"));
        Debug.Log("Screenshot Captured");
    }

    private void Start()
    {
        doneUI.SetActive(false);
        wandUI.SetActive(false);


        gameManager = FindObjectOfType<GameManager>();

        loggingManager = GameObject.Find("LoggingManager").GetComponent<LoggingManager>();
        filestamp = loggingManager.GetFileStamp();

        buildMode = FindObjectOfType<BuildMode>();
        battleMode = FindObjectOfType<BattleMode>();
        switcher = Camera.main.GetComponent<CinemachineSwitcher>();
        magicAttack = FindObjectOfType<MagicAttack>();    
        waveSpawner = FindObjectOfType<WaveSpawner>();
        soundManager = FindObjectOfType<SoundManager>();

        maxBCIExercises = BCIExercisesPerTriangle * triangles * iterations;

        if (intensity == Intensity.A)
        {
            maxManaPerPhase = BCIExercisesPerTriangle;
            waveSpawner.SetEnemyAmount(BCIExercisesPerTriangle);
        }
        else if (intensity == Intensity.B)
        {
            maxManaPerPhase = BCIExercisesPerTriangle * triangles;
            waveSpawner.SetEnemyAmount(triangles * BCIExercisesPerTriangle);
        }
        else if (intensity == Intensity.C)
        {

            maxManaPerPhase = BCIExercisesPerTriangle * triangles * iterations;
            waveSpawner.SetEnemyAmount(triangles * BCIExercisesPerTriangle);
            userChosenIntensityUI.SetActive(true);
        }

        waveSpawner.SetMaxTotalDeaths(BCIExercisesPerTriangle * triangles * iterations);

        GoToBattleMode();
        //GoToBuildMode();
    }

    public void NoMoreDefending()
    {
        buildMode.NoMoreDefending();
    }

    public void BuildModeEnded()
    {
        //If this is next in line
        GoToBattleMode();
    }

    public void BattleModeEnded()
    {
        GoToBuildMode();
    }

    public void BattleTransitionDone()
    {
        loggingManager.SetPhase("Defend");
        LogEvent("SwitchPhase", "Defend");
        battleMode.StartBattle();
        switcher.SwitchState("Battle");
        magicAttack.EnableMagicAttack();
        soundManager.SwapTrack(false);
    }

    public void BuildTransitionDone()
    {
        loggingManager.SetPhase("Build");
        LogEvent("SwitchPhase", "Build");
        wandUI.SetActive(true);
        buildMode.StartBuildMode();
        soundManager.SwapTrack(true);

    }

    void GoToBattleMode()
    {
        wandUI.SetActive(false);
        bgAnimator.SetInteger("Phase", 2);
    }
    void GoToBuildMode()
    {

        bgAnimator.SetInteger("Phase", 1);
        switcher.SwitchState("Build");
        magicAttack.DisableMagicAttack();
    }

    private void LogEvent(string eventLabel, string phase)
    {
        Dictionary<string, object> gameLog = new Dictionary<string, object>() {
            {"Event", eventLabel},
            {"Phase", phase},
            {"ManaTotal", manaGainedTotal}
        };

        loggingManager.Log("Game", gameLog);
    }

    public void DoneWithVersion()
    {
        TakeScreenshot("test");
        StartCoroutine(WaitAfterScreenshot());
    }

    IEnumerator WaitAfterScreenshot()
    {
        yield return new WaitForSeconds(2);
        doneUI.SetActive(true);
        gameManager.EndGame();
    }

    public bool BeenThroughEnoughExercises(int exerciseAmount)
    {
        return (exerciseAmount >= maxBCIExercises);
    }

    public bool HaveEnoughMana(int mana)
    {
        if (this.mana >= mana)
        {
            return true;
        }
        else return false;
    }

    public void AddToMana(int addedMana)
    {
        addedMana *= manaAmountPerEnemy;

        if (manaGainedTotal > maxManaPerPhase * manaAmountPerEnemy)
        {
            int difference = manaGainedTotal - maxManaPerPhase * manaAmountPerEnemy;

            mana += addedMana;
            mana -= difference;

            manaGainedTotal = maxManaPerPhase * manaAmountPerEnemy;
        }
        else
        {
            mana += addedMana;
            manaGainedTotal += addedMana;
        }

        /*if (mana + addedMana > maxManaPerPhase * manaAmountPerEnemy - manaGainedTotal)
        {
            mana = maxManaPerPhase * manaAmountPerEnemy - manaGainedTotal;
            manaGainedTotal = maxManaPerPhase * manaAmountPerEnemy;
        }
        else
        {
            mana += addedMana;
            manaGainedTotal += addedMana;
        }*/

        manaText.text = mana.ToString();
    }

    public void RemoveMana(int removedMana = 1)
    {
        removedMana *= manaAmountPerEnemy;
        if (mana - removedMana < 0)
        {
            mana = 0;
        }
        else
        {
            mana -= removedMana;
        }
        manaText.text = mana.ToString();
    }
}
