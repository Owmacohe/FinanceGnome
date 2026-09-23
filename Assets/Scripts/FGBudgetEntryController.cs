using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FGBudgetEntryController : MonoBehaviour
{
    [SerializeField] Toggle useCategory;
    [SerializeField] TMP_InputField category;
    
    [SerializeField] Toggle useAverageValue;
    [SerializeField] TMP_InputField manualValue;
    
    [SerializeField] Toggle track;

    bool categoryChanged,
         manualValueChanged;
    
    List<TMP_InputField> fields;
    TMP_InputField currentField;
    
    FGBudgetEntry budgetEntry;
    Action onSave;
    
    public Action<FGBudgetEntry, bool, bool> OnMove;
    public Action<FGBudgetEntry, bool> OnRemove;
    public Action<int> OnSubmitPressed;
    
    public void Initialize(FGBudgetEntry budgetEntry, Action onSave)
    {
        fields = new() { category, manualValue };
        
        this.budgetEntry = budgetEntry;
        this.onSave = onSave;
        
        #region Listeners
        
        #region OnValueChanged
        
        category.onValueChanged.AddListener(newValue =>
        {
            if (newValue.Contains(FGUtils.HIGHLIGHTER))
                newValue = newValue.Remove(newValue.IndexOf(FGUtils.HIGHLIGHTER));

            string matching = FGManager.Instance.Database.GetMatchingCategory(newValue);
            category.SetTextWithoutNotify(
                newValue +
                (matching != null && !string.IsNullOrEmpty(newValue) && this.budgetEntry.UseCategory
                    ? $"{FGUtils.HIGHLIGHTER}{matching.Substring(newValue.Length)}"
                    : ""));
            
            categoryChanged = true;
        });
        
        manualValue.onValueChanged.AddListener(_ => manualValueChanged = true);
        
        #endregion
        
        #region OnSelect/OnDeselect/OnSubmit
        
        useCategory.onValueChanged.AddListener(OnUseCategorySet);
        
        category.onSelect.AddListener(_ => currentField = category);
        category.onDeselect.AddListener(newValue => OnCategorySet(newValue, false));
        category.onSubmit.AddListener(_ => OnSubmit(true));
        
        useAverageValue.onValueChanged.AddListener(OnUseAverageValueSet);
        
        manualValue.onSelect.AddListener(_ => currentField = manualValue);
        manualValue.onDeselect.AddListener(OnManualValueSet);
        manualValue.onSubmit.AddListener(_ => OnSubmit());
        
        track.onValueChanged.AddListener(OnTrackSet);
        
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
    
    void OnUseCategorySet(bool newValue)
    {
        budgetEntry.UseCategory = newValue;
        useCategory.SetIsOnWithoutNotify(budgetEntry.UseCategory);
            
        onSave?.Invoke();

        currentField = null;
    }

    void OnCategorySet(string newValue, bool setMatchingCategory)
    {
        if (newValue.Contains(FGUtils.HIGHLIGHTER))
            newValue = newValue.Remove(newValue.IndexOf(FGUtils.HIGHLIGHTER));

        if (setMatchingCategory && !string.IsNullOrEmpty(newValue))
        {
            var matching = FGManager.Instance.Database.GetMatchingCategory(newValue);
            if (matching != null) newValue = matching;
        }
        
        var formatted = FGUtils.FormatString(newValue, FGUtils.DESCRIPTION_WHITELIST);
        budgetEntry.Category = formatted;
        category.SetTextWithoutNotify(budgetEntry.Category);
        // category.textComponent.color = FGUtils.StringToColour(Entry.Category);

        if (categoryChanged)
        {
            onSave?.Invoke();
            categoryChanged = false;
        }

        currentField = null;
    }
    
    void OnUseAverageValueSet(bool newValue)
    {
        budgetEntry.UseAverageValue = newValue;
        useAverageValue.SetIsOnWithoutNotify(budgetEntry.UseAverageValue);
            
        onSave?.Invoke();

        currentField = null;
    }

    void OnManualValueSet(string newValue)
    {
        var formatted = FGUtils.FormatString(newValue, FGUtils.VALUE_WHITELIST);
        
        if (formatted == "") budgetEntry.ManualValue = 0;
        else if (float.TryParse(formatted, out float outValue)) budgetEntry.ManualValue = outValue;
        
        manualValue.SetTextWithoutNotify(FGUtils.FormatLargeNumber(budgetEntry.ManualValue, false));
        ValueCheck();

        if (manualValueChanged)
        {
            onSave?.Invoke();
            manualValueChanged = false;
        }

        currentField = null;
    }
    
    void OnTrackSet(bool newValue)
    {
        budgetEntry.Track = newValue;
        track.SetIsOnWithoutNotify(budgetEntry.Track);
            
        onSave?.Invoke();

        currentField = null;
    }
    
    #endregion

    void ValueCheck()
    {
        manualValue.GetComponent<Image>().color = FGUtils.GraduatedColourLerp(
            budgetEntry.ManualValue / FGUtils.AMOUNT_MAX,
            FGUtils.GRADUATIONS,
            budgetEntry.IsCost ? FGUtils.NEGATIVE_LOW : FGUtils.POSITIVE_LOW,
            budgetEntry.IsCost ? FGUtils.NEGATIVE : FGUtils.POSITIVE);
    }
    
    public void Move(bool up) => OnMove?.Invoke(budgetEntry, up, true);

    public void Select(int index) => fields[index].Select();

    void OnSubmit(bool isCategory = false)
    {
        if (isCategory)
        {
            OnCategorySet(category.text, true);
            currentField = category;
        }
        
        OnSubmitPressed?.Invoke(fields.IndexOf(currentField));
    }

    public void Remove() => OnRemove?.Invoke(budgetEntry, true);
}