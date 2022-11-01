using System;
using System.Collections;
using System.Configuration;
using System.Threading.Tasks;
using UnityEngine;

public class LiveryMesh : MonoBehaviour
{
    [Header("PUT AN EMPTY TEXTURE IN YOUR DETAIL ALBEDOx2")] [Tooltip("All the meshes that should have the planes livery applied.")]
    public Renderer[] liveryMeshs;
}