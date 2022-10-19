using System;
using System.Collections.Generic;
using UnityEngine;

public class CWB_Explosion : MonoBehaviour
{
    [Serializable]
    public struct Explosions
    {
        public GameObject explosionObject;

        public float shake;
    }

    public List<Explosions> explosionsList;

    public void CreateExplosionEffect(GameObject explosion, float shake, Vector3 position, Vector3 normal, float scale = 1f)
    {
        var explosionParent = new GameObject("Exploisiosne!")
        {
            transform =
            {
                position = position,
                rotation = Quaternion.LookRotation(normal),
                parent = null
            }
        };
        
        Instantiate(explosion, explosionParent.transform);
        
        if (FlybyCameraMFDPage.instance && FlybyCameraMFDPage.instance.isCamEnabled && FlybyCameraMFDPage.instance.flybyCam)
        {
            try
            {
                float num2 = shake * shake / (FlybyCameraMFDPage.instance.flybyCam.transform.position - position).sqrMagnitude;
                FlybyCameraMFDPage.instance.ShakeCamera(num2 * 2f);
            }
            catch (NullReferenceException arg)
            {
                Debug.LogError(string.Format("Got an NRE creating an explosion effect despite checking for references first! Likely after a NetDestroy command.\n{0}", arg));
            }
        }
    }
}