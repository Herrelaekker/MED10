using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PhaseManager : MonoBehaviour
{
    BuildMode buildMode;
    BattleMode battleMode;
    CinemachineSwitcher switcher;

    private int mana;
    public TMP_Text manaText;

    public Animator bgAnimator;
    MagicAttack magicAttack;

    private void Start()
    {
        buildMode = FindObjectOfType<BuildMode>();
        battleMode = FindObjectOfType<BattleMode>();
        switcher = Camera.main.GetComponent<CinemachineSwitcher>();
        magicAttack = FindObjectOfType<MagicAttack>();    

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
        magicAttack.DisableMagicAttack();
    }

    void GoToBattleMode()
    {
        bgAnimator.SetInteger("Phase", 2);
    }
    void GoToBuildMode()
    {
        bgAnimator.SetInteger("Phase", 1);
        switcher.SwitchState("Build");
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
