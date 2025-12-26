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

    public void Initialize(int lineNumber, FGEntry entry)
    {
        line.text = lineNumber.ToString();

        date.text = entry.DateToString();
        description.text = entry.Description; // TODO: format
        value.text = FGUtils.FormatNumber(entry.Value);
        isCost.isOn = entry.IsCost;
        category.text = entry.Category; // TODO: format
        note.text = entry.Note; // TODO: format
        ignore.isOn = entry.Ignore;
        
        date.onValueChanged.AddListener(newValue =>
        {
            var formatted = FGUtils.FormatString(newValue, $"/{FGUtils.NUMERIC}");
            // TODO: set date
            date.SetTextWithoutNotify(formatted);
        });
        
        description.onValueChanged.AddListener(newValue =>
        {
            var formatted = FGUtils.FormatString(newValue, $"{FGUtils.ALPHANUMERIC}{FGUtils.SPECIAL}");
            entry.Description = formatted;
            description.SetTextWithoutNotify(formatted);
        });
        
        value.onValueChanged.AddListener(newValue =>
        {
            var formatted = FGUtils.FormatString(newValue, $".{FGUtils.NUMERIC}");
            entry.Value = float.Parse(newValue); // TODO: error catching
            value.SetTextWithoutNotify(formatted);
        });
        
        isCost.onValueChanged.AddListener(newValue =>
        {
            entry.IsCost = newValue;
            isCost.SetIsOnWithoutNotify(newValue);
        });
        
        category.onValueChanged.AddListener(newValue =>
        {
            var formatted = FGUtils.FormatString(newValue, $"{FGUtils.ALPHANUMERIC}{FGUtils.SPECIAL}");
            entry.Category = formatted;
            category.SetTextWithoutNotify(formatted);
        });
        
        note.onValueChanged.AddListener(newValue =>
        {
            var formatted = FGUtils.FormatString(newValue, $"{FGUtils.ALPHANUMERIC}{FGUtils.SPECIAL}");
            entry.Note = formatted;
            note.SetTextWithoutNotify(formatted);
        });
        
        ignore.onValueChanged.AddListener(newValue =>
        {
            entry.Ignore = newValue;
            ignore.SetIsOnWithoutNotify(newValue);
        });
    }
}