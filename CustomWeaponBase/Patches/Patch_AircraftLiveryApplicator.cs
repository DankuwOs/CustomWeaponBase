using Harmony;
using UnityEngine;

[HarmonyPatch(typeof(AircraftLiveryApplicator), "ApplyLivery")]
public class Patch_AircraftLiveryApplicator
{
    public static void Postfix(Texture2D texture, AircraftLiveryApplicator __instance)
    {
        var liveryMesh = __instance.gameObject.AddComponent<TextureHolder>();
        liveryMesh.texture = texture;
        Debug.Log($"Livery Patch: {liveryMesh.texture.name}");
    }
}