// Meme MOBA - LimitedViewController
// Dynamic, per-layer camera culling to simulate "limited vision" radius (client-side).
using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class LimitedViewController : MonoBehaviour
{
    [Header("Vision Radii by Layer (meters)")]
    [SerializeField] private float defaultRadius = 40f;
    [SerializeField] private float enemiesRadius = 25f;
    [SerializeField] private float minionsRadius = 30f;
    [SerializeField] private float projectilesRadius = 40f;
    [SerializeField] private float environmentRadius = 150f;

    private Camera cam;
    private float[] cullDistances;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cullDistances = new float[32];
        Apply();
    }

    public void SetDefault(float r){ defaultRadius = Mathf.Max(1f, r); Apply(); }
    public void SetEnemies(float r){ enemiesRadius = Mathf.Max(1f, r); Apply(); }
    public void SetMinions(float r){ minionsRadius = Mathf.Max(1f, r); Apply(); }
    public void SetProjectiles(float r){ projectilesRadius = Mathf.Max(1f, r); Apply(); }
    public void SetEnvironment(float r){ environmentRadius = Mathf.Max(1f, r); Apply(); }

    public void Apply()
    {
        for (int i = 0; i < 32; i++) cullDistances[i] = defaultRadius;

        int enemies = LayerMask.NameToLayer("Enemies");
        int minions = LayerMask.NameToLayer("Minions");
        int projectiles = LayerMask.NameToLayer("Projectiles");
        int environment = LayerMask.NameToLayer("Environment");

        if (enemies >= 0) cullDistances[enemies] = enemiesRadius;
        if (minions >= 0) cullDistances[minions] = minionsRadius;
        if (projectiles >= 0) cullDistances[projectiles] = projectilesRadius;
        if (environment >= 0) cullDistances[environment] = environmentRadius;

        cam.layerCullDistances = cullDistances;
        cam.layerCullSpherical = true; // nicer radial falloff
    }
}
