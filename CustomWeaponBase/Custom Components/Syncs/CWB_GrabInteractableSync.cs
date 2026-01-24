using Steamworks;
using UnityEngine;
using VTNetworking;
using VTOLVR.Multiplayer;


public class CWB_GrabInteractableSync : VTNetSyncRPCOnly
{
    public CWB_GrabInteractable grabInteractable;

    [Header("Used for getting the multi user vehicle sync, ")]
    public HPEquippable hpEquippable;
    
    private MultiUserVehicleSync _muvs;

    public override void OnNetInitialized()
    {
        base.OnNetInitialized();

        hpEquippable ??= GetComponentInParent<HPEquippable>();
        _muvs = hpEquippable?.weaponManager?.muvs;

        grabInteractable.OnSetPosition += GrabInteractable_OnSetPosition;
        
        if (_muvs)
        {
            _muvs.OnOccupantEntered += MuvsOnOccupantEntered;
        }
    }

    private void MuvsOnOccupantEntered(int seatidx, ulong userid)
    {
        var position = VTMapManager.WorldToGlobalPoint(grabInteractable.transform.position);
        _muvs.SendRPCToCopilots(this, 0, position, grabInteractable.transform.rotation);
    }

    public virtual void Instance_OnNewClientConnected(SteamId obj)
    {
        var position = VTMapManager.WorldToGlobalPoint(grabInteractable.transform.position);
        base.SendDirectedRPC(obj, 0, position, grabInteractable.transform.rotation);
    }

    public virtual void GrabInteractable_OnSetPosition(CWB_GrabInteractable.ObjectPosition objectPosition)
    {
        base.SendRPC(0, objectPosition.position, objectPosition.rotation);
    }

    [VTRPC(0)]
    public virtual void GrabInteractable_SetPosition(Vector3D objectPosition, Quaternion objectRotation)
    {
        grabInteractable.RemoteSetPosition(objectPosition, objectRotation);
    }
}