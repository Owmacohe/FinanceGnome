using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FGBudgetEntryController : MonoBehaviour
{
    // TODO)): properties
    
    List<TMP_InputField> fields;
    TMP_InputField currentField;
    
    FGBudgetEntry budgetEntry;
    Action onSave;
    
    public Action<FGBudgetEntry, bool, bool> OnMove;
    public Action<FGBudgetEntry, bool> OnRemove;
    public Action<int> OnSubmitPressed;
    
    public void Initialize(FGBudgetEntry budgetEntry, Action onSave)
    {
        fields = new() {  }; // TODO))
        
        this.budgetEntry = budgetEntry;
        this.onSave = onSave;
        
        // TODO))
    }
    
    // TODO)): setters
    
    public void Move(bool up) => OnMove?.Invoke(budgetEntry, up, true);

    public void Select(int index) => fields[index].Select();

    void OnSubmit() => OnSubmitPressed?.Invoke(fields.IndexOf(currentField));

    public void Remove() => OnRemove?.Invoke(budgetEntry, true);
}