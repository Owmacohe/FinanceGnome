using System;
using System.Collections.Generic;
using System.IO;
using NativeFileBrowser;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FGManager : MonoBehaviour
{
    [Header("Splash Screen")]
    [SerializeField] GameObject splashScreen;
    [SerializeField] TMP_InputField newDatabaseName;
    [SerializeField] Button openRecentDatabase;
    [SerializeField] List<Button> splashScreenButtons;

    [Header("Transactions Screen")]
    [SerializeField] Transform transactionsParent;
    [SerializeField] FGTransactionController transactionPrefab;
    
    string defaultDatabaseName = "New Document";
    FGDatabase database;
    
    void Start()
    {
        database = new(defaultDatabaseName);
        CheckRecentDatabase();
    }
    
    #region Splash Screen
    
    #region New Database
    
    public void SetNewDatabaseName(string name)
    {
        name = FGUtils.FormatString(name.ToLower(), $"{FGUtils.ALPHANUMERIC} ");
                
        database.Name = string.IsNullOrEmpty(name) ? defaultDatabaseName : name;
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
            var fullPath = $"{path}/{database.Name}.fg";
            Save(fullPath);
            
            PlayerPrefs.SetString("recentPath", fullPath);
            CheckRecentDatabase();

            LoadTransactions();
        }
        
        Invoke(nameof(EnableSplashScreenButtons), 0.1f);
    }
    
    #endregion
    
    #region Open Recent

    void CheckRecentDatabase()
    {
        if (PlayerPrefs.HasKey("recentPath"))
        {
            var path = PlayerPrefs.GetString("recentPath");
            
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                openRecentDatabase.gameObject.SetActive(false);
                return;
            }

            openRecentDatabase.gameObject.SetActive(true);
            
            var file = path.Substring(path.LastIndexOf('/') + 1);
            openRecentDatabase.GetComponentInChildren<TMP_Text>().text = $"Open Recent - <i>{file}</i>";
        }
        else openRecentDatabase.gameObject.SetActive(false);
    }

    public void OpenRecent()
    {
        Debug.Log("Opening recent database...");
        
        if (PlayerPrefs.HasKey("recentPath"))
            Load(PlayerPrefs.GetString("recentPath"));
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
            Load(path);
            
            PlayerPrefs.SetString("recentPath", path);
            CheckRecentDatabase();
        }
        
        Invoke(nameof(EnableSplashScreenButtons), 0.1f);
    }
    
    #endregion
    
    void DisableSplashScreenButtons() => splashScreenButtons.ForEach(button => button.interactable = false);
    void EnableSplashScreenButtons() => splashScreenButtons.ForEach(button => button.interactable = true);
    
    #endregion
    
    #region Transations Screen

    void LoadTransactions()
    {
        splashScreen.SetActive(false);
        
        for (int i = 0; i < database.Entries.Count; i++)
            Instantiate(transactionPrefab, transactionsParent).Initialize(i+1, database.Entries[i]);
    }
    
    #endregion
    
    #region IO

    void Save(string path)
    {
        Debug.Log("Saving...");
        
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Path is null");
            return;
        }
        
        if (database == null)
        {
            Debug.LogError("Database is null");
            return;
        }
        
        File.WriteAllText(path, database.ToString());
        Debug.Log($"Saved {database.Name} to {path}");
    }
    
    void Load(string path)
    {
        Debug.Log("Loading...");
        
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Path is null");
            return;
        }
        
        if (!File.Exists(path))
        {
            Debug.LogError("File does not exist");
            return;
        }
        
        var file = path.Substring(path.LastIndexOf('/') + 1);
        database = new FGDatabase(file.Substring(0, file.Length - 3), File.ReadAllText(path));
        
        Debug.Log($"Loaded {database.Name} from {path}");
        
        LoadTransactions();
    }
    
    #endregion
}