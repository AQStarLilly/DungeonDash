using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ShieldVisual : MonoBehaviour
{
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = false; // start hidden
    }

    public void ShowShield()
    {
        if (sr != null)
            sr.enabled = true;
    }

    public void HideShield()
    {
        if (sr != null)
            sr.enabled = false;
    }

    public void FlashHit()
    {
        // optional: small flash when taking damage
        if (sr != null)
            StartCoroutine(FlashRoutine());
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        Color original = sr.color;
        sr.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        sr.color = original;
    }
}
