using CustomWeaponBase;
using Harmony;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VTOLVR.Multiplayer;


[HarmonyPatch(typeof(VTMPScenarioSettings), "SetupScenarioSettings")]
public class Inject_GameModeSettingsMainMenu
{
    public static void Postfix(VTMPScenarioSettings __instance)
    {
        bool isIngame = VTOLMPUtils.IsMPAndHost();

        GameObject wmd = null;
        Transform wmdCheckmark = null;
        
        if (!isIngame)
        {
        
            GameObject settingsTemplate = __instance.iconsIndicator.transform.parent.gameObject;
            wmd = Object.Instantiate(settingsTemplate, settingsTemplate.transform.parent);
            wmd.SetActive(true);
            
            wmd.transform.localPosition = new Vector3(-1092,-408,0);
            wmdCheckmark = wmd.transform.Find(__instance.iconsIndicator.name);
        }
        else
        {
            GameObject settingsTemplate = __instance.lateJoinsIndicator.transform.parent.gameObject;
            wmd = Object.Instantiate(settingsTemplate, settingsTemplate.transform.parent);
            wmd.SetActive(true);

            wmd.transform.localPosition = new Vector3(-422, -247, 0);
            wmdCheckmark = wmd.transform.Find(__instance.lateJoinsIndicator.name);
        }

        

        Debug.Log("Try setup vrInt");
        VRInteractable vrInt = wmd.GetComponent<VRInteractable>();
        vrInt.interactableName = "Allow CWB WMDS";
        vrInt.OnInteract = new UnityEvent();
        vrInt.OnInteract.AddListener(delegate
        {
            Main.allowWMDS = !Main.allowWMDS;
            wmdCheckmark.gameObject.SetActive(Main.allowWMDS);
        });
        vrInt.enabled = true;
        vrInt.sqrRadius = 0.0121f;

        wmdCheckmark.gameObject.SetActive(Main.allowWMDS);

        wmd.GetComponentInChildren<Text>().text = "WMDS";
        Debug.Log("WMD setting made.");

        wmd.GetComponentInParent<VRPointInteractableCanvas>().RefreshInteractables();
    }
}