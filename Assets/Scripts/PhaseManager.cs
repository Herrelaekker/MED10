using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum Intensity { 
    Low,
    High,
    UserChosen
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
    public int BCIExercisesPerTriangle = 1;
    public int triangles = 1;
    public int iterations = 1;

    int maxManaPerPhase;

    public GameObject userChosenIntensityUI;

    public int manaAmountPerEnemy = 5;
    LoggingManager loggingManager;
    SoundManager soundManager;

    private void Start()
    {
        loggingManager = GameObject.Find("LoggingManager").GetComponent<LoggingManager>();

        buildMode = FindObjectOfType<BuildMode>();
        battleMode = FindObjectOfType<BattleMode>();
        switcher = Camera.main.GetComponent<CinemachineSwitcher>();
        magicAttack = FindObjectOfType<MagicAttack>();    
        waveSpawner = FindObjectOfType<WaveSpawner>();
        soundManager = FindObjectOfType<SoundManager>();

        if (intensity == Intensity.Low)
        {
            maxManaPerPhase = BCIExercisesPerTriangle;
            waveSpawner.SetEnemyAmount(BCIExercisesPerTriangle);
        }
        else if (intensity == Intensity.High)
        {
            maxManaPerPhase = BCIExercisesPerTriangle * triangles;
            waveSpawner.SetEnemyAmount(triangles * BCIExercisesPerTriangle);
        }
        else if (intensity == Intensity.UserChosen)
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
        buildMode.StartBuildMode();
        soundManager.SwapTrack(true);

    }

    void GoToBattleMode()
    {
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
            {"Phase", phase}
        };

        loggingManager.Log("Game", gameLog);
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
        if (mana + addedMana > maxManaPerPhase * manaAmountPerEnemy)
        {
            mana = maxManaPerPhase * manaAmountPerEnemy;
        }
        else
        {
            mana += addedMana;
        }
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
