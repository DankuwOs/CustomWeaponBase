using UnityEngine;


public class LiveryMesh : MonoBehaviour
{
    [Header("PUT AN EMPTY TEXTURE IN YOUR DETAIL ALBEDOx2")] [Tooltip("All the meshes that should have the planes livery applied.")]
    public Renderer[] liveryMeshs;

    [Tooltip("Copies entire material, used for cases where the weapon manager doesn't have a livery sample and you want skins to work with it.")]
    public bool copyMaterial;

    [Tooltip("Path to renderer containing material separated by forward slashes.")]
    public string materialPath;
}