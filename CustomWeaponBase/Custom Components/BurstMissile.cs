using UnityEngine;
using UnityEngine.Events;

public class BurstMissile : Missile
{
    [Header("Burst")]
    public float distance;

    [Tooltip("Event invoked when missile enters distance ↑")]
    public UnityEvent OnBurst = new UnityEvent();

    private bool _busted;

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        
        if (lastTargetDistance > distance || _busted)
        {
            return;
        }
        
        OnBurst.Invoke();
        _busted = true;
    }
}