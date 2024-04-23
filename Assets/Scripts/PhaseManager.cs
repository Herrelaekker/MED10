using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public enum Intensity { 
    Low,
    High
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

    private void Start()
    {
        buildMode = FindObjectOfType<BuildMode>();
        battleMode = FindObjectOfType<BattleMode>();
        switcher = Camera.main.GetComponent<CinemachineSwitcher>();
        magicAttack = FindObjectOfType<MagicAttack>();    
        waveSpawner = FindObjectOfType<WaveSpawner>();

        if (intensity == Intensity.Low)
        {
            waveSpawner.SetEnemyAmount(BCIExercisesPerTriangle);
        }
        else if (intensity == Intensity.High)
        {
            waveSpawner.SetEnemyAmount(triangles * BCIExercisesPerTriangle);
        }

        GoToBattleMode();
        //GoToBuildMode();
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
        battleMode.StartBattle();
        switcher.SwitchState("Battle");
        magicAttack.EnableMagicAttack();
    }
    public void BuildTransitionDone()
    {
        buildMode.StartBuildMode();
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
        mana += addedMana;
        manaText.text = mana.ToString();
    }

    public void RemoveMana(int removedMana = 1)
    {
        mana -= removedMana;
        manaText.text = mana.ToString();
    }
}
