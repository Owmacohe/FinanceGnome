using System.Collections.Generic;

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

    public override string ToString() => string.Join('\n', Entries);
}