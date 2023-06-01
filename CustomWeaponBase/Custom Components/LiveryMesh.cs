using UnityEngine;


public class LiveryMesh : MonoBehaviour
{
    [Header("PUT AN EMPTY TEXTURE IN YOUR DETAIL ALBEDOx2")] 
    [Tooltip("All the meshes that should have the planes livery applied.")]
    public Renderer[] liveryMeshs;

    [Tooltip("Copies entire material specified in materialPath")]
    public bool copyMaterial;

    [Tooltip("Path to renderer containing material separated by forward slashes.")]
    public string materialPath;

    [Header("This is for my own shader don't use it unless you need it")]
    public bool useLivery;

    public string textureID = "_Livery";
}