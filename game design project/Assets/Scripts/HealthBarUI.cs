using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public CharacterStats Health; // Drag boss object with BossHealth.cs here
    public Image healthFillImage; // Drag BossHealthBarFill here

    void Update()
    {
        if (Health != null && healthFillImage != null)
        {
            healthFillImage.fillAmount = Health.GetNormalizedHealth();
        }
    }
}
