using System.Collections.Generic;
using System.IO;
using NativeFileBrowser;
using UnityEngine;

public class FGImportScreenPanel : MonoBehaviour
{
    [SerializeField] Transform importRuleParent;
    [SerializeField] FGImportRuleController importRulePrefab;
    
    List<FGImportRuleController> importRules = new();
    
    FGManager manager;

    public void Initialize()
    {
        manager = FGManager.Instance;
    }
    
    public void InstantiateImportRules()
    {
        foreach (var i in manager.Database.ImportRules)
            AddImportRule(i);
    }

    void AddImportRule(FGImportRule importRule)
    {
        var importRuleController = Instantiate(importRulePrefab, importRuleParent);
        importRuleController.Initialize(importRule, OnValueChanged);
        importRules.Add(importRuleController);
        
        importRuleController.OnRemove += importRule =>
        {
            manager.Database.ImportRules.Remove(importRule);
            OnValueChanged();

            importRules.Remove(importRuleController);
            Destroy(importRuleController.gameObject);
        };
    }
    
    public void AddImportRule()
    {
        var importRule = new FGImportRule();
        manager.Database.ImportRules.Add(importRule);
        AddImportRule(importRule);
        
        OnValueChanged();
    }

    void OnValueChanged()
    {
        Debug.Log("Import rules changed");
        
        manager.Save();
    }

    void CheckEntry(FGEntry entry) => manager.Database.ImportRules.ForEach(rule => rule.CheckEntry(entry));
    
    public void Import()
    {
        Debug.Log("Importing...");
        
        var path = FileBrowser.PickFile();
        
        if (string.IsNullOrEmpty(path)) Debug.LogError("Path is null");
        else if (!path.Substring(path.IndexOf('.')).Equals(".csv")) Debug.LogError("File is not of a valid file type");
        else
        {
            var entries = manager.Database.Import(File.ReadAllText(path));

            for (int i = 0; i < entries.Count; i++)
            {
                CheckEntry(entries[i]);
                
                manager.transactionsScreen.AddTransaction(entries[i], manager.Database.Entries.Count - entries.Count + i + 1);
            }
            
            manager.transactionsScreen.OnValueChanged();
            
            Debug.Log($"Imported from <i>{path}</i>");
        }
        
        manager.SetTransactions();
    }

    public void Apply()
    {
        manager.Database.Entries.ForEach(CheckEntry);
        manager.transactionsScreen.RefreshTransactions();
    }
}