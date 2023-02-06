using Steamworks;
using UnityEngine;
using VTNetworking;


public class CWB_GrabInteractableSync : VTNetSyncRPCOnly
{
    public CWB_GrabInteractable grabInteractable;
    
    protected override void OnNetInitialized()
    {
        base.OnNetInitialized();
        
        if (base.isMine)
        {
            grabInteractable.OnSetPosition += GrabInteractable_OnSetPosition;
        }
        VTNetworkManager.instance.OnNewClientConnected += this.Instance_OnNewClientConnected;
    }

    public virtual void OnDestroy()
    {
        if (VTNetworkManager.hasInstance)
        {
            VTNetworkManager.instance.OnNewClientConnected -= this.Instance_OnNewClientConnected;
        }
    }

    public virtual void Instance_OnNewClientConnected(SteamId obj)
    {
        var position = VTMapManager.WorldToGlobalPoint(grabInteractable.transform.position);
        base.SendDirectedRPC(obj, "GrabInteractable_SetPosition", position, grabInteractable.transform.rotation);
    }

    public virtual void GrabInteractable_OnSetPosition(CWB_GrabInteractable.ObjectPosition objectPosition)
    {
        base.SendRPC("GrabInteractable_SetPosition", objectPosition);
    }

    [VTRPC]
    public virtual void GrabInteractable_SetPosition(CWB_GrabInteractable.ObjectPosition objectPosition)
    {
        Debug.Log($"[CWB Grab Int Sync]: Recieved RPC! Position: {objectPosition.position} | Rotation: {objectPosition.rotation}");
        grabInteractable.RemoteSetPosition(objectPosition.position, objectPosition.rotation);
    }
}