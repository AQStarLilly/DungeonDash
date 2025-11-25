using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.TextCore.Text;

public class FloatingDamageText : MonoBehaviour
{
    public TMP_Text text;
    public float floatSpeed = 2f;
    public float lifetime = 1.1f;

    private Color originalColor;

    private void Awake()
    {
        if(text == null)
            text = GetComponent<TMP_Text>();
        originalColor = text.color;
    }

    public void Initialize(int damage, bool isCrit, bool isShield = false)
    {
        text.text = damage.ToString();        
        if (isShield)
        {
            text.color = Color.blue;
        }
        else if (isCrit)
        {
            text.color = Color.red;
            text.text += "* ";
            text.text += "CRITICAL";
            text.fontSize *= 1.3f;
        }
        else
        {
            text.color = Color.white;
        }

        StartCoroutine(FloatAndFade());
    }

    private IEnumerator FloatAndFade()
    {
        float elapsed = 0f;
        Vector3 start = transform.position;

        while (elapsed < lifetime)
        {
            transform.position = start + Vector3.up * (elapsed * floatSpeed);

            float alpha = Mathf.Lerp(1f, 0f, elapsed / lifetime);
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);

            elapsed += Time.deltaTime;
            yield return null; 
        }
        Destroy(gameObject);
    }
}
