using System;
using System.Collections.Generic;
using UnityEngine;

public class FGEntry
{
    public DateTime Date { get; set; }
    public string Description { get; set; }
    public float Value { get; set; }
    public bool IsCost { get; set; }
    public string Category { get; set; }
    public string Note { get; set; }
    public bool Ignore { get; set; }

    public FGEntry(string entry)
    {
        var split = FGUtils.Split(entry);
        if (split.Count < 4) return;

        var dateFormatted = FGUtils.FormatString(split[0], DATE_WHITELIST);
        var temp = FGUtils.TryParseDateTime(dateFormatted, DateTime.Today, out var failed);
        if (!failed) Date = temp;
        
        var descriptionFormatted = FGUtils.FormatString(split[1], DESCRIPTION_WHITELIST);
        Description = descriptionFormatted;
        
        if (split.Count == 7)
        {
            var valueFormatted = FGUtils.FormatString(split[2], VALUE_WHITELIST);
            if (float.TryParse(valueFormatted, out float outValue)) Value = outValue;
            
            var isCostFormatted = FGUtils.FormatString(split[3], BOOL_WHITELIST);
            IsCost = !bool.TryParse(isCostFormatted, out bool outIsCost) || outIsCost;
        
            var categoryFormatted = FGUtils.FormatString(split[4], DESCRIPTION_WHITELIST);
            Category = categoryFormatted;
        
            var noteFormatted = FGUtils.FormatString(split[5], DESCRIPTION_WHITELIST);
            Note = noteFormatted;
        
            var ignoreFormatted = FGUtils.FormatString(split[6], BOOL_WHITELIST);
            if (bool.TryParse(ignoreFormatted, out bool outIgnore)) Ignore = outIgnore;
        }
        else
        {
            if (string.IsNullOrEmpty(split[3]))
            {
                var valueFormatted = FGUtils.FormatString(split[2], VALUE_WHITELIST);
                if (float.TryParse(valueFormatted, out float outValue)) Value = outValue;
                
                IsCost = true;
            }
            else
            {
                var valueFormatted = FGUtils.FormatString(split[3], VALUE_WHITELIST);
                if (float.TryParse(valueFormatted, out float outValue)) Value = outValue;
                
                IsCost = false;
            }

            Category = "";
            Note = "";
            Ignore = false;
        }
    }

    public FGEntry(DateTime date, string description = "", float value = 0, bool isCost = true, string category = "", string note = "", bool ignore = false)
    {
        Date = date;
        Description = description;
        Value = value;
        IsCost = isCost;
        Category = category;
        Note = note;
        Ignore = ignore;
    }
    
    public override string ToString() =>
        $"{FGUtils.DateToString(Date)}," +
        $"{Description}," +
        $"{Value}," +
        $"{IsCost}," +
        $"{Category}," +
        $"{Note}," +
        $"{Ignore}";

    public static string DATE_WHITELIST => $"/-{FGUtils.NUMERIC}";
    public static string DESCRIPTION_WHITELIST => $"{FGUtils.ALL}";
    public static string VALUE_WHITELIST => $".{FGUtils.NUMERIC}";
    public static string BOOL_WHITELIST => FGUtils.ALPHANUMERIC;
}