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

    private void Start()
    {
        buildMode = FindObjectOfType<BuildMode>();
        battleMode = FindObjectOfType<BattleMode>();
        switcher = Camera.main.GetComponent<CinemachineSwitcher>();

        GoToBattleMode();
    }

    public void BuildModeEnded()
    {
        //If this is next in line
        GoToBattleMode();
    }

    public void BattleModeEnded()
    {
        GoToBlueprintMode();
    }

    void GoToBattleMode()
    {
        battleMode.StartBattle();
        switcher.SwitchState("Battle");
    }
    void GoToBuildMode()
    {
        buildMode.StartBuildMode();
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
