using System.Collections.Generic;
using UnityEngine;

public class FGBudgetScreenPanel : MonoBehaviour
{
    [SerializeField] Transform budgetEntryIncomeSibling;
    [SerializeField] Transform budgetEntryCostSibling;
    [SerializeField] FGBudgetEntryController budgetEntryPrefab;
    
    List<FGBudgetEntryController> budgetEntries = new();
    
    FGManager manager;
    
    public void Initialize()
    {
        manager = FGManager.Instance;
    }
    
    public void InstantiateBudgetEntries()
    {
        foreach (var i in manager.Database.BudgetEntries)
            AddBudgetEntry(i, false);
    }

    void AddBudgetEntry(FGBudgetEntry budgetEntry, bool undoable)
    {
        // TODO))
    }

    void AddBudgetEntry()
    {
        var budgetEntry = new FGBudgetEntry();
        manager.Database.BudgetEntries.Add(budgetEntry);
        AddBudgetEntry(budgetEntry, true);
        
        OnValueChanged();
    }

    void MoveBudgetEntry(FGBudgetEntryController budgetEntryController, FGBudgetEntry budgetEntry, bool up)
    {
        var index = budgetEntryController.transform.GetSiblingIndex() + (up ? -1 : 1);

        if (index >= budgetEntries.Count || index < 0) return;
            
        manager.Database.BudgetEntries.Remove(budgetEntry);
        manager.Database.BudgetEntries.Insert(index, budgetEntry);
        OnValueChanged();
            
        budgetEntryController.transform.SetSiblingIndex(index);
    }

    void OnValueChanged()
    {
        Debug.Log("Budget entries changed");
        
        manager.Save();
    }

    public void Import()
    {
        // TODO))
    }
}