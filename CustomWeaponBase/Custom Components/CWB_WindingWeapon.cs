using UnityEngine;
using UnityEngine.Events;


public class CWB_WindingWeapon : MonoBehaviour
{
    public AudioSource source;
    
    [Range(0f, 5f)] public float pitchFactor;
    
    [Range(0f, 5f)] public float windUpTime;

    [Range(0f, 3f)] public float volumeMultiplier;

    public AnimationCurve pitchCurve;
    public AnimationCurve volumeCurve;

    public UnityEvent<float> OnWind;
    public UnityEvent OnStartWind;
    public UnityEvent OnStopWind;
    
    
    private bool _winding;
    
    private float _windUp;

    private void OnEnable()
    {
        source.volume = 0;
        source.Play();
    }

    private void OnDisable()
    {
        source.Stop();
    }

    public void StartWinding()
    {
        _winding = true;
        if (OnStartWind != null)
            OnStartWind.Invoke();
    }

    public void StopWinding()
    {
        _winding = false;
        if (OnStopWind != null)
            OnStopWind.Invoke();
    }

    public void StopWindingImmediate()
    {
        _winding = false;
        _windUp = 0;
        source.volume = 0;
        if (OnStopWind != null)
            OnStopWind.Invoke();
    }

    private void Update()
    {
        if (_winding)
        {
            _windUp += Time.deltaTime;
        }
        else
        {
            _windUp -= 2 * Time.deltaTime;
            if (_windUp <= 0f)
            {
                source.pitch = 0f;
                source.volume = 0f;
                _windUp = 0;
                
                if (OnWind != null)
                    OnWind.Invoke(_windUp);
                
                return;
            }
        }

        _windUp = Mathf.Clamp(_windUp, 0, windUpTime + 0.0001f);
        
        if (OnWind != null)
            OnWind.Invoke(_windUp);

        source.pitch = pitchCurve.Evaluate(Mathf.Min(windUpTime, _windUp) / windUpTime) * pitchFactor;
        source.volume = volumeCurve.Evaluate(Mathf.Min(windUpTime, _windUp) / windUpTime) * volumeMultiplier;
    }

    public bool IsWoundUp()
    {
        return _windUp >= windUpTime;
    }

    public bool IsWinding() => _winding;

    public float WindT() => _windUp / windUpTime;
}