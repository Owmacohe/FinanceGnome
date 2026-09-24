using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FGDatabase
{
    public string Name { get; set; }
    
    public List<FGImportRule> ImportRules { get; }
    public List<FGBudgetEntry> IncomeBudgetEntries { get; }
    public List<FGBudgetEntry> CostBudgetEntries { get; }
    
    public List<FGEntry> Entries { get; set; }
    public List<FGEntry> ValidEntries => Entries.Where(entry => !entry.Ignore).ToList();
    public List<FGEntry> SortedEntries => Entries.OrderBy(entry => entry.Date).ThenBy(entry => entry.Value).ToList();
    
    public List<string> Categories(bool includeInvalid) => (includeInvalid ? Entries : ValidEntries)
        .Select(entry => entry.Category)
        .Distinct()
        .OrderBy(category => category)
        .ToList();
    
    public float ValueTotal => ValidEntries.Sum(entry => entry.Value * (entry.IsCost ? -1 : 1));
    
    public FGDatabase(string name, string entries = "")
    {
        Name = name;
        ImportRules = new();
        IncomeBudgetEntries = new();
        CostBudgetEntries = new();
        Entries = new();
        
        if (!string.IsNullOrEmpty(entries))
        {
            foreach (var i in entries.Split('\n'))
            {
                if (!string.IsNullOrEmpty(i))
                {
                    var split = FGUtils.Split(i);
                    
                    if (int.TryParse(split[0], out var typeIndex) && typeIndex is >= 0 and <= 2)
                    {
                        var data = i.Substring(i.IndexOf(',') + 1);

                        switch (typeIndex)
                        {
                            case 0:
                                Entries.Add(new(data));
                                break;
                            
                            case 1:
                                ImportRules.Add(new(data));
                                break;
                            
                            case 2:
                                FGBudgetEntry newBudgetEntry = new(data);
                                
                                if (newBudgetEntry.IsCost) CostBudgetEntries.Add(newBudgetEntry);
                                else IncomeBudgetEntries.Add(newBudgetEntry);
                                break;
                        }
                    }
                    else
                    {
                        if (split.Count == 6) ImportRules.Add(new(i));
                        else if (split.Count == 7) Entries.Add(new(i));
                    }
                }
            }
        }
    }
    
    public List<FGEntry> Import(string entries)
    {
        List<FGEntry> temp = new();

        foreach (var i in entries.Split('\n'))
            if (!string.IsNullOrEmpty(i))
                temp.Add(new FGEntry(i));

        var ordered = temp.OrderBy(entry => entry.Date).ThenBy(entry => entry.Value).ToList();
        
        Entries.AddRange(ordered);

        return ordered;
    }
    
    public string GetMatchingCategory(string value)
    {
        var matching = Categories(true)
            .Where(category =>
                category.Length >= value.Length &&
                category.Substring(0, value.Length).ToLower() == value.ToLower())
            .ToList();

        return matching.Count > 0 ? matching[0] : null;
    }
    
    #region Months

    public int TotalMonths() => TotalMonthsInCategory(ValidEntries);

    public float TotalForMonth(int month, bool costs) => ValidEntries
        .Where(entry => entry.IsCost == costs && entry.Date.Month == month)
        .Sum(entry => entry.Value);
    
    public float TotalForMonthByCategory(List<FGEntry> categoryEntries, int month) =>
        categoryEntries.Where(entry => entry.Date.Month == month).Sum(entry => entry.Value);

    public int TotalEntriesForMonth(int month, bool costs) =>
        ValidEntries.Where(entry => entry.IsCost == costs && entry.Date.Month == month).ToList().Count;

    public List<FGEntry> EntriesInCategoryForMonth(List<FGEntry> categoryEntries, int month) =>
        categoryEntries.Where(entry => entry.Date.Month == month).ToList();
    
    #endregion
    
    #region Categories
    
    public List<FGEntry> EntriesInCategory(string category, bool costs) => ValidEntries
        .Where(entry => entry.Category == category && entry.IsCost == costs)
        .ToList();

    int TotalMonthsInCategory(List<FGEntry> categoryEntries) => categoryEntries.Count > 0
        ? categoryEntries.OrderByDescending(entry => entry.Date.Month).ToList()[0].Date.Month
        : 0;
    
    public float TotalForCategory(List<FGEntry> categoryEntries) =>
        categoryEntries.Sum(entry => entry.Value);

    public float AverageForCategoryByWeek(List<FGEntry> categoryEntries) =>
        categoryEntries.Sum(entry => entry.Value) / (TotalMonthsInCategory(categoryEntries) * (52f/12f));

    public float AverageForCategoryByMonth(List<FGEntry> categoryEntries) =>
        categoryEntries.Sum(entry => entry.Value) / TotalMonthsInCategory(categoryEntries);
    
    #endregion

    public override string ToString() =>
        $"{string.Join('\n', ImportRules)}\n" +
        $"{string.Join('\n', IncomeBudgetEntries)}\n" +
        $"{string.Join('\n', CostBudgetEntries)}\n" +
        $"{string.Join('\n', SortedEntries)}";
}