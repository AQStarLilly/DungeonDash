using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform fillTransform;  // assign the Fill child
    [SerializeField] private RectTransform backgroundTransform; // assign the BG parent

    private float maxWidth;

    private void Awake()
    {
        if (backgroundTransform != null)
            maxWidth = backgroundTransform.rect.width;
    }

    public void SetHealth(float current, float max)
    {
        if (fillTransform == null || backgroundTransform == null)
            return;

        float percent = Mathf.Clamp01(current / max);
        fillTransform.sizeDelta = new Vector2(maxWidth * percent, fillTransform.sizeDelta.y);
    }
}