using System;
using UnityEngine;

public enum FGImportRuleIfProperty
{
    Description, Date, Value, IsCost
}

public enum FGImportRuleThenProperty
{
    Category, Date, Description, Value, IsCost, Note, Ignore
}

public enum FGImportRuleComparator
{
    GreaterThanOrEquals,
    Equals, NotEquals,
    GreaterThan,
    LessThan, LessThanOrEquals
}

public class FGImportRule
{
    public FGImportRuleIfProperty ifProperty { get; set; }
    public FGImportRuleComparator Comparator { get; set; }
    public string Comparison { get; set; }
    
    public FGImportRuleThenProperty thenProperty { get; set; }
    public string Result { get; set; }

    public FGImportRule(string importRule)
    {
        var split = FGUtils.Split(importRule);
        if (split.Count != 5) return;
        
        var ifPropertyFormatted = FGUtils.FormatString(split[0], FGUtils.ALL);
        if (Enum.TryParse(typeof(FGImportRuleIfProperty), ifPropertyFormatted, out var outIfProperty))
            ifProperty = (FGImportRuleIfProperty) outIfProperty;
        
        var comparatorFormatted = FGUtils.FormatString(split[1], FGUtils.ALL);
        if (Enum.TryParse(typeof(FGImportRuleComparator), comparatorFormatted, out var outComparator))
            Comparator = (FGImportRuleComparator) outComparator;
        
        var comparisonFormatted = FGUtils.FormatString(split[2], FGUtils.ALL);
        Comparison = comparisonFormatted;
        
        var thenPropertyFormatted = FGUtils.FormatString(split[3], FGUtils.ALL);
        if (Enum.TryParse(typeof(FGImportRuleThenProperty), thenPropertyFormatted, out var outThenProperty))
            thenProperty = (FGImportRuleThenProperty) outThenProperty;
        
        var resultFormatted = FGUtils.FormatString(split[4], FGUtils.ALL);
        Result = resultFormatted;
    }

    public FGImportRule(
        FGImportRuleIfProperty ifProperty = FGImportRuleIfProperty.Description, FGImportRuleComparator comparator = FGImportRuleComparator.GreaterThanOrEquals, string comparison = "",
        FGImportRuleThenProperty thenProperty = FGImportRuleThenProperty.Category, string result = "")
    {
        this.ifProperty = ifProperty;
        Comparator = comparator;
        Comparison = comparison;

        this.thenProperty = thenProperty;
        Result = result;
    }

    public void CheckEntry(FGEntry entry)
    {
        if (EntryMatchesRule(entry))
        {
            switch (thenProperty)
            {
                case FGImportRuleThenProperty.Date:
                    var parsedDate = FGUtils.TryParseDateTime(Result, DateTime.Today, out var failed);
                    if (!failed) entry.Date = parsedDate;
                    break;
                
                case FGImportRuleThenProperty.Description:
                    entry.Description = Result;
                    break;
                
                case FGImportRuleThenProperty.Value:
                    if (float.TryParse(Result, out var parsedValue))
                        entry.Value = parsedValue;
                    break;
                
                case FGImportRuleThenProperty.IsCost:
                    if (bool.TryParse(Result, out var parsedIsCost))
                        entry.IsCost = parsedIsCost;
                    break;
                
                case FGImportRuleThenProperty.Category:
                    entry.Category = Result;
                    break;
                
                case FGImportRuleThenProperty.Note:
                    entry.Note = Result;
                    break;
                
                case FGImportRuleThenProperty.Ignore:
                    if (bool.TryParse(Result, out var parsedIgnore))
                        entry.Ignore = parsedIgnore;
                    break;
            }
        }
    }

    bool EntryMatchesRule(FGEntry entry)
    {
        switch (ifProperty)
        {
            case FGImportRuleIfProperty.Date:
                var parsedDate = FGUtils.TryParseDateTime(Comparison, DateTime.Today, out var failed);
                if (failed) return false;
                
                switch (Comparator)
                {
                    case FGImportRuleComparator.Equals: return FGUtils.DateToString(entry.Date).Equals(Comparison);
                    case FGImportRuleComparator.NotEquals: return !FGUtils.DateToString(entry.Date).Equals(Comparison);
                    case FGImportRuleComparator.GreaterThan: return entry.Date > parsedDate;
                    case FGImportRuleComparator.GreaterThanOrEquals: return entry.Date >= parsedDate;
                    case FGImportRuleComparator.LessThan: return entry.Date > parsedDate;
                    case FGImportRuleComparator.LessThanOrEquals: return entry.Date >= parsedDate;
                }
                break;
            
            case FGImportRuleIfProperty.Description:
                switch (Comparator)
                {
                    case FGImportRuleComparator.Equals: return entry.Description.Equals(Comparison);
                    case FGImportRuleComparator.NotEquals: return !entry.Description.Equals(Comparison);
                    case FGImportRuleComparator.GreaterThan: return !entry.Description.Replace(Comparison, "").Equals("");
                    case FGImportRuleComparator.GreaterThanOrEquals: return entry.Description.Contains(Comparison);
                    case FGImportRuleComparator.LessThan: return !Comparison.Replace(entry.Description, "").Equals("");
                    case FGImportRuleComparator.LessThanOrEquals: return Comparison.Contains(entry.Description);
                }
                break;
            
            case FGImportRuleIfProperty.Value:
                if (!float.TryParse(Comparison, out var parsedValue)) return false;
                
                switch (Comparator)
                {
                    case FGImportRuleComparator.Equals: return Mathf.Approximately(entry.Value, parsedValue);
                    case FGImportRuleComparator.NotEquals: return !Mathf.Approximately(entry.Value, parsedValue);
                    case FGImportRuleComparator.GreaterThan: return entry.Value > parsedValue;
                    case FGImportRuleComparator.GreaterThanOrEquals: return entry.Value >= parsedValue;
                    case FGImportRuleComparator.LessThan: return entry.Value < parsedValue;
                    case FGImportRuleComparator.LessThanOrEquals: return entry.Value <= parsedValue;
                }
                break;
            
            case FGImportRuleIfProperty.IsCost:
                if (!bool.TryParse(Comparison, out var parsedIsCost)) return false;
                
                switch (Comparator)
                {
                    case FGImportRuleComparator.Equals: return entry.IsCost.Equals(parsedIsCost);
                    case FGImportRuleComparator.NotEquals: return !entry.IsCost.Equals(parsedIsCost);
                    case FGImportRuleComparator.GreaterThan:
                    case FGImportRuleComparator.GreaterThanOrEquals:
                    case FGImportRuleComparator.LessThan:
                    case FGImportRuleComparator.LessThanOrEquals: return false;
                }
                break;
        }

        return false;
    }

    public override string ToString() =>
        $"{ifProperty}," +
        $"{Comparator}," +
        $"{Comparison}," +
        $"{thenProperty}," +
        $"{Result}";
}