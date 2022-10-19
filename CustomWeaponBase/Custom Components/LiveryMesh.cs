using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class LiveryMesh : MonoBehaviour
{
    [Header("PUT AN EMPTY TEXTURE IN YOUR DETAIL ALBEDOx2")]
    public Renderer[] liveryMeshs;

    [Tooltip("Put an empty texture here, used for checking if the ")]
    public Texture2D _livery;

    private int i;

    private void Start()
    {
        foreach (var renderer in liveryMeshs)
        {
            renderer.material.SetTexture("_DetailAlbedoMap", _livery);
        }
    }

    public void ApplyMesh()
    {
            var equip = GetComponent<HPEquippable>();
            if (!equip)
                return;
            
            Texture2D newLivery = CustomWeaponsBase.instance.GetAircraftLivery(equip);

            if (newLivery.name == _livery.name)
                return;
            
            foreach (var renderer in liveryMeshs)
            {
                renderer.material.SetTexture("_DetailAlbedoMap", newLivery);
                renderer.material.EnableKeyword("_DETAIL_MULX2");
            }
    }

    private void FixedUpdate()
    {
        i++;
        if (!(i >= 50))
            return;
        
        i = 0;

        ApplyMesh();
    }
}