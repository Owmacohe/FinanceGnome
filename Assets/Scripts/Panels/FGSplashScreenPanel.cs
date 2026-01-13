using System;
using System.Collections.Generic;
using System.IO;
using NativeFileBrowser;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FGSplashScreenPanel : MonoBehaviour
{
    [SerializeField] TMP_InputField newDatabaseName;
    [SerializeField] Button openRecentDatabase;
    [SerializeField] List<Button> splashScreenButtons;

    FGManager manager;

    public void Initialize()
    {
        manager = FGManager.Instance;
    }

    #region New Database
    
    public void SetNewDatabaseName(string name)
    {
        name = FGUtils.FormatString(name.ToLower(), $"{FGUtils.ALPHANUMERIC} ");
                
        manager.Database.Name = string.IsNullOrEmpty(name) ? FGManager.DefaultDatabaseName : name;
        newDatabaseName.SetTextWithoutNotify(name);
    }

    public void NewDatabase()
    {
        Debug.Log("Creating new database...");
        
        DisableSplashScreenButtons();
        
        var path = FileBrowser.PickFolder();
        
        if (string.IsNullOrEmpty(path)) Debug.LogError("Path is null");
        else
        {
            #if UNITY_STANDALONE_WIN
            var fullPath = $"{path}\\{manager.Database.Name}.fg";
            #else
            var fullPath = $"{path}/{manager.Database.Name}.fg";
            #endif
            
            manager.Save(fullPath);
            
            PlayerPrefs.SetString(FGManager.RECENT_PATH, fullPath);
            CheckRecentDatabase();

            manager.transactionsScreen.InstantiateTransactions();
            manager.balanceSheetScreen.InstantiateBalanceSheetCells();
            manager.importScreen.InstantiateImportRules();
            manager.SetBalanceSheet();
        }
        
        Invoke(nameof(EnableSplashScreenButtons), 0.1f);
    }
    
    #endregion
    
    #region Open Recent

    public void CheckRecentDatabase()
    {
        if (PlayerPrefs.HasKey(FGManager.RECENT_PATH))
        {
            var path = PlayerPrefs.GetString(FGManager.RECENT_PATH);
            
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                openRecentDatabase.gameObject.SetActive(false);
                return;
            }

            openRecentDatabase.gameObject.SetActive(true);
            
            #if UNITY_STANDALONE_WIN
            var file = path.Substring(path.LastIndexOf('\\') + 1);
            #else
            var file = path.Substring(path.LastIndexOf('/') + 1);
            #endif
            
            openRecentDatabase.GetComponentInChildren<TMP_Text>().text = $"Open Recent - <i>{file}</i>";
        }
        else openRecentDatabase.gameObject.SetActive(false);
    }

    public void OpenRecent()
    {
        Debug.Log("Opening recent database...");
        
        if (PlayerPrefs.HasKey(FGManager.RECENT_PATH))
            manager.Load(PlayerPrefs.GetString(FGManager.RECENT_PATH), true);
    }
    
    #endregion
    
    #region Open Database

    public void OpenDatabase()
    {
        Debug.Log("Opening database...");
        
        DisableSplashScreenButtons();
        
        var path = FileBrowser.PickFile();
        
        if (string.IsNullOrEmpty(path)) Debug.LogError("Path is null");
        else if (!path.Substring(path.IndexOf('.')).Equals(".fg")) Debug.LogError("File is not of a valid file type");
        else
        {
            manager.Load(path, true);
            CheckRecentDatabase();
        }
        
        Invoke(nameof(EnableSplashScreenButtons), 0.1f);
    }
    
    #endregion
    
    void DisableSplashScreenButtons() => splashScreenButtons.ForEach(button => button.interactable = false);
    void EnableSplashScreenButtons() => splashScreenButtons.ForEach(button => button.interactable = true);
}