using UnityEngine;
using UnityEngine.Events;

public class BurstMissile : Missile
{
    [Header("Burst")]
    public float distance;

    [Tooltip("Event invoked when missile enters distance ↑")]
    public UnityEvent OnBurst = new UnityEvent();

    private bool _busted;

    public override void BPU_FixedUpdate()
    {
        base.BPU_FixedUpdate();
        
        
        
        if (Vector3.Distance(transform.position, estTargetPos) > distance || _busted)
        {
            return;
        }
        
        OnBurst.Invoke();
        _busted = true;
    }
}