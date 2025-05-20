using UnityEngine;
using UnityEngine.UI;

public class ManaBarUI : MonoBehaviour
{
    public CharacterStats stats;      // Drag player or character with CharacterStats.cs here
    public Image manaFillImage;       // Drag UI Image representing the mana fill here

    void Update()
    {
        if (stats != null && manaFillImage != null)
        {
            manaFillImage.fillAmount = stats.GetNormalizedMana();
        }
    }
}
