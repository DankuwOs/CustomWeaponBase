using System;
using System.Collections;
using System.Collections.Generic;
using CustomWeaponBase;
using CustomWeaponBase.Patches.LobbyData;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using VTOLVR.Multiplayer;


[HarmonyPatch(typeof(VTMPMainMenu))]
public class CWB_VTMPMainMenu_LobbyData
{
    [HarmonyPatch(nameof(VTMPMainMenu.LaunchMPGameForScenario))]
    [HarmonyPostfix]
    public static void Patch_LaunchMPGameForScenario()
    {
        if (VTOLMPLobbyManager.isLobbyHost)
        {
            VTOLMPLobbyManager.currentLobby.SetData("WMD", (Main.allowWMDS ? 1 : 0).ToString());

            var serializedPackData = CWBLobbyData.GetSerializedPackData();
            VTOLMPLobbyManager.currentLobby.SetData("CWB_Packs", serializedPackData);
        }
        else if (VTOLMPLobbyManager.isInLobby)
        {
            var wmdAllowed = VTOLMPLobbyManager.currentLobby.GetData("WMD");
            Main.allowWMDS = int.Parse(wmdAllowed) != 0;
        }
    }
    
    [HarmonyPatch(nameof(VTMPMainMenu.TransitionToNewMission))]
    [HarmonyPostfix]
    public static void Patch_TransitionToNewMission()
    {
        if (VTOLMPLobbyManager.isLobbyHost)
        {
            VTOLMPLobbyManager.currentLobby.SetData("WMD", (Main.allowWMDS ? 1 : 0).ToString());

            var serializedPackData = CWBLobbyData.GetSerializedPackData();
            VTOLMPLobbyManager.currentLobby.SetData("CWB_Packs", serializedPackData);
        }
        else if (VTOLMPLobbyManager.isInLobby)
        {
            var wmdAllowed = VTOLMPLobbyManager.currentLobby.GetData("WMD");
            Main.allowWMDS = int.Parse(wmdAllowed) != 0;
        }
    }

    [HarmonyPatch(nameof(VTMPMainMenu.JoinLobbyRoutine))]
    [HarmonyPostfix]
    public static void Patch_JoinLobbyRoutine(ref IEnumerator __result)
    {
        var original = __result;
        
        IEnumerator Prefix()
        {
            Main.wasInMP = true;
            Main.instance.myCWBPacks = Main.instance.cwbPacks;
            
            var serializedPackData = VTOLMPLobbyManager.currentLobby.GetData("CWB_Packs");
            var packData = CWBLobbyData.DeserializePackData(serializedPackData);
            bool join = false;
            yield return CWBLobbyData.LoadPacksForLobby(packData).ToCoroutine(b => join = b);
            if (join == true)
                yield return original;
            else
            {
                VTOLMPLobbyManager.LeaveLobby();
                ControllerEventHandler.UnpauseEvents();
            }
        }

        __result = Prefix();
    }
}

[HarmonyPatch(typeof(VTOLMPBriefingRoomUI))]
public class CWB_VTOLMPBriefingRoomUI_LobbyData
{
    [HarmonyPatch(nameof(VTOLMPBriefingRoomUI.NG_StartButton))]
    [HarmonyPostfix]
    public static void Patch_NGStartButton()
    {
        if (VTOLMPLobbyManager.isLobbyHost)
        {
            VTOLMPLobbyManager.currentLobby.SetData("WMD", (Main.allowWMDS ? 1 : 0).ToString());

            var serializedPackData = CWBLobbyData.GetSerializedPackData();
            VTOLMPLobbyManager.currentLobby.SetData("CWB_Packs", serializedPackData);
        }
        else if (VTOLMPLobbyManager.isInLobby)
        {
            var wmdAllowed = VTOLMPLobbyManager.currentLobby.GetData("WMD");
            Main.allowWMDS = int.Parse(wmdAllowed) != 0;
        }
    }
}