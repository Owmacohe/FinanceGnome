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
        var split = FGUtils.Split(budgetEntry);
        if (split.Count != 6) return;
        
        var useCategoryFormatted = FGUtils.FormatString(split[0], FGUtils.BOOL_WHITELIST);
        if (bool.TryParse(useCategoryFormatted, out bool outUseCategory)) UseCategory = outUseCategory;
        
        var categoryFormatted = FGUtils.FormatString(split[1], FGUtils.DESCRIPTION_WHITELIST);
        Category = categoryFormatted;
        
        var useAverageValueFormatted = FGUtils.FormatString(split[2], FGUtils.BOOL_WHITELIST);
        if (bool.TryParse(useAverageValueFormatted, out bool outUseAverageValue)) UseAverageValue = outUseAverageValue;
        
        var manualValueFormatted = FGUtils.FormatString(split[3], FGUtils.VALUE_WHITELIST);
        if (float.TryParse(manualValueFormatted, out float outManualValue)) ManualValue = outManualValue;
        
        var isCostFormatted = FGUtils.FormatString(split[4], FGUtils.BOOL_WHITELIST);
        if (bool.TryParse(isCostFormatted, out bool outIsCost)) IsCost = outIsCost;
        
        var trackFormatted = FGUtils.FormatString(split[5], FGUtils.BOOL_WHITELIST);
        if (bool.TryParse(trackFormatted, out bool outTrack)) Track = outTrack;
    }

    public FGBudgetEntry(
        bool useCategory = false, string category = "",
        bool useAverageValue = false, float manualValue = 0,
        bool isCost = false, bool track = false)
    {
        UseCategory = useCategory;
        Category = category;

        UseAverageValue = useAverageValue;
        ManualValue = manualValue;

        IsCost = isCost;
        Track = track;
    }

    public override string ToString() =>
        $"2," +
        $"{UseCategory}," +
        $"{Category}," +
        $"{UseAverageValue}," +
        $"{ManualValue}," +
        $"{IsCost}," +
        $"{Track}";
}