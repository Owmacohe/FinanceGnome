public class FGBudgetEntry
{
    public bool UseCategory { get; set; }
    public string Category { get; set; }
    
    public bool UseAverageValue { get; set; }
    public float ManualValue { get; set; }
    
    public bool IsCost { get; set; }
    public bool Track { get; set; }

    public FGBudgetEntry(string budgetEntry)
    {
        // TODO))
    }
}