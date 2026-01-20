using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FGImportRuleController : MonoBehaviour
{
    [Header("If")]
    [SerializeField] TMP_Dropdown ifProperty;
    [SerializeField] TMP_Dropdown comparator;
    [SerializeField] TMP_InputField comparison;

    [Header("Then")]
    [SerializeField] TMP_Dropdown thenProperty;
    [SerializeField] TMP_InputField result;
    [SerializeField] TMP_InputField note;
    
    bool comparisonChanged,
         resultChanged,
         noteChanged;
    
    List<TMP_InputField> fields;
    TMP_InputField currentField;
    
    FGImportRule importRule;
    Action onSave;

    public Action<FGImportRule, bool, bool> OnMove;
    public Action<FGImportRule, bool> OnRemove;
    public Action<int> OnSubmitPressed;
    
    public void Initialize(FGImportRule importRule, Action onSave)
    {
        fields = new() { comparison, result, note  };
        
        this.importRule = importRule;
        this.onSave = onSave;
        
        OnIfPropertySet((int)importRule.ifProperty);
        OnComparatorSet((int)importRule.Comparator);
        OnComparisonSet(importRule.Comparison);
        
        OnThenPropertySet((int)importRule.thenProperty);
        OnResultSet(importRule.Result);
        OnNoteSet(importRule.Note);
        
        #region Listeners
        
        #region OnValueChanged
        
        ifProperty.onValueChanged.AddListener(OnIfPropertySet);
        comparator.onValueChanged.AddListener(OnComparatorSet);
        comparison.onValueChanged.AddListener(_ => comparisonChanged = true);
        
        thenProperty.onValueChanged.AddListener(OnThenPropertySet);
        result.onValueChanged.AddListener(_ => resultChanged = true);
        note.onValueChanged.AddListener(_ => noteChanged = true);
        
        #endregion
        
        #region OnSelectOnDeselect/OnSubmit
        
        comparison.onSelect.AddListener(_ => currentField = comparison);
        comparison.onDeselect.AddListener(OnComparisonSet);
        comparison.onSubmit.AddListener(_ => OnSubmit());
        
        result.onSelect.AddListener(_ => currentField = result);
        result.onDeselect.AddListener(OnResultSet);
        result.onSubmit.AddListener(_ => OnSubmit());
        
        note.onSelect.AddListener(_ => currentField = note);
        note.onDeselect.AddListener(OnNoteSet);
        note.onSubmit.AddListener(_ => OnSubmit());
        
        #endregion
        
        #endregion

        FGManager.Instance.OnTabPressed += () =>
        {
            if (currentField == null) return;
            
            int index = fields.IndexOf(currentField);
            index++;
            if (index >= fields.Count) index = 0;
            
            fields[index].Select();
        };
    }
    
    #region Setters

    void OnIfPropertySet(int index)
    {
        importRule.ifProperty = (FGImportRuleIfProperty)Enum.GetValues(typeof(FGImportRuleIfProperty)).GetValue(index);
        ifProperty.SetValueWithoutNotify(index);

        onSave?.Invoke();

        currentField = null;
    }

    void OnComparatorSet(int index)
    {
        importRule.Comparator = (FGImportRuleComparator)Enum.GetValues(typeof(FGImportRuleComparator)).GetValue(index);
        comparator.SetValueWithoutNotify(index);

        onSave?.Invoke();

        currentField = null;
    }

    void OnComparisonSet(string newValue)
    {
        var formatted = FGUtils.FormatString(newValue, FGUtils.ALL);
        importRule.Comparison = formatted;
        comparison.SetTextWithoutNotify(importRule.Comparison);

        if (comparisonChanged)
        {
            onSave?.Invoke();
            comparisonChanged = false;
        }

        currentField = null;
    }

    void OnThenPropertySet(int index)
    {
        importRule.thenProperty = (FGImportRuleThenProperty)Enum.GetValues(typeof(FGImportRuleThenProperty)).GetValue(index);
        thenProperty.SetValueWithoutNotify(index);

        onSave?.Invoke();

        currentField = null;
    }

    void OnResultSet(string newValue)
    {
        var formatted = FGUtils.FormatString(newValue, FGUtils.ALL);
        importRule.Result = formatted;
        result.SetTextWithoutNotify(importRule.Result);
        result.textComponent.color = FGUtils.StringToColour(importRule.Result);

        if (resultChanged)
        {
            onSave?.Invoke();
            resultChanged = false;
        }

        currentField = null;
    }

    void OnNoteSet(string newValue)
    {
        var formatted = FGUtils.FormatString(newValue, FGUtils.ALL);
        importRule.Note = formatted;
        note.SetTextWithoutNotify(importRule.Note);

        if (noteChanged)
        {
            onSave?.Invoke();
            noteChanged = false;
        }

        currentField = null;
    }
    
    #endregion

    public void Move(bool up) => OnMove?.Invoke(importRule, up, true);

    public void Select(int index) => fields[index].Select();

    void OnSubmit() => OnSubmitPressed?.Invoke(fields.IndexOf(currentField));

    public void Remove() => OnRemove?.Invoke(importRule, true);
}