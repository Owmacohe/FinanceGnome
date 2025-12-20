using System;
using System.Collections.Generic;
using System.Linq;

public class FGDatabase
{
    List<FGEntry> _entries = new();

    public List<FGEntry> Entries => _entries
        .OrderBy(entry => entry.Date)
        // .ThenBy(entry => entry.Category)
        // .ThenBy(entry => entry.Description)
        .ToList();

    public List<string> Categories => _entries
        .Select(entry => entry.Category)
        .Append("")
        .Distinct()
        .OrderBy(category => category)
        .ToList();

    public double TotalCosts => Entries.Sum(entry => entry.ValueOut);
    public double TotalIncome => Entries.Sum(entry => entry.ValueIn);
    public double TotalBalance => TotalIncome - TotalCosts;

    public FGDatabase(string[][] entries)
    {
        for (int i = 0; i < entries.GetLength(0); i++)
        {
            if (entries[i].Length >= 6)
            {
                _entries.Add(new FGEntry(
                    entries[i][0],
                    entries[i][1],
                    entries[i][2],
                    entries[i][3],
                    entries[i][4],
                    entries[i][5]));
            }
            else if (entries[i].Length >= 4)
            {
                _entries.Add(new FGEntry(
                    entries[i][0],
                    entries[i][1],
                    entries[i][2],
                    entries[i][3]));
            }
        }
    }

    public void AddEntry(FGEntry entry) => _entries.Add(entry);
    public void RemoveEntry(FGEntry entry) => _entries.Remove(entry);

    public override string ToString()
    {
        string temp = "";
        if (Entries.Count == 0) return temp;
        
        Entries.ForEach(entry => temp += entry + "\n");
        return temp.Substring(0, temp.Length - 1);
    }
}