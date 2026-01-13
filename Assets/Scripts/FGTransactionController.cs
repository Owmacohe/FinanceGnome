using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FGTransactionController : MonoBehaviour
{
    [SerializeField] TMP_Text line;
    [SerializeField] TMP_InputField date;
    [SerializeField] TMP_InputField description;
    [SerializeField] TMP_InputField value;
    [SerializeField] Toggle isCost;
    [SerializeField] TMP_InputField category;
    [SerializeField] TMP_InputField note;
    [SerializeField] Toggle ignore;
    
    bool dateChanged, 
         descriptionChanged,
         valueChanged,
         categoryChanged,
         noteChanged;

    List<TMP_InputField> fields;
    TMP_InputField currentField;

    int lineNumber;
    FGEntry entry;
    Action onSave;

    const string HIGHLIGHTER = "<mark=#A8CEFF>";

    public Action<FGEntry, bool> OnRemove;
    public Action<int> OnSubmitPressed;

    public void Initialize(int lineNumber, FGEntry entry, Action onSave)
    {
        fields = new() { date, description, value, category, note  };
        
        this.entry = entry;
        this.onSave = onSave;

        this.lineNumber = lineNumber;
        ModifyLineNumber();

        Refresh();
        
        #region Listeners
        
        #region OnValueChanged
        
        date.onValueChanged.AddListener(_ => dateChanged = true);
        description.onValueChanged.AddListener(_ => descriptionChanged = true);
        value.onValueChanged.AddListener(_ => valueChanged = true);
        category.onValueChanged.AddListener(newValue =>
        {
            if (newValue.Contains(HIGHLIGHTER))
                newValue = newValue.Remove(newValue.IndexOf(HIGHLIGHTER));

            string matching = FGManager.Instance.Database.GetMatchingCategory(newValue);
            category.SetTextWithoutNotify(
                newValue +
                (matching != null && !string.IsNullOrEmpty(newValue)
                    ? $"{HIGHLIGHTER}{matching.Substring(newValue.Length)}"
                    : ""));
            
            categoryChanged = true;
        });
        note.onValueChanged.AddListener(_ => noteChanged = true);
        
        #endregion
        
        #region OnSelect/OnDeselect/OnSubmit
        
        date.onSelect.AddListener(_ => currentField = date);
        date.onDeselect.AddListener(OnDateSet);
        date.onSubmit.AddListener(_ => OnSubmit());
        
        description.onSelect.AddListener(_ => currentField = description);
        description.onDeselect.AddListener(OnDescriptionSet);
        description.onSubmit.AddListener(_ => OnSubmit());
        
        value.onSelect.AddListener(_ => currentField = value);
        value.onDeselect.AddListener(OnValueSet);
        value.onSubmit.AddListener(_ => OnSubmit());
        
        isCost.onValueChanged.AddListener(OnIsCostSet);
        
        category.onSelect.AddListener(_ => currentField = category);
        category.onDeselect.AddListener(newValue => OnCategorySet(newValue, false));
        category.onSubmit.AddListener(_ => OnSubmit());
        
        note.onSelect.AddListener(_ => currentField = note);
        note.onDeselect.AddListener(OnNoteSet);
        note.onSubmit.AddListener(_ => OnSubmit());
        
        ignore.onValueChanged.AddListener(OnIgnoreSet);
        
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

    public void Refresh()
    {
        OnDateSet(FGUtils.DateToString(entry.Date));
        OnDescriptionSet(entry.Description);
        OnValueSet(entry.Value.ToString());
        OnCategorySet(entry.Category, false);
        OnNoteSet(entry.Note);
        
        isCost.isOn = entry.IsCost;
        ignore.isOn = entry.Ignore;
        
        IgnoreCheck();
    }
    
    #region Setters

    void OnDateSet(string newValue)
    {
        var formatted = FGUtils.FormatString(newValue, FGEntry.DATE_WHITELIST);
        var temp = FGUtils.TryParseDateTime(formatted, entry.Date, out var failed);

        if (!failed)
        {
            entry.Date = temp;
            date.SetTextWithoutNotify(FGUtils.DateToString(entry.Date));

            if (dateChanged)
            {
                onSave?.Invoke();
                dateChanged = false;
            }   
        }

        currentField = null;
    }

    void OnDescriptionSet(string newValue)
    {
        var formatted = FGUtils.FormatString(newValue, FGEntry.DESCRIPTION_WHITELIST);
        entry.Description = formatted;
        description.SetTextWithoutNotify(entry.Description);

        if (descriptionChanged)
        {
            onSave?.Invoke();
            descriptionChanged = false;
        }

        currentField = null;
    }

    void OnValueSet(string newValue)
    {
        var formatted = FGUtils.FormatString(newValue, FGEntry.VALUE_WHITELIST);
        
        if (formatted == "") entry.Value = 0;
        else if (float.TryParse(formatted, out float outValue)) entry.Value = outValue;
        
        value.SetTextWithoutNotify(FGUtils.FormatLargeNumber(entry.Value, false));
        ValueCheck();

        if (valueChanged)
        {
            onSave?.Invoke();
            valueChanged = false;
        }

        currentField = null;
    }

    void OnIsCostSet(bool newValue)
    {
        entry.IsCost = newValue;
        isCost.SetIsOnWithoutNotify(entry.IsCost);
            
        ValueCheck();
            
        onSave?.Invoke();

        currentField = null;
    }

    void OnCategorySet(string newValue, bool setMatchingCategory)
    {
        if (newValue.Contains(HIGHLIGHTER))
            newValue = newValue.Remove(newValue.IndexOf(HIGHLIGHTER));

        if (setMatchingCategory && !string.IsNullOrEmpty(newValue))
        {
            var matching = FGManager.Instance.Database.GetMatchingCategory(newValue);
            if (matching != null) newValue = matching;
        }
        
        var formatted = FGUtils.FormatString(newValue, FGEntry.DESCRIPTION_WHITELIST);
        entry.Category = formatted;
        category.SetTextWithoutNotify(entry.Category);

        if (categoryChanged)
        {
            onSave?.Invoke();
            categoryChanged = false;
        }

        currentField = null;
    }

    void OnNoteSet(string newValue)
    {
        var formatted = FGUtils.FormatString(newValue, FGEntry.DESCRIPTION_WHITELIST);
        entry.Note = formatted;
        note.SetTextWithoutNotify(entry.Note);

        if (noteChanged)
        {
            onSave?.Invoke();
            noteChanged = false;
        }

        currentField = null;
    }

    void OnIgnoreSet(bool newValue)
    {
        entry.Ignore = newValue;
        ignore.SetIsOnWithoutNotify(entry.Ignore);

        IgnoreCheck();
            
        onSave?.Invoke();

        currentField = null;
    }
    
    #endregion

    void ValueCheck()
    {
        value.GetComponent<Image>().color = FGUtils.GraduatedColourLerp(
            Color.white,
            entry.IsCost ? FGUtils.NEGATIVE : FGUtils.POSITIVE,
            entry.Value / 3000f,
            6);
    }

    void IgnoreCheck()
    {
        date.interactable = !entry.Ignore;
        description.interactable = !entry.Ignore;
        value.interactable = !entry.Ignore;
        isCost.interactable = !entry.Ignore;
        category.interactable = !entry.Ignore;
        note.interactable = !entry.Ignore;
    }
    
    public void ModifyLineNumber(int amount = 0)
    {
        lineNumber += amount;
        line.text = lineNumber.ToString();
    }

    public void Select(int index) => fields[index].Select();
    
    void OnSubmit() => OnSubmitPressed?.Invoke(fields.IndexOf(currentField));

    public void Remove() => OnRemove?.Invoke(entry, true);
}