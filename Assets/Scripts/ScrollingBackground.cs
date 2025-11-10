using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class ScrollingBackground : MonoBehaviour
{
    private Material material;
    private Vector2 offset = Vector2.zero;

    [Header("Scroll Settings")]
    public float scrollSpeed = 0.1f;
    public float moveDuration = 2f;

    private bool isScrolling = false;

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        material = sr.material;

        if (material == null || material.mainTexture == null)
        {
            Debug.LogError("ScrollingBackground: Missing material or texture on sprite renderer.");
            enabled = false;
            return;
        }

        //material.mainTexture.wrapMode = TextureWrapMode.Repeat;
        material.mainTextureOffset = Vector2.zero;
    }

    public IEnumerator ScrollLeft()
    {
        if (isScrolling) yield break;
        isScrolling = true;

        float timer = 0f;

        while (timer < moveDuration)
        {
            //If the game is pasued, wait until it's resumed
            if (GameManager.Instance != null && GameManager.Instance.currentState == GameManager.GameState.Pause)
            {
                yield return null;
                continue;
            }

            //Scroll normally during gameplay
            timer += Time.deltaTime;
            offset.x += scrollSpeed * Time.deltaTime;
            material.mainTextureOffset = offset;
            yield return null;
        }

        isScrolling = false;
    }

    public void ResetScroll()
    {
        offset = Vector2.zero;
        if(material != null)
        {
            material.mainTextureOffset = offset;
        }
    }
}