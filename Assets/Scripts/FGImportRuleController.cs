using System;
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
    
    bool comparisonChanged,
         resultChanged;
    
    FGImportRule importRule;
    Action onSave;

    public Action<FGImportRule> OnRemove;
    
    public void Initialize(FGImportRule importRule, Action onSave)
    {
        this.importRule = importRule;
        this.onSave = onSave;
        
        OnIfPropertySet((int)importRule.ifProperty);
        OnComparatorSet((int)importRule.Comparator);
        OnComparisonSet(importRule.Comparison);
        
        OnThenPropertySet((int)importRule.thenProperty);
        OnResultSet(importRule.Result);
        
        #region Listeners
        
        #region OnValueChanged
        
        ifProperty.onValueChanged.AddListener(OnIfPropertySet);
        comparator.onValueChanged.AddListener(OnComparatorSet);
        comparison.onValueChanged.AddListener(_ => comparisonChanged = true);
        
        thenProperty.onValueChanged.AddListener(OnThenPropertySet);
        result.onValueChanged.AddListener(_ => resultChanged = true);
        
        #endregion
        
        #region OnDeselect/OnSubmit
        
        comparison.onDeselect.AddListener(OnComparisonSet);
        comparison.onSubmit.AddListener(OnComparisonSet);
        
        result.onDeselect.AddListener(OnResultSet);
        result.onSubmit.AddListener(OnResultSet);
        
        #endregion
        
        #endregion
    }
    
    #region Setters

    void OnIfPropertySet(int index)
    {
        importRule.ifProperty = (FGImportRuleIfProperty)Enum.GetValues(typeof(FGImportRuleIfProperty)).GetValue(index);
        ifProperty.SetValueWithoutNotify(index);

        onSave?.Invoke();
    }

    void OnComparatorSet(int index)
    {
        importRule.Comparator = (FGImportRuleComparator)Enum.GetValues(typeof(FGImportRuleComparator)).GetValue(index);
        comparator.SetValueWithoutNotify(index);

        onSave?.Invoke();
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
    }

    void OnThenPropertySet(int index)
    {
        importRule.thenProperty = (FGImportRuleThenProperty)Enum.GetValues(typeof(FGImportRuleThenProperty)).GetValue(index);
        thenProperty.SetValueWithoutNotify(index);

        onSave?.Invoke();
    }

    void OnResultSet(string newValue)
    {
        var formatted = FGUtils.FormatString(newValue, FGUtils.ALL);
        importRule.Result = formatted;
        result.SetTextWithoutNotify(importRule.Result);

        if (resultChanged)
        {
            onSave?.Invoke();
            resultChanged = false;
        }
    }
    
    #endregion

    public void Remove() => OnRemove?.Invoke(importRule);
}