
using CustomWeaponBase.CWB_Utils;
using Harmony;
using UnityEngine;
using VTNetworking;

public class CWB_VRInteractableSync : VTNetSyncRPCOnly
{
    public VRInteractable interactable;

    public CWB_VRIntGlovePoser vrIntGlovePoser;

    private bool _isRightController;

    protected override void OnNetInitialized()
    {
        base.OnNetInitialized();

        if (isMine)
        {
            if (!interactable)
                interactable = GetComponent<VRInteractable>();
            interactable.OnInteract.AddListener(OnInteract);
            interactable.OnStopInteract.AddListener(OnStopInteract);
            interactable.OnInteracting.AddListener(OnInteracting);

            if (!vrIntGlovePoser)
                vrIntGlovePoser = GetComponent<CWB_VRIntGlovePoser>();
        }
    }

    public void OnInteract()
    {
        if (isMine)
        {
            _isRightController = !interactable.activeController.isLeft;
            SendRPC("RPC_CWBOnInteract", _isRightController? 1 : 0, BDSteamClient.mySteamID);
            return;
        }
        interactable.activeController.ReleaseFromInteractable();
    }
    
    public void OnStopInteract()
    {
        if (isMine)
        {
            SendRPC("RPC_CWBOnStopInteract", BDSteamClient.mySteamID);
        }
    }
    
    public void OnInteracting()
    {
        if (isMine)
        {
            SendRPC("RPC_CWBOnInteracting");
        }
    }
    
    [VTRPC]
    public void RPC_CWBOnInteract(int isRight, ulong steamId)
    {
        if (!isMine)
        {
            var playerModelSync = PlayerModelSync.GetPlayerModel(steamId);
            if (vrIntGlovePoser)
                SetRemoteInteractable(playerModelSync, isRight == 1, interactable);
            else 
                playerModelSync.SetRemoteInteractable(isRight == 1, interactable);
            
            interactable.OnInteract.Invoke();
        }
    }
    
    [VTRPC]
    public void RPC_CWBOnStopInteract(ulong steamId)
    {
        if (!isMine)
        {
            var playerModelSync = PlayerModelSync.GetPlayerModel(steamId);
            if (vrIntGlovePoser)
                SetRemoteInteractable(playerModelSync, _isRightController, null);
            else 
                playerModelSync.SetRemoteInteractable(_isRightController, null);
            
            interactable.OnStopInteract.Invoke();
        }
    }
    
    [VTRPC]
    public void RPC_CWBOnInteracting()
    {
        if (!isMine)
        {
            interactable.OnInteracting.Invoke();
        }
    }

    public void SetRemoteInteractable(PlayerModelSync modelSync, bool rightHand, VRInteractable interactable)
    {
        var modelSyncTraverse = Traverse.Create(modelSync);
        if (rightHand)
        {
            modelSyncTraverse.Field("rhInt").SetValue(interactable);
            UpdateInteractable(modelSync, true);
            return;
        }

        modelSyncTraverse.Field("lhInt").SetValue(interactable);
        UpdateInteractable(modelSync, false);
    }

    public void UpdateInteractable(PlayerModelSync modelSync, bool rightHand)
    {
        var modelSyncTraverse = Traverse.Create(modelSync);
        
        if (rightHand)
        {
            var rhInt = modelSyncTraverse.Field("rhInt").GetValue<VRInteractable>();

            if (rhInt)
            {
                modelSyncTraverse.Field("interactingRight").SetValue(true);
                modelSync.remoteGloveR.SetLockTransform(vrIntGlovePoser.lockTransform);
                
                vrIntGlovePoser.SetGloveInteract(modelSync.remoteGloveR);
                return;
            }

            modelSyncTraverse.Field("interactingRight").SetValue(false);
            modelSync.remoteGloveR.ClearInteractPose();
            
            vrIntGlovePoser.ClearGloveInteract(modelSync.remoteGloveR);
            return;
        }
        
        var lhInt = modelSyncTraverse.Field("lhInt").GetValue<VRInteractable>();
        

        if (lhInt)
        {
            modelSyncTraverse.Field("interactingLeft").SetValue(true);
            modelSync.remoteGloveL.SetLockTransform(vrIntGlovePoser.leftLockTransform);
            
            vrIntGlovePoser.SetGloveInteract(modelSync.remoteGloveL);
            return;
        }

        modelSyncTraverse.Field("interactingLeft").SetValue(false);
        modelSync.remoteGloveL.ClearInteractPose();
        
        vrIntGlovePoser.ClearGloveInteract(modelSync.remoteGloveL);
    }
}