using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UltimateBarUI : MonoBehaviour
{
    public CharacterStats stats;         // Reference to CharacterStats
    public Image ultimateFillImage;      // Fill image for UI bar

    void Update()
    {
        if (stats != null && ultimateFillImage != null)
        {
            ultimateFillImage.fillAmount = stats.GetNormalizedUltimate();
        }
    }
}
