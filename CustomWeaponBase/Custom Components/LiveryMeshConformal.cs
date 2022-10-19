
using UnityEngine;

public class LiveryMeshConformal : LiveryMesh
{
    public Material yourMaterial;
    
    private string _ALDetailAlbedox2 = "_DetailAlbedoMap";
    private string _ALDetailNormal = "_DetailNormalMap";
    private string _ALGloss = "_SpecGlossMap";

    private string _MAINDetailAlbedox2 = "_MainTex";
    private string _MAINDetailNormal = "_BumpMap";
    private string _MAINDetailGloss = "_MetallicGlossMap";

    public override void ApplyMesh()
    {
        var equip = GetComponent<HPEquippable>();
        if (!equip || !equip.weaponManager)
            return;
        
        var wm = equip.weaponManager;
        var aircraft = wm.gameObject;

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        AircraftLiveryApplicator applicator = wm.gameObject.GetComponent<AircraftLiveryApplicator>();
        Material livery = applicator.materials[0];
        
        MeshRenderer renderer = null;

        if (aircraft.name.Contains("FA-26B"))
            renderer = aircraft.transform.Find("aFighter2").Find("body").GetComponent<MeshRenderer>();
        if (aircraft.name.Contains("SEVTF"))
            renderer = aircraft.transform.Find("sevtf_layer_2").Find("body.001").GetComponent<MeshRenderer>();
        if (aircraft.name.Contains("AH-94"))
            renderer = aircraft.transform.Find("ah94_final").Find("exteriorBody").GetComponent<MeshRenderer>();
        if (aircraft.name.Contains("VTOL4"))
            renderer = aircraft.transform.Find("VT4Body(new)").Find("body_main").GetComponent<MeshRenderer>();

        if (aircraft.transform.Find("liverySample")) // Incase any plane devs use liveries and wanna support this.
            renderer = aircraft.transform.Find("liverySample").GetComponent<MeshRenderer>();

        if (!renderer)
            return;

        renderer.GetPropertyBlock(block);
        
        foreach (var meshRenderer in liveryMeshs)
        {
            meshRenderer.material = livery;
            meshRenderer.SetPropertyBlock(block);

            var albedo = yourMaterial.GetTexture(_MAINDetailAlbedox2);
            var gloss = yourMaterial.GetTexture(_MAINDetailGloss);
            var normal = yourMaterial.GetTexture(_MAINDetailNormal);

            var mat = meshRenderer.material;
            if (albedo)
                mat.SetTexture(_ALDetailAlbedox2, albedo);
            if (gloss)
                mat.SetTexture(_ALGloss, gloss);
            if (normal)
                mat.SetTexture(_ALDetailNormal, normal);
        }
    }
}