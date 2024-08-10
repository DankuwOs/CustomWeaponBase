using HarmonyLib;
using UnityEngine;
using VTNetworking;


public class AnimationToggleSync : VTNetSyncRPCOnly
{
    public AnimationToggle Toggle;

    private bool _lastDeployed;

    public override void OnNetInitialized()
    {
        base.OnNetInitialized();
        
        if (netEntity == null)
            Debug.Log("[AnimationToggleSync]: No net entity attached!");

        if (!Toggle)
        {
            Toggle = GetComponentInChildren<AnimationToggle>();
        }
        
        _lastDeployed = Toggle.deployed;
    }

    private void FixedUpdate()
    {
        if (!isMine)
            return;

        var deployed = Toggle.deployed;
        
        if (deployed == _lastDeployed) return;
        
        _lastDeployed = deployed;
        SendRPC("AnimationToggleSyncToggle", _lastDeployed ? 1 : 0);
    }

    [VTRPC]
    public void AnimationToggleSyncToggle(int toggle)
    {
        if (isMine)
            return;

        var toggled = toggle == 1;

        if (toggled == Toggle.deployed) return;
        
        _lastDeployed = toggled;
        Toggle.Toggle();
    }
}