using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    private Slider slider;

    private void Start()
    {
        slider = GetComponent<Slider>();

        if (SoundManager.Instance != null)
        {
            slider.value = SoundManager.Instance.GetVolume();
            slider.onValueChanged.AddListener(SoundManager.Instance.SetVolume);
        }
    }
}
