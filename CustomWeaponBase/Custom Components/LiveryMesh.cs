using System;
using System.Collections;
using System.Configuration;
using System.Threading.Tasks;
using UnityEngine;

public class LiveryMesh : MonoBehaviour
{
    [Header("PUT AN EMPTY TEXTURE IN YOUR DETAIL ALBEDOx2")] [Tooltip("All the meshs's that liveries shall be applicableied")]
    public Renderer[] liveryMeshs;

    [Tooltip("Put an empty texture here")] public Texture2D _livery;

    private int i;

    private HPEquippable _equippable;

    private void Awake()
    {
        _equippable = GetComponent<HPEquippable>();
        _equippable.OnEquipped += ApplyMesh;

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