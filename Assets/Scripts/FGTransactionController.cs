using System;
using TMPro;
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

    FGEntry entry;
    Action onSave;

    const string HIGHLIGHTER = "<mark=#A8CEFF>";

    public void Initialize(int lineNumber, FGEntry entry, Action onSave)
    {
        this.entry = entry;
        this.onSave = onSave;
        
        line.text = lineNumber.ToString();

        date.text = FGUtils.DateToString(entry.Date);
        description.text = entry.Description;
        value.text = FGUtils.FormatLargeNumber(entry.Value);
        isCost.isOn = entry.IsCost;
        category.text = entry.Category;
        note.text = entry.Note;
        ignore.isOn = entry.Ignore;
        
        date.onValueChanged.AddListener(_ => dateChanged = true);
        description.onValueChanged.AddListener(_ => descriptionChanged = true);
        value.onValueChanged.AddListener(_ => valueChanged = true);
        category.onValueChanged.AddListener(newValue =>
        {
            if (newValue.Contains(HIGHLIGHTER))
                newValue = newValue.Remove(newValue.IndexOf(HIGHLIGHTER));

            string matching = FGManager.Database.GetMatchingCategory(newValue);
            category.SetTextWithoutNotify(
                newValue +
                (matching != null && !string.IsNullOrEmpty(newValue)
                    ? $"{HIGHLIGHTER}{matching.Substring(newValue.Length)}"
                    : ""));
            
            categoryChanged = true;
        });
        note.onValueChanged.AddListener(_ => noteChanged = true);
        
        date.onDeselect.AddListener(OnDateSet);
        date.onSubmit.AddListener(OnDateSet);
        
        description.onDeselect.AddListener(OnDescriptionSet);
        description.onSubmit.AddListener(OnDescriptionSet);
        
        value.onDeselect.AddListener(OnValueSet);
        value.onSubmit.AddListener(OnValueSet);
        
        isCost.onValueChanged.AddListener(newValue =>
        {
            entry.IsCost = newValue;
            isCost.SetIsOnWithoutNotify(entry.IsCost);
            
            onSave?.Invoke();
        });
        
        category.onDeselect.AddListener(newValue => OnCategorySet(newValue, false));
        category.onSubmit.AddListener(newValue => OnCategorySet(newValue, true));
        
        note.onDeselect.AddListener(OnNoteSet);
        note.onSubmit.AddListener(OnNoteSet);
        
        ignore.onValueChanged.AddListener(newValue =>
        {
            entry.Ignore = newValue;
            ignore.SetIsOnWithoutNotify(entry.Ignore);
            
            onSave?.Invoke();
        });
    }

    void OnDateSet(string newValue)
    {
        var formatted = FGUtils.FormatString(newValue, FGEntry.DATE_WHITELIST);
        entry.Date = FGUtils.TryParseDateTime(formatted, entry.Date);
        date.SetTextWithoutNotify(FGUtils.DateToString(entry.Date));

        if (dateChanged)
        {
            onSave?.Invoke();
            dateChanged = false;
        }
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
    }

    void OnValueSet(string newValue)
    {
        var formatted = FGUtils.FormatString(newValue, FGEntry.VALUE_WHITELIST);
        
        if (formatted == "") entry.Value = 0;
        else if (float.TryParse(formatted, out float outValue)) entry.Value = outValue;
        
        value.SetTextWithoutNotify(FGUtils.FormatLargeNumber(entry.Value));

        if (valueChanged)
        {
            onSave?.Invoke();
            valueChanged = false;
        }
    }

    void OnCategorySet(string newValue, bool setMatchingCategory)
    {
        if (newValue.Contains(HIGHLIGHTER))
            newValue = newValue.Remove(newValue.IndexOf(HIGHLIGHTER));

        if (setMatchingCategory && !string.IsNullOrEmpty(newValue))
        {
            var matching = FGManager.Database.GetMatchingCategory(newValue);
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
    }
}