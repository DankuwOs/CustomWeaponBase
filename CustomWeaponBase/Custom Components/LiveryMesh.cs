using System;
using System.Collections;
using UnityEngine;

public class LiveryMesh : MonoBehaviour
{
    public Renderer[] liveryMeshs;

    public Texture2D _livery;

    public void Start()
    {
        StartCoroutine(ApplyMesh());
    }

    private IEnumerator ApplyMesh()
    {
        int i = 0;
        while (liveryMeshs[0].material.GetTexture("_DetailAlbedoMap") == _livery)
        {
            Texture2D newLivery = CustomWeaponsBase.instance.GetAircraftLivery(gameObject);
            
            if (newLivery.name == _livery.name)
                continue;
            
            foreach (var renderer in liveryMeshs)
            {
                renderer.material.SetTexture("_DetailAlbedoMap", newLivery);
                renderer.material.EnableKeyword("_DETAIL_MULX2");
            }

            i++;
            yield return new WaitForSeconds(1f);
        }
    }

}