using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NativeFileBrowser;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FGManager : MonoBehaviour
{
    public static FGManager Instance;

    [SerializeField] TMP_Text versionNumber;
    
    [Header("Top Bar")]
    [SerializeField] GameObject topRow;
    [SerializeField] List<Button> allButtons;
    [SerializeField] Button transactionsButton;
    [SerializeField] Button balanceSheetButton;
    [SerializeField] Button importButton;
    
    [Header("Panels")]
    [SerializeField] List<GameObject> allPanels;
    public FGSplashScreenPanel splashScreen;
    public FGBalanceSheetScreenPanel balanceSheetScreen;
    public FGTransactionsScreenPanel transactionsScreen;
    public FGImportScreenPanel importScreen;
    
    public const string DefaultDatabaseName = "New Document";
    public const string RECENT_PATH = "recentPath";
    
    [HideInInspector] public FGDatabase Database;
    
    [HideInInspector] public bool TransactionsChanged;

    public Action OnTabPressed;
    
    void Start()
    {
        Instance = this;
        Database = new(DefaultDatabaseName);

        versionNumber.text = $"<sprite name=\"fg\"> {Application.version}";
        
        splashScreen.Initialize();
        balanceSheetScreen.Initialize();
        transactionsScreen.Initialize();
        importScreen.Initialize();
        
        splashScreen.CheckRecentDatabase();
    }

    public void OnTab() => OnTabPressed?.Invoke();
    
    #region Top Row
    
    public void SetTransactions() => SetScreen(transactionsButton, transactionsScreen.gameObject);
    
    public void SetBalanceSheet()
    {
        SetScreen(balanceSheetButton, balanceSheetScreen.gameObject);
        
        if (TransactionsChanged) balanceSheetScreen.RefreshBalanceSheetRows();
    }
    
    public void SetImport() => SetScreen(importButton, importScreen.gameObject);

    void SetScreen(Button button, GameObject screen)
    {
        foreach (var i in allButtons)
            i.interactable = i != button;
        
        foreach (var j in allPanels)
            j.SetActive(j == screen);
        
        topRow.SetActive(true);
        splashScreen.gameObject.SetActive(false);
    }
    
    #endregion
    
    #region IO

    public void Save(string path = null)
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
        Debug.Log($"Saved <b>{Database.Name}</b> to <i>{path}</i>");
    }
    
    public void Load(string path, bool thenSave = false)
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
        
        #if UNITY_STANDALONE_WIN
        var file = path.Substring(path.LastIndexOf('\\') + 1);
        #else
        var file = path.Substring(path.LastIndexOf('/') + 1);
        #endif
        
        Database = new FGDatabase(file.Substring(0, file.Length - 3), File.ReadAllText(path));
        
        Debug.Log($"Loaded <b>{Database.Name}</b> from <i>{path}</i>");
        
        PlayerPrefs.SetString(RECENT_PATH, path);
        
        // TODO: clear old balance sheet
        // TODO: clear old transactions
        
        balanceSheetScreen.InstantiateBalanceSheetCells();
        transactionsScreen.InstantiateTransactions();
        importScreen.InstantiateImportRules();
        SetBalanceSheet();
        
        if (thenSave) Save(path);
    }
    
    #endregion
}