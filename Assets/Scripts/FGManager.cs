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
    [Header("Top Bar")]
    [SerializeField] GameObject topRow;
    [SerializeField] Button balanceSheetButton;
    [SerializeField] Button transactionsScreenButton;
    
    [Header("Splash Screen")]
    [SerializeField] GameObject splashScreen;
    [SerializeField] TMP_InputField newDatabaseName;
    [SerializeField] Button openRecentDatabase;
    [SerializeField] List<Button> splashScreenButtons;

    [Header("Balance Sheet Screen")]
    [SerializeField] GameObject balanceSheetScreen;
    [SerializeField] Transform balanceSheetParent;
    [SerializeField] TMP_Text balanceSheetCell;

    [Header("Transactions Screen")]
    [SerializeField] GameObject transactionsScreen;
    [SerializeField] Transform transactionsParent;
    [SerializeField] FGTransactionController transactionPrefab;

    List<FGTransactionController> transactions = new();
    bool transactionsChanged;
    
    string defaultDatabaseName = "New Document";
    public static FGDatabase Database;

    const string RECENT_PATH = "recentPath";

    int addEntriesAmount = 1;
    
    void Start()
    {
        Database = new(defaultDatabaseName);
        CheckRecentDatabase();
    }
    
    #region Top Row
    
    public void SetBalanceSheet()
    {
        balanceSheetButton.interactable = false;
        transactionsScreenButton.interactable = true;
        
        balanceSheetScreen.SetActive(true);
        transactionsScreen.SetActive(false);
        
        topRow.SetActive(true);
        splashScreen.SetActive(false);

        if (transactionsChanged)
        {
            for (int i = 0; i < balanceSheetParent.childCount - 15; i++)
                Destroy(balanceSheetParent.GetChild(15 + i).gameObject);
        
            InstantiateBalanceSheetCells();

            transactionsChanged = false;
        }
    }
    
    public void SetTransactions()
    {
        balanceSheetButton.interactable = true;
        transactionsScreenButton.interactable = false;

        balanceSheetScreen.SetActive(false);
        transactionsScreen.SetActive(true);
        
        topRow.SetActive(true);
        splashScreen.SetActive(false);
    }
    
    #endregion
    
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
            #if UNITY_STANDALONE_WIN
            var fullPath = $"{path}\\{Database.Name}.fg";
            #else
            var fullPath = $"{path}/{Database.Name}.fg";
            #endif
            
            Save(fullPath);
            
            PlayerPrefs.SetString(RECENT_PATH, fullPath);
            CheckRecentDatabase();

            InstantiateBalanceSheetCells();
            InstantiateTransactions();
            SetBalanceSheet();
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
    
    #region Balance Sheet Screen

    void InstantiateBalanceSheetCells()
    {
        AddBlankBalanceSheetRow();
        AddHeaderBalanceSheetRow("Costs");
        AddCategoryEntries(true);

        AddBlankBalanceSheetRow();
        AddHeaderBalanceSheetRow("Income");
        AddCategoryEntries(false);

        AddBlankBalanceSheetRow();
        AddBalanceBalanceSheetRow();
    }
    
    void AddBlankBalanceSheetRow() => AddBalanceSheetRow((new string[15]).ToList());

    void AddHeaderBalanceSheetRow(string header)
    {
        var temp = new string[15];
        temp[0] = $"<b>{header}</b>";
        
        AddBalanceSheetRow(temp.ToList());
    }

    Dictionary<string, List<TMP_Text>> AddCategoryEntries(bool costs)
    {
        var colourMax = costs ? Color.red : Color.green;

        Dictionary<string, List<TMP_Text>> temp = new();
        
        foreach (var i in Database.Categories)
        {
            List<string> row = new();

            var entries = Database.EntriesInCategory(i, costs);
            if (entries.Count == 0 || Database.TotalForCategory(entries) == 0) continue;
            
            row.Add(string.IsNullOrEmpty(i) ? "N/A" : i);

            for (int j = 0; j < 12; j++)
                row.Add(FGUtils.FormatLargeNumber(
                    Database.TotalForMonthByCategory(entries, j + 1),
                    true,
                    colourMax,
                    3000));
            
            row.Add(FGUtils.FormatLargeNumber(
                Database.AverageForCategoryByWeek(entries),
                true,
                colourMax,
                500));
            
            row.Add(FGUtils.FormatLargeNumber(
                Database.AverageForCategoryByMonth(entries),
                true,
                colourMax,
                2000));

            temp.Add(i, AddBalanceSheetRow(row));
        }

        return temp;
    }

    List<TMP_Text> AddBalanceBalanceSheetRow()
    {
        List<string> row = new();
            
        row.Add("<b>Balance</b>");

        for (int i = 0; i < 12; i++)
        {
            var incomeTotal = Database.TotalForMonth(i+1, false);
            var costsTotal = Database.TotalForMonth(i+1, true);
            float balance = incomeTotal - costsTotal;
            
            row.Add(FGUtils.FormatLargeNumber(
                balance,
                true,
                balance >= 0 ? Color.green : Color.red,
                10000));
        }
        
        var weeklyTotal = Database.ValueTotal / 52f;
        var monthlyTotal = Database.ValueTotal / 12f;
        
        row.Add(FGUtils.FormatLargeNumber(
            weeklyTotal,
            true,
            weeklyTotal >= 0 ? Color.green : Color.red,
            1000));
        
        row.Add(FGUtils.FormatLargeNumber(
            monthlyTotal,
            true,
            monthlyTotal >= 0 ? Color.green : Color.red,
            7500));
            
        return AddBalanceSheetRow(row);
    }

    List<TMP_Text> AddBalanceSheetRow(List<string> row)
    {
        if (row.Count != 15)
        {
            Debug.LogError($"Row of size {row.Count} is not valid (must be of size 15)");
            return null;
        }

        List<TMP_Text> temp = new();

        foreach (var i in row)
        {
            var cell = Instantiate(balanceSheetCell, balanceSheetParent);
            cell.text = string.IsNullOrEmpty(i) ? "" : i;
            cell.name = cell.text;
            temp.Add(cell);
        }

        return temp;
    }
    
    #endregion
    
    #region Transations Screen

    void InstantiateTransactions()
    {
        for (int i = 0; i < Database.Entries.Count; i++)
            AddTransaction(Database.Entries[i], i + 1);
    }

    void AddTransaction(FGEntry entry, int lineNumber)
    {
        var transaction = Instantiate(transactionPrefab, transactionsParent);
        transaction.Initialize(lineNumber, entry, () => OnValueChanged());
        transactions.Add(transaction);
        
        transaction.OnRemove += entry =>
        {
            Database.Entries.Remove(entry);
            OnValueChanged();
            
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
        
        OnValueChanged();
    }

    public void OnValueChanged()
    {
        Debug.Log("Transactions changed (balance sheet will reload when next shown)");
        
        transactionsChanged = true;
        
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
        
        #if UNITY_STANDALONE_WIN
        var file = path.Substring(path.LastIndexOf('\\') + 1);
        #else
        var file = path.Substring(path.LastIndexOf('/') + 1);
        #endif
        
        Database = new FGDatabase(file.Substring(0, file.Length - 3), File.ReadAllText(path));
        
        Debug.Log($"Loaded {Database.Name} from {path}");
        
        // TODO: clear old balance sheet
        // TODO: clear old transactions
        
        InstantiateBalanceSheetCells();
        InstantiateTransactions();
        SetBalanceSheet();
        
        if (thenSave) Save(path);
    }

    public void Import()
    {
        Debug.Log("Importing...");
        
        DisableSplashScreenButtons();
        
        var path = FileBrowser.PickFile();
        
        if (string.IsNullOrEmpty(path)) Debug.LogError("Path is null");
        else if (!path.Substring(path.IndexOf('.')).Equals(".csv")) Debug.LogError("File is not of a valid file type");
        else
        {
            var entries = Database.Import(File.ReadAllText(path));

            for (int i = 0; i < entries.Count; i++)
                AddTransaction(entries[i], Database.Entries.Count - entries.Count + i + 1);
            
            OnValueChanged();
            
            Debug.Log($"Imported from {path}");
        }
        
        Invoke(nameof(EnableSplashScreenButtons), 0.1f);
    }
    
    #endregion
}