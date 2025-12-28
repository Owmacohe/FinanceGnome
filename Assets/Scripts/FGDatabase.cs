using System.Collections.Generic;
using System.Linq;

public class FGDatabase
{
    public string Name { get; set; }
    
    public List<FGEntry> Entries { get; }
    public List<FGEntry> ValidEntries => Entries.Where(entry => !entry.Ignore).ToList();
    public List<string> Categories => ValidEntries
        .Select(entry => entry.Category)
        .Distinct()
        .OrderBy(category => category)
        .ToList();
    public float ValueTotal => ValidEntries.Sum(entry => entry.Value * (entry.IsCost ? -1 : 1));
    
    public FGDatabase(string name, string entries = "")
    {
        Name = name;
        Entries = new();
        
        foreach (var i in entries.Split('\n'))
            if (!string.IsNullOrEmpty(i))
                Entries.Add(new(i));
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

    public float TotalForMonth(int month, bool costs) => ValidEntries
        .Where(entry => entry.IsCost == costs && entry.Date.Month == month)
        .Sum(entry => entry.Value);
    
    public float TotalForMonthByCategory(List<FGEntry> categoryEntries, int month) =>
        categoryEntries.Where(entry => entry.Date.Month == month).Sum(entry => entry.Value);
    
    #endregion
    
    #region Categories
    
    public List<FGEntry> EntriesInCategory(string category, bool costs) => ValidEntries
        .Where(entry => entry.Category == category && entry.IsCost == costs)
        .ToList();
    
    public float TotalForCategory(List<FGEntry> categoryEntries) =>
        categoryEntries.Sum(entry => entry.Value);

    public float AverageForCategoryByWeek(List<FGEntry> categoryEntries) =>
        categoryEntries.Sum(entry => entry.Value) / 52f;

    public float AverageForCategoryByMonth(List<FGEntry> categoryEntries) =>
        categoryEntries.Sum(entry => entry.Value) / 12f;
    
    #endregion

    public override string ToString() => string.Join('\n', Entries);
}