public class FGBudgetEntry
{
    public string Category { get; set; }
    
    public bool UseAverageValue { get; set; }
    public float ManualValue { get; set; }
    
    public float CurrentValue { get; set; }
    
    public bool IsCost { get; set; }
    public bool Essential { get; set; }

    public FGBudgetEntry(string budgetEntry)
    {
        var split = FGUtils.Split(budgetEntry);
        if (split.Count != 5) return;
        
        var categoryFormatted = FGUtils.FormatString(split[0], FGUtils.DESCRIPTION_WHITELIST);
        Category = categoryFormatted;
        
        var useAverageValueFormatted = FGUtils.FormatString(split[1], FGUtils.BOOL_WHITELIST);
        if (bool.TryParse(useAverageValueFormatted, out bool outUseAverageValue)) UseAverageValue = outUseAverageValue;
        
        var manualValueFormatted = FGUtils.FormatString(split[2], FGUtils.VALUE_WHITELIST);
        if (float.TryParse(manualValueFormatted, out float outManualValue)) ManualValue = outManualValue;
        
        var isCostFormatted = FGUtils.FormatString(split[3], FGUtils.BOOL_WHITELIST);
        if (bool.TryParse(isCostFormatted, out bool outIsCost)) IsCost = outIsCost;
        
        var essentialFormatted = FGUtils.FormatString(split[4], FGUtils.BOOL_WHITELIST);
        if (bool.TryParse(essentialFormatted, out bool outEssential)) Essential = outEssential;
    }

    public FGBudgetEntry(
        string category = "",
        bool useAverageValue = false, float manualValue = 0,
        bool isCost = false, bool essential = true)
    {
        Category = category;

        UseAverageValue = useAverageValue;
        ManualValue = manualValue;

        IsCost = isCost;
        Essential = essential;
    }

    public override string ToString() =>
        $"2," +
        $"{Category}," +
        $"{UseAverageValue}," +
        $"{ManualValue}," +
        $"{IsCost}," +
        $"{Essential}";
}