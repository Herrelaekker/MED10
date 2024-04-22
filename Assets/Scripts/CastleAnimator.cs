using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CastleAnimator : MonoBehaviour
{
    public PhaseManager phaseManager;
    public void Phase1To2()
    {
        phaseManager.BattleTransitionDone();
    }    
    public void Phase2To1()
    {
        phaseManager.BuildTransitionDone();
    }
}
