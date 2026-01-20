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
            AddImportRule(i, false);
    }

    void AddImportRule(FGImportRule importRule, bool undoable)
    {
        var importRuleController = Instantiate(importRulePrefab, importRuleParent);
        importRuleController.Initialize(importRule, OnValueChanged);
        importRules.Add(importRuleController);

        importRuleController.OnMove += (importRule, up, undoable) =>
        {
            MoveImportRule(importRuleController, importRule, up);
            
            if (undoable) FGUndoController.Instance.SaveUndo(() => MoveImportRule(importRuleController, importRule, !up));
        };
        
        importRuleController.OnRemove += (importRule, undoable) =>
        {
            manager.Database.ImportRules.Remove(importRule);
            OnValueChanged();

            importRules.Remove(importRuleController);
            Destroy(importRuleController.gameObject); // TODO: potential undo bug
        
            if (undoable) FGUndoController.Instance.SaveUndo(() =>
            {
                manager.Database.ImportRules.Add(importRule);
                AddImportRule(importRule, false);
                
                OnValueChanged();
                
                manager.SetImport();
            });
        };

        importRuleController.OnSubmitPressed += index =>
        {
            int importRuleIndex = importRules.IndexOf(importRuleController) + 1;
            if (importRuleIndex >= importRules.Count) importRuleIndex = 0;
            
            importRules[importRuleIndex].Select(index);
        };
        
        if (undoable) FGUndoController.Instance.SaveUndo(() =>
        {
            importRuleController.OnRemove?.Invoke(importRule, false);
                
            manager.SetImport();
        });
    }
    
    public void AddImportRule()
    {
        var importRule = new FGImportRule();
        manager.Database.ImportRules.Add(importRule);
        AddImportRule(importRule, true);
        
        OnValueChanged();
    }

    void MoveImportRule(FGImportRuleController importRuleController, FGImportRule importRule, bool up)
    {
        var index = importRuleController.transform.GetSiblingIndex() + (up ? -1 : 1);

        if (index >= importRules.Count || index < 0) return;
            
        manager.Database.ImportRules.Remove(importRule);
        manager.Database.ImportRules.Insert(index, importRule);
        OnValueChanged();
            
        importRuleController.transform.SetSiblingIndex(index);
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
                
                manager.transactionsScreen.AddTransaction(
                    entries[i],
                    manager.Database.Entries.Count - entries.Count + i + 1,
                    false);
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
        manager.transactionsScreen.OnValueChanged();
        manager.SetTransactions();
        
        manager.Save();
    }
}