using System.Collections.Generic;
using System.Linq;

public class FGDatabase
{
    public string Name { get; set; }
    
    public List<FGEntry> Entries { get; }

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
        var matching = Entries
            .Select(entry => entry.Category)
            .Where(category =>
                !string.IsNullOrEmpty(category) &&
                category.Length >= value.Length &&
                category.Substring(0, value.Length).ToLower() == value.ToLower())
            .ToList();

        return matching.Count > 0 ? matching[0] : null;
    }

    public override string ToString() => string.Join('\n', Entries);
}