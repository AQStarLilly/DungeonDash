using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveNumberVisual : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text waveLabelText;
    public Image waveNumberImage;

    [Header("Number Sprites")]
    public Sprite[] redNumberSprites;
    public Sprite[] greyNumberSprites;

    private int currentWave = 1;
    public int maxWaves = 30;

    private bool isGrey = false;

    public void Initialize(int max)
    {
        maxWaves = max;
        currentWave = 1;
        isGrey = false;
        UpdateVisual();
    }

    public void SetWave(int wave, bool grey)
    {
        currentWave = Mathf.Clamp(wave, 1, maxWaves);
        isGrey = grey;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (waveLabelText != null)
            waveLabelText.text = $"Wave";

        if (waveNumberImage == null) return;

        Sprite targetSprite = null;
        if (!isGrey && redNumberSprites.Length >= currentWave)
        {
            targetSprite = redNumberSprites[currentWave - 1];
        }
        else if (isGrey && greyNumberSprites.Length >= currentWave)
        {
            targetSprite = greyNumberSprites[currentWave - 1];
        }

        if (targetSprite != null)
        {
            waveNumberImage.sprite = targetSprite;
            waveNumberImage.enabled = true;
        }
        else
        {
            waveNumberImage.enabled = false;
        }
    }
}
