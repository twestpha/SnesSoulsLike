#if UNITY_EDITOR

using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;

// https://discussions.unity.com/t/how-can-i-add-copy-paste-clipboard-support-to-my-game/44249/2
public class ClipboardHelper {
    public static string clipboard {
        get {
            return GUIUtility.systemCopyBuffer;
        }
        set {
            GUIUtility.systemCopyBuffer = value;
        }
    }
}

[ExecuteInEditMode]
public class LocalizationMenuItems : MonoBehaviour
{
    private static LocalizationMenuItems instance;
    
    void Update(){
        // Iunno this works more consistently
        instance = this;
    }
    
    private const string DEFAULT_EMPTY_LOC = "???";
    private const string ENUM_LOC = "enm_";
    
    [MenuItem("Localization/Force Refresh Localization", false, 0)]
    public static void RefreshLocalization(){
        Localizer.ForceRefresh();
    }
        
    [MenuItem("Localization/Scrape Localization", false, 0)]
    public static void ScrapeLocalization(){
        instance.StartCoroutine(instance.ScrapeLocalizationCoroutine());
    }
    
    private IEnumerator ScrapeLocalizationCoroutine(){
        // Force assets to save and refresh because unity forgets to do that a _lot_
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Dictionary<string, string> entries = new();

        // Get all the enums in the game
        GetEnumLocs<DungeonName>(entries, "dungeon_");
        
        // Get serializable data
        GetItemLocs(entries);
        
        // Get in-scene data
        GetInteractLocs(entries);
        
        // Write results out both to log and clipboard
        string result = "";
        foreach(KeyValuePair<string, string> kvp in entries){
            result += kvp.Key + "\t" + kvp.Value + "\n";
        }
        
        Debug.Log(result);
        ClipboardHelper.clipboard = result;
        
        yield break;
    }
    
    public static void GetEnumLocs<T>(Dictionary<string, string> entries, string prepend){
        foreach(T t in Enum.GetValues(typeof(T))){
            string enumName = t.ToString().ToLower();
    
            if(enumName != "count"){
                string locKey = prepend + enumName;
                string locValue = Localizer.Localize(locKey);
    
                if(locValue.Contains(Localizer.MISSING_LOCALIZATION)){
                    entries[locKey] = DEFAULT_EMPTY_LOC;
                } else {
                    entries[locKey] = locValue;
                }
            }
        }
    }
    
    public static void GetItemLocs(Dictionary<string, string> entries){
        
        // Also include recieving format text
        {
            string locKey = InventoryComponent.ITEM_RECEIVE_LOC;
            string locValue = Localizer.Localize(locKey);

            if(locValue.Contains(Localizer.MISSING_LOCALIZATION)){
                entries[locKey] = DEFAULT_EMPTY_LOC;
            } else {
                entries[locKey] = locValue;
            }
        }
        
        ItemData[] allItems = Resources.FindObjectsOfTypeAll<ItemData>();
    
        for(int i = 0, count = allItems.Length; i < count; ++i){
            {
                string locKey = allItems[i].nameLoc;
                string locValue = Localizer.Localize(locKey);
    
                if(locValue.Contains(Localizer.MISSING_LOCALIZATION)){
                    entries[locKey] = DEFAULT_EMPTY_LOC;
                } else {
                    entries[locKey] = locValue;
                }
            }
            {
                string locKey = allItems[i].pluralNameLoc;
                string locValue = Localizer.Localize(locKey);
    
                if(locValue.Contains(Localizer.MISSING_LOCALIZATION)){
                    entries[locKey] = DEFAULT_EMPTY_LOC;
                } else {
                    entries[locKey] = locValue;
                }
            }
            {
                string locKey = allItems[i].descLoc;
                string locValue = Localizer.Localize(locKey);
    
                if(locValue.Contains(Localizer.MISSING_LOCALIZATION)){
                    entries[locKey] = DEFAULT_EMPTY_LOC;
                } else {
                    entries[locKey] = locValue;
                }
            }
        }
    }
    
    public static void GetInteractLocs(Dictionary<string, string> entries){
        
        // Also include "leave"
        {
            string locKey = InteractComponent.LEAVE_LOC;
            string locValue = Localizer.Localize(locKey);

            if(locValue.Contains(Localizer.MISSING_LOCALIZATION)){
                entries[locKey] = DEFAULT_EMPTY_LOC;
            } else {
                entries[locKey] = locValue;
            }
        }
        
        // All interacts
        InteractComponent[] allInteracts = FindObjectsOfType<InteractComponent>(true);
        
        for(int i = 0, icount = allInteracts.Length; i < icount; ++i){
            InteractComponent targetInteract = allInteracts[i];
            
            for(int j = 0, jcount = targetInteract.options.Length; j < jcount; ++j){
                InteractOption option = targetInteract.options[j];

                if(!String.IsNullOrEmpty(option.optionLocText)){
                    string locKey = option.optionLocText;
                    string locValue = Localizer.Localize(locKey);
        
                    if(locValue.Contains(Localizer.MISSING_LOCALIZATION)){
                        entries[locKey] = DEFAULT_EMPTY_LOC;
                    } else {
                        entries[locKey] = locValue;
                    }
                }
                
                if(!String.IsNullOrEmpty(option.messageLocText)){
                    string locKey = option.messageLocText;
                    string locValue = Localizer.Localize(locKey);
        
                    if(locValue.Contains(Localizer.MISSING_LOCALIZATION)){
                        entries[locKey] = DEFAULT_EMPTY_LOC;
                    } else {
                        entries[locKey] = locValue;
                    }
                }
            }
        }
    }
    
    // public static void GetUILocs(Dictionary<string, string> entries){
    //     LocalizeMeComponent[] allItems = FindObjectsOfType<LocalizeMeComponent>();
    // 
    //     for(int i = 0, count = allItems.Length; i < count; ++i){
    //         LocalizeMeComponent l = allItems[i];
    // 
    //         if(l.locType == LocalizationType.Static){
    //             Text t = l.GetComponent<Text>();
    // 
    //             if(!string.IsNullOrEmpty(t.text)){
    //                 string locKey = t.text;
    //                 string locValue = Localizer.Localize(locKey);
    // 
    //                 if(locValue.Contains(Localizer.MISSING_LOCALIZATION)){
    //                     entries[locKey] = DEFAULT_EMPTY_LOC;
    //                 } else {
    //                     entries[locKey] = locValue;
    //                 }
    //             }
    //         } else if(l.locType == LocalizationType.Formatted){
    //             if(!string.IsNullOrEmpty(l.format)){
    //                 string locKey = l.format;
    //                 string locValue = Localizer.Localize(locKey);
    // 
    //                 if(locValue.Contains(Localizer.MISSING_LOCALIZATION)){
    //                     entries[locKey] = DEFAULT_EMPTY_LOC;
    //                 } else {
    //                     entries[locKey] = locValue;
    //                 }
    //             }
    //         }
    //     }
    // }

    private const string localizationSourceUrl = "https://docs.google.com/spreadsheets/d/1nQ4XPPsTl2nYBRtqj78Lv153h_m-ZVs7N07X2Ao9TUw/export?format=tsv";
    
    [MenuItem("Localization/Download Localization", false, 1)]
    public static void DownloadLocalization(){
        instance.StartCoroutine(instance.GetRequest(localizationSourceUrl));
    }
    
    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    File.WriteAllText(Application.dataPath + "\\Data\\Localization\\localization.txt", webRequest.downloadHandler.text);
                    Debug.Log("Successfully downloaded localization sheet and saved as localization.txt");
                    break;
            }
        }
    }
}

#endif // UNITY_EDITOR