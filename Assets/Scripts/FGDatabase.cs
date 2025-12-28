using System.Collections.Generic;
using System.Linq;

public class FGDatabase
{
    public string Name { get; set; }
    
    public List<FGEntry> Entries { get; }
    public List<string> Categories => Entries
        .Select(entry => entry.Category)
        .Distinct()
        .OrderBy(category => category)
        .ToList();
    
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
    
    public List<FGEntry> CategoryEntries(string category, bool costs) =>
        Entries.Where(entry => entry.Category == category &&
                               entry.IsCost == costs &&
                               !entry.Ignore).ToList();

    public float CategoryTotalForMonth(List<FGEntry> categoryEntries, int month) =>
        categoryEntries.Where(entry => entry.Date.Month == month).Sum(entry => entry.Value);

    public float CategoryAverageByWeek(List<FGEntry> categoryEntries) =>
        categoryEntries.Sum(entry => entry.Value) / 52;

    public float CategoryAverageByMonth(List<FGEntry> categoryEntries) =>
        categoryEntries.Sum(entry => entry.Value) / 12;

    public override string ToString() => string.Join('\n', Entries);
}