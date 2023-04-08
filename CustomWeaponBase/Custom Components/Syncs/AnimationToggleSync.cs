using Harmony;
using UnityEngine;
using VTNetworking;


public class AnimationToggleSync : VTNetSyncRPCOnly
{
    public AnimationToggle Toggle;

    private bool _lastDeployed;

    private Traverse toggleTraverse;

    protected override void OnNetInitialized()
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
    public void AnimationToggleSyncToggle(bool toggle)
    {
        if (isMine)
            return;
        
        if (toggle != (bool)toggleTraverse.Field("deployed").GetValue())
        {
            _lastDeployed = toggle;
            Toggle.Toggle();
        }
    }
}