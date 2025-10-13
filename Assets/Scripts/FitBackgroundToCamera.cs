using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FitBackgroundToCamera : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Camera mainCam;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCam = Camera.main;
        FitToScreen();
    }

    private void FitToScreen()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null || mainCam == null)
            return;

        // Get the sprite size in world units
        float spriteWidth = spriteRenderer.sprite.bounds.size.x;
        float spriteHeight = spriteRenderer.sprite.bounds.size.y;

        // Get camera height and width in world units
        float worldScreenHeight = mainCam.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight * mainCam.aspect;

        // Scale the sprite to fit the camera view
        transform.localScale = new Vector3(
            worldScreenWidth / spriteWidth,
            worldScreenHeight / spriteHeight,
            1f
        );
    }
}
