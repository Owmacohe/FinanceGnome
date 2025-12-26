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
        
        var dateSplit = split[0].Split('/');
        if (dateSplit.Length != 3) return;

        Date = new DateTime(
            int.Parse(dateSplit[2]),
            int.Parse(dateSplit[1]),
            int.Parse(dateSplit[0])); // TODO: error catching
        Description = split[1];
        Value = float.Parse(split[2]); // TODO: error catching
        IsCost = bool.Parse(split[3]); // TODO: error catching
        Category = split[4];
        Note = split[5];
        Ignore = bool.Parse(split[6]); // TODO: error catching
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

    public string DateToString() => $"{Date.Day}/{Date.Month}/{Date.Year}";
    
    public override string ToString() =>
        $"{DateToString()}," +
        $"{Description}," +
        $"{Value}," +
        $"{IsCost}," +
        $"{Category}," +
        $"{Note}," +
        $"{Ignore}";
}