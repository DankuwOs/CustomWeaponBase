using UnityEngine;


public class HeadTrackObject : MonoBehaviour
{
    public ModuleTurret turret;
    
    public HPEquippable equip;

    private void LateUpdate()
    {
        if (equip.itemActivated)
        {
            if (!VRHead.instance)
                return;
            
            turret.AimToTarget(VRHead.instance.transform.position + VRHead.instance.transform.forward * 2000f, true,
                true, false);
        }
    }
}