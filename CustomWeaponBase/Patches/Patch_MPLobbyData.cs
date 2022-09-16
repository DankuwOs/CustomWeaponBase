using CustomWeaponBase;
using Harmony;
using VTOLVR.Multiplayer;

[HarmonyPatch(typeof(VTMPMainMenu), "LaunchMPGameForScenario")]
public class Custom_LobbyData1
{
    public static void Postfix()
    {
        if (VTOLMPLobbyManager.isLobbyHost)
            VTOLMPLobbyManager.currentLobby.SetData("WMD", (Main.allowWMDS ? 1 : 0).ToString());
        else if (VTOLMPLobbyManager.isInLobby)
        {
            string wmdAllowed = VTOLMPLobbyManager.currentLobby.GetData("WMD");
            Main.allowWMDS = int.Parse(wmdAllowed) != 0;
        }
    }
}

[HarmonyPatch(typeof(VTMPMainMenu), nameof(VTMPMainMenu.TransitionToNewMission))]
public class Custom_LobbyData2
{
    public static void Postfix()
    {
        if (VTOLMPLobbyManager.isLobbyHost)
            VTOLMPLobbyManager.currentLobby.SetData("WMD", (Main.allowWMDS ? 1 : 0).ToString());
        else if (VTOLMPLobbyManager.isInLobby)
        {
            string wmdAllowed = VTOLMPLobbyManager.currentLobby.GetData("WMD");
            Main.allowWMDS = int.Parse(wmdAllowed) != 0;
        }
    }
}

[HarmonyPatch(typeof(VTOLMPBriefingRoomUI), nameof(VTOLMPBriefingRoomUI.NG_StartButton))]
public class Custom_LobbyData3
{
    public static void Postfix()
    {
        if (VTOLMPLobbyManager.isLobbyHost)
            VTOLMPLobbyManager.currentLobby.SetData("WMD", (Main.allowWMDS ? 1 : 0).ToString());
        else if (VTOLMPLobbyManager.isInLobby)
        {
            string wmdAllowed = VTOLMPLobbyManager.currentLobby.GetData("WMD");
            Main.allowWMDS = int.Parse(wmdAllowed) != 0;
        }
    }
}