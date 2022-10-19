using UnityEngine;
using UnityEngine.Events;
using VTOLVR.Multiplayer;

public class CEMissile : Missile
{
    public event UnityAction<Missile> OnMissileDetonated;

    [Header("Custom Explosions")] 
    public CWB_Explosion cwbExplosion;

    private bool useScriptedExplosion = false;

    public override void Detonate(Collider directHit)
    {
        if (detonated)
        {
            return;
        }
        detonated = true;
        Vector3 sourceVelocity = Vector3.one;
        if (rb)
        {
            sourceVelocity = rb.velocity;
        }
        else
        {
            Debug.LogError("Missile tried to detonate without a rigidbody!");
        }
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i])
            {
                colliders[i].enabled = false;
            }
        }
        if (explosionType == ExplosionManager.ExplosionTypes.Aerial)
        {
            explosionNormal = sourceVelocity;
        }

        if (!useScriptedExplosion && cwbExplosion.explosionsList.Count > 0)
        {
            cwbExplosion.CreateExplosionEffect(cwbExplosion.explosionsList[0].explosionObject, cwbExplosion.explosionsList[0].shake, transform.position, explosionNormal);
        }
        
        if (isLocal)
        {
            ExplosionManager.instance.CreateDamageExplosion(transform.position, explodeRadius, explodeDamage, actor, sourceVelocity, directHit, debugExplosion, sourcePlayer, weaponEntityID);
        }
        
        UnityEvent onDetonate = OnDetonate;
        if (onDetonate != null)
        {
            onDetonate.Invoke();
        }
        UnityAction<Missile> onMissileDetonated = OnMissileDetonated;
        onMissileDetonated?.Invoke(this);
        
        if (exhaustTransform)
        {
            ps.SetEmission(false);
            exhaustTransform.parent = null;
            FloatingOriginTransform floatingOriginTransform = exhaustTransform.gameObject.GetComponent<FloatingOriginTransform>();
            if (!floatingOriginTransform)
            {
                floatingOriginTransform = exhaustTransform.gameObject.AddComponent<FloatingOriginTransform>();
            }
            floatingOriginTransform.shiftParticles = false;
            Destroy(exhaustTransform.gameObject, ps.GetLongestLife());
            if (exhaustLight)
            {
                exhaustLight.enabled = false;
            }
        }
        if (VTOLMPUtils.IsMultiplayer())
        {
            gameObject.SetActive(false);
            return;
        }
        Destroy(gameObject);
    }
}