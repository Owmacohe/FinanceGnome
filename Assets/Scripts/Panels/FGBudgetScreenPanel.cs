using UnityEngine;

public class FGBudgetScreenPanel : MonoBehaviour
{
    // TODO)): properties
    
    FGManager manager;
    
    public void Initialize()
    {
        manager = FGManager.Instance;
    }
    
    public void InstantiateBudgetEntries()
    {
        // TODO))
    }

    void AddBudgetEntry(FGBudgetEntry budgetEntry, bool undoable)
    {
        // TODO))
    }

    void AddBudgetEntry()
    {
        // TODO))
    }

    void MoveBudgetEntry(FGBudgetEntryController budgetEntryController, FGBudgetEntry budgetEntry, bool up)
    {
        // TODO))
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