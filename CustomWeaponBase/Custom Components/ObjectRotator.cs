using UnityEngine;


public class ObjectRotator : MonoBehaviour
{
    public Transform rotationTf;

    public Vector3 rotationAxis = new Vector3(0, 0, 1);

    public AnimationCurve rotationCurve;

    public float swapObjectRPM;

    public GameObject[] objectsToEnable;

    public GameObject[] objectsToDisable;
    
    [Header("Don't touch, for animations and events.")]
    public bool rotate;

    private float _time;

    private bool _secondaryObjectsEnabled;

    private float _rpm;

    public void Update()
    {
        if (!rotate)
            return;
        
        _time += Time.deltaTime;

        if (_rpm > swapObjectRPM && !_secondaryObjectsEnabled)
        {
            foreach (var o in objectsToDisable)
            {
                o.SetActive(false);
            }

            foreach (var o in objectsToEnable)
            {
                o.SetActive(true);
            }

            _secondaryObjectsEnabled = true;
        }

        _rpm = rotationCurve.Evaluate(_time);

        if (_secondaryObjectsEnabled)
            _rpm /= 5;
        
        rotationTf.Rotate(rotationAxis, _rpm * Time.deltaTime);
    }

    public void Rotate() => rotate = true;

    public void StopRotating() => rotate = false;
}