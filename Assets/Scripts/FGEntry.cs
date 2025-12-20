using System;

public class FGEntry
{
    public DateTime Date { get; set; }
    public string Description { get; set; }
    public double ValueOut { get; set; }
    public double ValueIn { get; set; }
    public string Category { get; set; }
    public string Note { get; set; }
    
    // public FGEntryRow Row { get; set; }
    
    #region Constructors
    
    public FGEntry(DateTime date, string description)
    {
        Date = date;
        Description = description;
        ValueOut = 0;
        ValueIn = 0;
        Category = "";
        Note = "";
    }

    public FGEntry(string date, string description, string valueOut, string valueIn, string category = "", string note = "", bool reversedDate = false)
    {
        SetDate(date, reversedDate);
        Description = description;
        SetValueOut(valueOut);
        SetValueIn(valueIn);
        Category = category;
        Note = note;
    }
    
    #endregion
    
    #region Setters & Getters

    public void SetDate(string date, bool reversedOrder = false)
    {
        if (string.IsNullOrEmpty(date)) return;

        date = Utils.FormatString(date, ",");
        
        var dateSplit = date.Split('-');
        if (dateSplit.Length != 3) dateSplit = date.Split('/');
        
        if (dateSplit.Length != 3 ||
            dateSplit[0].Length == 0 ||
            dateSplit[1].Length == 0 ||
            dateSplit[2].Length == 0) return;
        
        try
        {
            Date = !reversedOrder
                ? new DateTime(int.Parse(dateSplit[0]), int.Parse(dateSplit[1]), int.Parse(dateSplit[2]))
                : new DateTime(int.Parse(dateSplit[2]), int.Parse(dateSplit[1]), int.Parse(dateSplit[0]));
        }
        catch { }
    }

    public string GetDate() => $"{Date.Year}-{Date.Month}-{Date.Day}";

    public void SetValueOut(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            ValueOut = 0;
            return;
        }
        
        try
        {
            var temp = double.Parse(value);
            if (temp < 0) temp *= -1;
            ValueOut = temp;
        }
        catch (FormatException) { }
    }

    public void SetValueIn(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            ValueIn = 0;
            return;
        }
        
        try
        {
            var temp = double.Parse(value);
            if (temp < 0) temp *= -1;
            ValueIn = temp;
        }
        catch (FormatException) { }
    }
    
    #endregion

    public override string ToString() =>
        $"{GetDate()}," +
        $"{Description}," +
        $"{ValueOut}," +
        $"{ValueIn}," +
        $"{Category}," +
        $"{Note}";
}