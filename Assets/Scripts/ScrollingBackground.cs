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

        // ensure the texture repeats correctly
        material.mainTexture.wrapMode = TextureWrapMode.Repeat;
    }

    public IEnumerator ScrollLeft()
    {
        if (isScrolling) yield break;
        isScrolling = true;

        float timer = 0f;

        while (timer < moveDuration)
        {
            timer += Time.deltaTime;
            offset.x += scrollSpeed * Time.deltaTime;
            material.mainTextureOffset = offset;
            yield return null;
        }

        isScrolling = false;
    }
}