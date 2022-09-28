using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BurstMissile : Missile
{
    [Header("Burst")]
    public float distance;

    [Tooltip("Event invoked when missile enters distance ↑")]
    public UnityEvent OnBurst = new UnityEvent();

    public override void Fire()
    {
        base.Fire();

        StartCoroutine(BurstMissileRoutine());
    }

    private IEnumerator BurstMissileRoutine()
    {
        while (lastTargetDistance > distance)
        {
            yield return null;
        }
        OnBurst.Invoke();
    }
}