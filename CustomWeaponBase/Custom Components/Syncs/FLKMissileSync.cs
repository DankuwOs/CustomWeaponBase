using VTNetworking;


public class FLKMissileSync : VTNetSyncRPCOnly
{
    public CWBFLKMissile flkMissile;
    
    protected override void OnNetInitialized()
    {
        base.OnNetInitialized();
        if (netEntity == null)
            return;
        
        flkMissile.OnFire.AddListener(OnFire);
        flkMissile.OnEmpty.AddListener(OnEmpty);
    }

    private void OnFire()
    {
        if (!isMine)
            return;
        
        SendRPC("FLKM_OnFire");
    }
    
    private void OnEmpty()
    {
        if (!isMine)
            return;
        
        SendRPC("FLKM_OnEmpty");
    }

    [VTRPC]
    private void FLKM_OnFire()
    {
        flkMissile.OnFire.Invoke();
    }
    
    [VTRPC]
    private void FLKM_OnEmpty()
    {
        flkMissile.OnEmpty.Invoke();
    }
}