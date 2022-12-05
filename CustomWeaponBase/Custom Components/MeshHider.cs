using UnityEngine;

public class MeshHider : MonoBehaviour
{
    
    public string[] hiddenMeshs;

    public bool hideSubMeshs = true;

    private HPEquippable _equippable;

    private void Awake()
    {
        OnEquipped();
    }

    private void OnEquipped()
    {
        Debug.Log($"[Mesh Hider]: OnEquipped");
        var tf = transform.root;
        if (!tf.GetComponent<WeaponManager>())
            return;
        
        foreach (var s in hiddenMeshs)
        {
            var transform = tf;
            
            string[] subStrings = s.Split('/');
            
            if (subStrings.Length == 0) return;
            
            foreach (var subString in subStrings)
            {
                var tranny = transform.Find(subString);
                if (!tranny)
                {
                    Debug.Log($"[MeshHider]: Couldn't find {subString} in {transform}");
                    break;
                }
                transform = tranny;
            }
            
            if (hideSubMeshs)
            {
                var meshs = transform.GetComponentsInChildren<Renderer>();
                foreach (var renderer in meshs)
                {
                    Debug.Log($"Turning off {renderer.gameObject.name}");
                    renderer.enabled = false;
                }
            }
            else
            {
                var mesh = transform.GetComponent<Renderer>();
                Debug.Log($"Turning off {mesh.gameObject.name}");
                mesh.enabled = false;
            }
        }
    }
}