using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FGBudgetEntryController : MonoBehaviour
{
    [Header("Entry")]
    [SerializeField] Toggle useCategory;
    [SerializeField] TMP_InputField category;
    
    [SerializeField] Toggle useAverageValue;
    [SerializeField] TMP_InputField manualValue;
    
    [SerializeField] Toggle essential;
    
    [Header("Dynamic")]
    [SerializeField] TMP_Text total;

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
        
        OnUseCategorySet(budgetEntry.UseCategory, true);
        OnCategorySet(budgetEntry.Category, false);
        
        OnUseAverageValueSet(budgetEntry.UseAverageValue);
        OnManualValueSet(budgetEntry.ManualValue.ToString());
        
        OnEssentialSet(budgetEntry.Essential);
        
        for (int i = 0; i < essential.transform.childCount; i++)
            essential.transform.GetChild(i).gameObject.SetActive(budgetEntry.IsCost);
        
        #region Listeners
        
        #region OnValueChanged
        
        category.onValueChanged.AddListener(newValue =>
        {
            if (newValue.Contains(FGUtils.HIGHLIGHTER))
                newValue = newValue.Remove(newValue.IndexOf(FGUtils.HIGHLIGHTER));

            string matching = FGManager.Instance.Database.GetMatchingCategory(newValue);
            category.SetTextWithoutNotify(
                newValue +
                (matching != null && !string.IsNullOrEmpty(newValue)
                    ? $"{FGUtils.HIGHLIGHTER}{matching.Substring(newValue.Length)}"
                    : ""));
            
            categoryChanged = true;
        });
        
        manualValue.onValueChanged.AddListener(_ => manualValueChanged = true);
        
        #endregion
        
        #region OnSelect/OnDeselect/OnSubmit
        
        useCategory.onValueChanged.AddListener(newValue => OnUseCategorySet(newValue));
        
        category.onSelect.AddListener(_ => currentField = category);
        category.onDeselect.AddListener(newValue => OnCategorySet(newValue, false));
        category.onSubmit.AddListener(_ => OnSubmit(true));
        
        useAverageValue.onValueChanged.AddListener(OnUseAverageValueSet);
        
        manualValue.onSelect.AddListener(_ => currentField = manualValue);
        manualValue.onDeselect.AddListener(OnManualValueSet);
        manualValue.onSubmit.AddListener(_ => OnSubmit());
        
        essential.onValueChanged.AddListener(OnEssentialSet);
        
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
    
    void OnUseCategorySet(bool newValue, bool initializing  = false)
    {
        budgetEntry.UseCategory = newValue;
        useCategory.SetIsOnWithoutNotify(budgetEntry.UseCategory);

        useAverageValue.interactable = newValue;
        if (!initializing) OnUseAverageValueSet(newValue);
        RefreshBudgetEntryRow();
            
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

        bool hasCategory = FGManager.Instance.Database.Categories(false).Contains(newValue);
        useCategory.interactable = hasCategory;
        if (!hasCategory) OnUseCategorySet(false);

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
        
        manualValue.interactable = !newValue;
        RefreshAverageValue();
        
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

        RefreshTotal();
        
        FGManager.Instance.budgetScreen.RefreshCalculations();

        if (manualValueChanged)
        {
            onSave?.Invoke();
            manualValueChanged = false;
        }

        currentField = null;
    }
    
    void OnEssentialSet(bool newValue)
    {
        budgetEntry.Essential = newValue;
        essential.SetIsOnWithoutNotify(budgetEntry.Essential);
        
        FGManager.Instance.budgetScreen.RefreshCalculations();
            
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

    void RefreshAverageValue()
    {
        if (!budgetEntry.UseAverageValue) return;
        
        var database = FGManager.Instance.Database;
        OnManualValueSet(
            database.AverageForCategoryByMonth(
                database.EntriesInCategory(budgetEntry.Category, budgetEntry.IsCost)).ToString());
    }

    void RefreshTotal()
    {
        if (budgetEntry.UseCategory && budgetEntry.IsCost)
        {
            var database = FGManager.Instance.Database;
            var amount = database.TotalForMonthByCategory(
                database.EntriesInCategory(budgetEntry.Category, budgetEntry.IsCost),
                DateTime.Today.Month);
            
            budgetEntry.CurrentValue = amount;
            
            var formatted = FGUtils.FormatLargeNumber(budgetEntry.CurrentValue, true, FGUtils.POSITIVE, FGUtils.NEGATIVE, budgetEntry.ManualValue);
            var formattedLeft = FGUtils.FormatLargeNumber(budgetEntry.ManualValue - budgetEntry.CurrentValue, true, FGUtils.NEGATIVE, FGUtils.POSITIVE, budgetEntry.ManualValue);

            total.text = $"{FGUtils.GetMonth(DateTime.Today.Month)} total = {formatted} ({formattedLeft} left)";
        }
        else total.text = "";
    }

    public void RefreshBudgetEntryRow()
    {
        RefreshAverageValue();
        RefreshTotal();
    }
}