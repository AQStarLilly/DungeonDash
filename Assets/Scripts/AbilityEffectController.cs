using UnityEngine;
using System.Collections;

public class AbilityEffectController : MonoBehaviour
{
    public static AbilityEffectController Instance;

    [Header("Effect Prefabs")]
    public GameObject janitorFacePopup;
    public GameObject hrLadyFacePopup;
    public GameObject drunkFacePopup;

    public GameObject janitorProjectile;
    public GameObject hrLadyProjectile;
    public GameObject drunkProjectile;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayAbilityEffect(string abilityId, Transform player, Transform enemy)
    {
        GameObject facePrefab = null;
        GameObject projectilePrefab = null;

        switch (abilityId)
        {
            case "janitor":
                facePrefab = janitorFacePopup;
                projectilePrefab = janitorProjectile;
                break;

            case "hrlady":
                facePrefab = hrLadyFacePopup;
                projectilePrefab = hrLadyProjectile;
                break;

            case "drunkCoworker":
                facePrefab = drunkFacePopup;
                projectilePrefab = drunkProjectile;
                break;
        }

        if (facePrefab != null)
            StartCoroutine(SpawnAndThrow(facePrefab, projectilePrefab, player, enemy));
    }

    private IEnumerator SpawnAndThrow(GameObject facePrefab, GameObject projectilePrefab, Transform player, Transform enemy)
    {
        // --- Spawn face popup ---
        GameObject face = Instantiate(facePrefab, player.position + new Vector3(1f, 1f, 0f), Quaternion.identity);
        Destroy(face, 0.5f);

        yield return new WaitForSeconds(0.15f);

        // --- Throw projectile ---
        GameObject proj = Instantiate(projectilePrefab, face.transform.position, Quaternion.identity);

        float duration = 0.35f;
        float t = 0f;

        Vector3 start = proj.transform.position;
        Vector3 end = enemy.position + new Vector3(0f, 0.5f, 0f);

        while (t < duration)
        {
            t += Time.deltaTime;
            float progress = t / duration;
            proj.transform.position = Vector3.Lerp(start, end, progress);
            yield return null;
        }

        Destroy(proj);
    }
}
