using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverTint : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Color Settings")]
    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.8f, 0.8f, 0.8f); // Slightly dimmed
    public float fadeSpeed = 8f;

    private Image img;
    private Color targetColor;

    private void Awake()
    {
        img = GetComponent<Image>();
        targetColor = normalColor;
    }

    private void OnEnable()
    {
        img.color = normalColor; 
        targetColor = normalColor;
    }

    private void Update()
    {
        img.color = Color.Lerp(img.color, targetColor, Time.deltaTime * fadeSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetColor = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetColor = normalColor;
    }
}
