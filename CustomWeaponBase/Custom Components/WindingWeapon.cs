
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WindingWeapon : MonoBehaviour
{
    public HPEquippable equip;
    
    public AudioSource _source;
    
    [Range(0f, 5f)] public float pitchFactor;
    
    [Range(0f, 5f)] public float windUpTime;

    [Range(0f, 3f)] public float volumeMultiplier;

    public AnimationCurve pitchCurve;
    public AnimationCurve volumeCurve;

    public UnityEvent startFiring;
    public UnityEvent onFired;
    public UnityEvent stopFiring;
    
    private bool winding;
    
    private float _windUp;

    private bool isCoroutine;

    public void StartWinding()
    {
        
        if (winding != true)
        {
            winding = true;
            if (!isCoroutine)
            {
                isCoroutine = true;
                _source.Play();
                StartCoroutine(WindUpWeapon());
            }
        }

        UnityEvent startFiring = this.startFiring;
        if (startFiring != null)
        {
            startFiring.Invoke();
        }
    }

    public void StopWinding()
    {
        winding = false;
        
        UnityEvent stopFiring = this.stopFiring;
        if (stopFiring != null)
        {
            stopFiring.Invoke();
        }
    }
    
    public IEnumerator WindUpWeapon()
    {
        while (true)
        {
            if (winding)
            {
                _windUp += Time.deltaTime;
            }
            else
            {
                _windUp -= 2 * Time.deltaTime;
                if (_windUp <= 0f)
                {
                    _source.Stop();
                    _source.pitch = 0f;
                    _source.volume = 0f;
                    isCoroutine = false;
                    yield break;
                }
            }

            if (_windUp >= windUpTime)
            {
                _windUp = 0f;
                _source.Stop();
                _source.pitch = 0f;
                _source.volume = 0f;
                UnityEvent onFired = this.onFired;
                if (onFired != null)
                {
                    onFired.Invoke();
                }
                isCoroutine = false;
                yield break;
            }
            _source.pitch = pitchCurve.Evaluate(_windUp / windUpTime) * pitchFactor;
            _source.volume = volumeCurve.Evaluate(_windUp / windUpTime) * volumeMultiplier;
            yield return null;
        }
    }
}