using System;
using System.Collections;
using UnityEngine;

public class LiveryMesh : MonoBehaviour
{
    public Renderer[] liveryMeshs;

    public Texture2D _livery;

    private TextureHolder _holder;

    public void Start()
    {
        _holder = this.GetComponentInParent<TextureHolder>();
        if (!_holder)
        {
            Debug.Log("Texture holder null");
            return;
        }
        
        _livery = _holder.texture;
        StartCoroutine(ApplyMesh());
    }

    private IEnumerator ApplyMesh()
    {
        int i = 0;
        while (i < 30)
        {
            if (!_holder)
            {
                _holder = this.GetComponentInParent<TextureHolder>();
                if (!_holder)
                    yield return null;
                _livery = _holder.texture;
            }
            foreach (var renderer in liveryMeshs)
            {
                renderer.material.SetTexture("_DetailAlbedoMap", _livery);
                renderer.material.EnableKeyword("_DETAIL_MULX2");
            }

            i++;
            yield return new WaitForSeconds(1f);
        }
    }

}