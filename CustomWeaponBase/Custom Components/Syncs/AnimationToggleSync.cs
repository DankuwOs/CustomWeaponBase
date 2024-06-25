using HarmonyLib;
using UnityEngine;
using VTNetworking;


public class AnimationToggleSync : VTNetSyncRPCOnly
{
    public AnimationToggle Toggle;

    private bool _lastDeployed;

    private Traverse toggleTraverse;

    public override void OnNetInitialized()
    {
        base.OnNetInitialized();
        
        if (netEntity == null)
            Debug.Log("[AnimationToggleSync]: No net entity attached!");

        if (!Toggle)
        {
            Toggle = GetComponentInChildren<AnimationToggle>();
        }
        
        toggleTraverse = Traverse.Create(Toggle);
        
        _lastDeployed = (bool)toggleTraverse.Field("deployed").GetValue();
    }

    private void FixedUpdate()
    {
        if (!isMine)
            return;

        bool deployed = (bool)toggleTraverse.Field("deployed").GetValue();
        if (deployed != _lastDeployed)
        {
            _lastDeployed = deployed;
            SendRPC("AnimationToggleSyncToggle", _lastDeployed ? 1 : 0);
        }
    }

    [VTRPC]
    public void AnimationToggleSyncToggle(int toggle)
    {
        if (isMine)
            return;

        bool toggled = toggle == 1;
        
        if (toggled != (bool)toggleTraverse.Field("deployed").GetValue())
        {
            _lastDeployed = toggled;
            Toggle.Toggle();
        }
    }
}