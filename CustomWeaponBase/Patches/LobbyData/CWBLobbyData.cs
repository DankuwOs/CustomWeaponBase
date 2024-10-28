using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CheeseMods.VTOLTaskProgressUI;
using Cysharp.Threading.Tasks;
using HarmonyLib;
using SteamQueries.Models;
using UnityEngine;
using UnityEngine.Serialization;
using Valve.Newtonsoft.Json;
using VTOLAPI;
using VTOLVR.Multiplayer;

namespace CustomWeaponBase.Patches.LobbyData;

public class CWBLobbyData
{
    [Serializable]
    public struct PackData
    {
        public string itemTitle;
        public string packName;
        public ulong packId;
    }
    
    public static string GetSerializedPackData()
    {
        var packDataList = new List<PackData>();
        
        foreach (var cwbPack in Main.instance.cwbPacks)
        {
            var packData = new PackData
            {
                itemTitle = cwbPack.item.Title,
                packName = cwbPack.name,
                packId = cwbPack.item.PublishFieldId
            };
            packDataList.Add(packData);
        }

        var packsData = JsonConvert.SerializeObject(packDataList, Formatting.Indented);
        
        Debug.Log($"[CWB INFO]: Serialized pack data\nDATA:\n{packsData}");
        
        return packsData;
    }

    public static List<PackData> DeserializePackData(string data)
    {
        Debug.Log($"[CWB INFO]: Recieved pack data\nDATA:\n{data}");
        var packDataList = JsonConvert.DeserializeObject<List<PackData>>(data);

        if (packDataList == null)
        {
            Debug.Log($"[CWB INFO]: Lobby has no pack data, returning empty list.");
            return new List<PackData>();
        }
        
        Debug.Log($"[CWB INFO]: Deserialized pack data");
        for (var index = 0; index < packDataList.Count; index++)
        {
            var pData = packDataList[index];
            Debug.Log($"[CWB INFO]: pData[{index}]: Title: '{pData.itemTitle}', Name: '{pData.packName}', PFieldID: '{pData.packId}'");
        }

        return packDataList;
    }

    public static async UniTask<bool> LoadPacksForLobby(List<PackData> packDataList)
    {
        int selection = 0;
        ControllerEventHandler.UnpauseEvents();
        
        Debug.Log($"[CWB INFO]: Showing MP pack sync confirmation");
        VTMPMainMenu.instance.confirmUI.DisplayConfirmation("Weapon Packs", "Joining this lobby may enable, disable, or download packs.", () => selection = 1, () => selection = 2);

        await UniTask.WaitUntil(() => selection != 0);

        if (selection == 2)
            return false;
        
        VTMPMainMenu.instance.ShowError("Loading packs for lobby, you will join once it's done.");
        var steamItems = Main.instance.AllItems;

        var syncTask = VTOLTaskProgressManager.RegisterTask(Main.instance, "Syncing Packs With Lobby");
        
        // Could probably load all of them at the same time but its easier to just not atm.
        for (var index = 0; index < packDataList.Count; index++)
        {
            syncTask.SetProgress((float)index / packDataList.Count);
            
            var packData = packDataList[index];
            
            var item = steamItems.FirstOrDefault(p =>
                p.Title == packData.itemTitle || p.PublishFieldId == packData.packId);
            
            
            
            // Try to get steam item from the workshop if it isnt already downloaded
            if (item == null)
            {
                if (packData.packId <= 0)
                {
                    Debug.Log($"[CWB WARN]: PublishFieldId is not set for '{packData.itemTitle}', the host is likely using a locally installed pack");
                    continue;
                }
                
                Debug.Log($"[CWB INFO]: Getting item '{packData.itemTitle}' 'https://steamcommunity.com/sharedfiles/filedetails/?id={packData.packId}'");
                var getPackItem = await ModLoader.SteamQuery.SteamQueries.Instance.GetItem(packData.packId);
                if (getPackItem.Items.Count == 0)
                {
                    Debug.Log($"[CWB ERROR]: Couldn't get item, check if 'https://steamcommunity.com/sharedfiles/filedetails/?id={packData.packId}' is available.");
                    continue;
                }

                item = getPackItem.Items[0];
            }

            if (item == null) continue;
            
            if (!item.IsInstalled)
            {
                Debug.Log($"[CWB INFO]: Downloading item '{packData.itemTitle}' 'https://steamcommunity.com/sharedfiles/filedetails/?id={packData.packId}'");
                await ModLoader.SteamQuery.SteamQueries.Instance.DownloadItem(item.PublishFieldId);
                
                var getItem = await ModLoader.SteamQuery.SteamQueries.Instance.GetItem(item.PublishFieldId);
                if (getItem.Items.Count == 0)
                {
                    Debug.Log($"[CWB ERROR]: Couldn't get item 'https://steamcommunity.com/sharedfiles/filedetails/?id={item.PublishFieldId}' after downloading?");
                }

                item = getItem.Items[0];
            }
            
            if (TryGetCWBFiles(item, out var cwbFiles))
            {
                foreach (var fileInfo in cwbFiles)
                {
                    if (packData.packName == fileInfo.Name)
                        await Main.instance.LoadPackRoutine(fileInfo, item);
                }
            }
        }

        var myPacks = Main.instance.myCWBPacks.ToArray();
        foreach (var origPack in myPacks.Where(origPack => packDataList.All(p => p.packName != origPack.name)))
        {
            Main.instance.UnloadPack(origPack);
        }
        
        syncTask.FinishTask(); 
        return true;
    }

    
    private static bool TryGetCWBFiles(SteamItem item, out List<FileInfo> cwbFiles)
    {
        cwbFiles = new List<FileInfo>();
        
        if (item.Directory == null)
        {
            Debug.Log($"[CWB ERROR]: Couldn't get item directory for 'https://steamcommunity.com/sharedfiles/filedetails/?id={item.PublishFieldId}', check if it exists.");
            return false;
        }
        
        var files = Directory.GetFiles(item.Directory, "*.cwb");
        
        if (files.Length == 0)
            return false;
        
        cwbFiles.AddRange(files.Select(p => new FileInfo(p)));
        return true;
    }
}