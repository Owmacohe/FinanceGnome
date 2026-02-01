using System.Collections.Generic;
using System.Linq;

public class FGDatabase
{
    public string Name { get; set; }
    
    public List<FGImportRule> ImportRules { get; }
    
    public List<FGEntry> Entries { get; }
    public List<FGEntry> ValidEntries => Entries.Where(entry => !entry.Ignore).ToList();
    public List<FGEntry> SortedEntries => Entries.OrderBy(entry => entry.Date).ThenBy(entry => entry.Value).ToList();
    
    public List<string> Categories => ValidEntries
        .Select(entry => entry.Category)
        .Distinct()
        .OrderBy(category => category)
        .ToList();
    
    public float ValueTotal => ValidEntries.Sum(entry => entry.Value * (entry.IsCost ? -1 : 1));
    
    public FGDatabase(string name, string entries = "")
    {
        Name = name;
        ImportRules = new();
        Entries = new();
        
        if (!string.IsNullOrEmpty(entries))
        {
            foreach (var i in entries.Split('\n'))
            {
                if (!string.IsNullOrEmpty(i))
                {
                    int count = FGUtils.Split(i).Count;

                    if (count == 6) ImportRules.Add(new(i));
                    else if (count == 7) Entries.Add(new(i));
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
        var matching = Categories
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

    public override string ToString() => $"{string.Join('\n', ImportRules)}\n{string.Join('\n', SortedEntries)}";
}