using System;
using System.Collections.Generic;

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
        var split = Split(entry);
        if (split.Count != 7) return;

        var dateFormatted = FGUtils.FormatString(split[0], DATE_WHITELIST);
        Date = FGUtils.TryParseDateTime(dateFormatted, DateTime.Today);
        
        var descriptionFormatted = FGUtils.FormatString(split[1], DESCRIPTION_WHITELIST);
        Description = descriptionFormatted;
        
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

    List<string> Split(string s, char separator = ',', char exception = '"')
    {
        List<string> split = new List<string> {""};
        bool excepted = false;

        foreach (var i in s.Trim())
        {
            if (i == exception) excepted = !excepted;
            else if (i == separator && !excepted) split.Add("");
            else split[^1] += i;
        }

        return split;
    }
    
    public override string ToString() =>
        $"{FGUtils.DateToString(Date)}," +
        $"{Description}," +
        $"{Value}," +
        $"{IsCost}," +
        $"{Category}," +
        $"{Note}," +
        $"{Ignore}";

    public static string DATE_WHITELIST => $"/{FGUtils.NUMERIC}";
    public static string DESCRIPTION_WHITELIST => $"{FGUtils.ALPHANUMERIC}{FGUtils.SPECIAL}";
    public static string VALUE_WHITELIST => $".{FGUtils.NUMERIC}";
    public static string BOOL_WHITELIST => FGUtils.ALPHANUMERIC;
}