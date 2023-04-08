using UnityEngine;


public class CWB_Explosion : MonoBehaviour
{
    [Header("Ensure all of your explosion prefabs do not have a FloatingOriginTransform on them.")]
    public GameObject[] explosions;
    
    public float shake;

    [Tooltip("Use direct hit normal, if false will use velocity")]
    public bool useHitNormal;
    
    public Vector3 upAxis = Vector3.up;

    public bool shiftParticles = true;

    public bool scaleHierarchy;

    private void Awake()
    {
        if (!scaleHierarchy)
            return;
        
        foreach (var explosion in explosions)
        {
            var explosionFx = explosion.GetComponent<ExplosionFX>();
            if (!explosionFx)
                return;
            foreach (var ps in explosionFx.particleSystems)
            {
                SetScalingMode(ps);
            }
        }
    }

    public void CreateExplosionEffect(GameObject explosion, Vector3 position, Vector3 normal, float scale = 1f)
    {
        var explosionParent = new GameObject("Exploisiosne!")
        {
            transform =
            {
                position = position,
                rotation = Quaternion.FromToRotation(upAxis, normal),
                parent = null,
                localScale = Vector3.one * scale
            }
        };
        
        var floatingOriginTransform = explosionParent.AddComponent<FloatingOriginTransform>();
        floatingOriginTransform.shiftParticles = shiftParticles;

        var obj = Instantiate(explosion, explosionParent.transform);
        obj.SetActive(true);


        CustomWeaponsBase.instance.AddObject(obj);
    }

    public void SetScalingMode(ParticleSystem ps)
    {
        var main = ps.main;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;
    }
}