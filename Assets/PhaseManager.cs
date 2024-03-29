using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseManager : MonoBehaviour
{
    BuildMode buildMode;
    BlueprintMode blueprintMode;

    private void Start()
    {
        buildMode = FindObjectOfType<BuildMode>();
        blueprintMode = FindObjectOfType<BlueprintMode>();
    }

}
