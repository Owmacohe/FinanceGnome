using System.Collections.Generic;
using System.IO;
using NativeFileBrowser;
using UnityEngine;

public class FGBudgetScreenPanel : MonoBehaviour
{
    [SerializeField] Transform budgetEntryIncomeParent;
    [SerializeField] Transform budgetEntryCostParent;
    [SerializeField] FGBudgetEntryController budgetEntryPrefab;
    
    List<FGBudgetEntryController> incomeBudgetEntries = new();
    List<FGBudgetEntryController> costBudgetEntries = new();
    
    List<FGBudgetEntryController> BudgetEntries(bool isCost) => isCost
        ? costBudgetEntries
        : incomeBudgetEntries;

    List<FGBudgetEntry> BudgetEntriesData(bool isCost) => isCost
        ? manager.Database.CostBudgetEntries
        : manager.Database.IncomeBudgetEntries;
    
    FGManager manager;
    
    public void Initialize()
    {
        manager = FGManager.Instance;
    }
    
    public void InstantiateBudgetEntries()
    {
        foreach (var i in manager.Database.IncomeBudgetEntries)
            AddBudgetEntry(i, false);
        
        foreach (var i in manager.Database.CostBudgetEntries)
            AddBudgetEntry(i, false);
    }

    void AddBudgetEntry(FGBudgetEntry budgetEntry, bool undoable)
    {
        bool isCost = budgetEntry.IsCost;
        
        var budgetEntryController = Instantiate(budgetEntryPrefab, isCost ? budgetEntryCostParent : budgetEntryIncomeParent);
        budgetEntryController.Initialize(budgetEntry, OnValueChanged);

        var budgetEntries = BudgetEntries(isCost);
        var budgetEntriesData = BudgetEntriesData(isCost);
        budgetEntries.Add(budgetEntryController);

        budgetEntryController.OnMove += (budgetEntry, up, undoable) =>
        {
            MoveBudgetEntry(budgetEntryController, budgetEntry, up);
            
            if (undoable) FGUndoController.Instance.SaveUndo(() => MoveBudgetEntry(budgetEntryController, budgetEntry, !up));
        };
        
        budgetEntryController.OnRemove += (budgetEntry, undoable) =>
        {
            budgetEntriesData.Remove(budgetEntry);
            OnValueChanged();

            budgetEntries.Remove(budgetEntryController);
            Destroy(budgetEntryController.gameObject); // TODO: potential undo bug
        
            if (undoable) FGUndoController.Instance.SaveUndo(() =>
            {
                budgetEntriesData.Add(budgetEntry);
                AddBudgetEntry(budgetEntry, false);
                
                OnValueChanged();
                
                manager.SetBudget();
            });
        };

        budgetEntryController.OnSubmitPressed += index =>
        {
            int budgetEntryIndex = budgetEntries.IndexOf(budgetEntryController) + 1;
            if (budgetEntryIndex >= budgetEntries.Count) budgetEntryIndex = 0;
            
            budgetEntries[budgetEntryIndex].Select(index);
        };
        
        if (undoable) FGUndoController.Instance.SaveUndo(() =>
        {
            budgetEntryController.OnRemove?.Invoke(budgetEntry, false);
                
            manager.SetBudget();
        });
    }

    public void AddBudgetEntry(bool isCost)
    {
        var budgetEntry = new FGBudgetEntry { IsCost = isCost };
        BudgetEntriesData(isCost).Add(budgetEntry);
        AddBudgetEntry(budgetEntry, true);
        
        OnValueChanged();
    }

    void MoveBudgetEntry(FGBudgetEntryController budgetEntryController, FGBudgetEntry budgetEntry, bool up)
    {
        var index = budgetEntryController.transform.GetSiblingIndex() + (up ? -1 : 1);

        if (index >= BudgetEntries(budgetEntry.IsCost).Count || index < 0) return;
        
        var dataList = BudgetEntriesData(budgetEntry.IsCost);
        dataList.Remove(budgetEntry);
        dataList.Insert(index, budgetEntry);
        OnValueChanged();
            
        budgetEntryController.transform.SetSiblingIndex(index);
    }

    void OnValueChanged()
    {
        Debug.Log("Budget entries changed");
        
        manager.Save();
    }
    
    public void RefreshBudgetEntryRows()
    {
        foreach (var i in incomeBudgetEntries)
            i.RefreshBudgetEntryRow();
        
        foreach (var j in costBudgetEntries)
            j.RefreshBudgetEntryRow();
    }
    
    // TODO)): calculations
}