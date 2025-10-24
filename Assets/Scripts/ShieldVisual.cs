using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ShieldVisual : MonoBehaviour
{
    private SpriteRenderer sr;
    private float fullScale = 1f;
    private bool isVisible = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = false; // start hidden
        fullScale = transform.localScale.x;
    }

    public void ShowShield()
    {
        sr.enabled = true;
        isVisible = true;
        transform.localScale = Vector3.one * fullScale;
    }

    public void HideShield()
    {
        sr.enabled = false;
        isVisible = false;
    }

    public void UpdateShieldVisual(float currentShield, float maxShield)
    {
        if (!isVisible || maxShield <= 0)
            return;

        float ratio = Mathf.Clamp01(currentShield / maxShield);
        transform.localScale = Vector3.one * fullScale * ratio;

        if (ratio <= 0.01f)
        {
            HideShield();
        }
    }

    public void FlashHit()
    {
        if (sr != null)
            StartCoroutine(FlashRoutine());
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        if (sr == null) yield break;
        Color original = sr.color;

        // quick bright flash when hit
        sr.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        sr.color = original;
    }
}
