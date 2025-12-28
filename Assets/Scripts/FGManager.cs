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
    [SerializeField] GameObject transactionsScreen;
    [SerializeField] Transform transactionsParent;
    [SerializeField] FGTransactionController transactionPrefab;

    List<FGTransactionController> transactions = new();
    
    string defaultDatabaseName = "New Document";
    public static FGDatabase Database;

    const string RECENT_PATH = "recentPath";

    int addEntriesAmount = 1;
    
    void Start()
    {
        Database = new(defaultDatabaseName);
        CheckRecentDatabase();
    }
    
    #region Splash Screen
    
    #region New Database
    
    public void SetNewDatabaseName(string name)
    {
        name = FGUtils.FormatString(name.ToLower(), $"{FGUtils.ALPHANUMERIC} ");
                
        Database.Name = string.IsNullOrEmpty(name) ? defaultDatabaseName : name;
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
            var fullPath = $"{path}/{Database.Name}.fg";
            Save(fullPath);
            
            PlayerPrefs.SetString(RECENT_PATH, fullPath);
            CheckRecentDatabase();

            InstantiateTransactions();
        }
        
        Invoke(nameof(EnableSplashScreenButtons), 0.1f);
    }
    
    #endregion
    
    #region Open Recent

    void CheckRecentDatabase()
    {
        if (PlayerPrefs.HasKey(RECENT_PATH))
        {
            var path = PlayerPrefs.GetString(RECENT_PATH);
            
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
        
        if (PlayerPrefs.HasKey(RECENT_PATH))
            Load(PlayerPrefs.GetString(RECENT_PATH), true);
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
            Load(path, true);
            
            PlayerPrefs.SetString(RECENT_PATH, path);
            CheckRecentDatabase();
        }
        
        Invoke(nameof(EnableSplashScreenButtons), 0.1f);
    }
    
    #endregion
    
    void DisableSplashScreenButtons() => splashScreenButtons.ForEach(button => button.interactable = false);
    void EnableSplashScreenButtons() => splashScreenButtons.ForEach(button => button.interactable = true);
    
    #endregion
    
    #region Transations Screen

    void InstantiateTransactions()
    {
        splashScreen.SetActive(false);
        transactionsScreen.SetActive(true);

        for (int i = 0; i < Database.Entries.Count; i++)
            AddTransaction(Database.Entries[i], i + 1);
    }

    void AddTransaction(FGEntry entry, int lineNumber)
    {
        var transaction = Instantiate(transactionPrefab, transactionsParent);
        transaction.Initialize(lineNumber, entry, () => Save());
        transactions.Add(transaction);
        
        transaction.OnRemove += entry =>
        {
            Database.Entries.Remove(entry);
            Save();
            
            for (int i = transactions.IndexOf(transaction); i < transactions.Count; i++)
                transactions[i].ModifyLineNumber(-1);

            transactions.Remove(transaction);
            Destroy(transaction.gameObject);
        };
    }

    public void SetAddEntriesAmount(string amount) =>
        addEntriesAmount = !string.IsNullOrEmpty(amount) && int.Parse(amount) > 0 ? int.Parse(amount) : 1;

    public void AddEntries()
    {
        for (int i = 0; i < addEntriesAmount; i++)
        {
            var entry = new FGEntry(DateTime.Today);
            Database.Entries.Add(entry);
            AddTransaction(entry, Database.Entries.Count);
        }
        
        Save();
    }
    
    #endregion
    
    #region IO

    void Save(string path = null)
    {
        if (path == null) path = PlayerPrefs.GetString(RECENT_PATH);
        
        Debug.Log("Saving...");
        
        if (string.IsNullOrEmpty(path))
        {
            Debug.LogError("Path is null");
            return;
        }
        
        if (Database == null)
        {
            Debug.LogError("Database is null");
            return;
        }
        
        File.WriteAllText(path, Database.ToString());
        Debug.Log($"Saved {Database.Name} to {path}");
    }
    
    void Load(string path, bool thenSave = false)
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
        Database = new FGDatabase(file.Substring(0, file.Length - 3), File.ReadAllText(path));
        
        Debug.Log($"Loaded {Database.Name} from {path}");
        
        InstantiateTransactions();
        
        if (thenSave) Save(path);
    }
    
    #endregion
}