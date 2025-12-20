using System;
using System.Collections.Generic;
using System.Linq;

public static class Utils
{
    public static double Round(double value, int decimalPlaces)
    {
        double multiplier = Math.Pow(10, decimalPlaces);
        return Math.Round(value * multiplier) / multiplier;
    }
    
    public static string FormatValue(double value)
    {
        value = Round(value, 2);
        
        string temp = value.ToString();
        var split = temp.Split('.');
        var dollars = "";

        for (int i = 0; i < split[0].Length; i++)
        {
            dollars = dollars.Insert(0, split[0][split[0].Length - 1 - i].ToString());
            
            if (i != split[0].Length - 1 &&
                !(i == split[0].Length - 2 && value < 0) &&
                (i + 1) % 3 == 0)
                dollars = dollars.Insert(0, ",");
        }

        return split.Length > 1 ? $"{dollars}.{split[1]}" : dollars;
    }
    
    public static string FormatString(string? str, string remove)
    {
        if (str == null) return "";
        
        string temp = str.Trim();

        foreach (var i in remove)
            temp = temp.Replace(i.ToString(), "");
        
        return temp;
    }

    public static string FormatString(object? obj, string remove) => obj == null ? "" : FormatString(obj.ToString(), remove);

    public static List<string> Split(string line, char separator, char ignore = '"')
    {
        bool ignoreSeparator = false;
        List<string> split = new List<string> { "" };

        foreach (var i in line)
        {
            if (i == ignore) ignoreSeparator = !ignoreSeparator;

            if (i == separator && !ignoreSeparator) split.Add("");
            else split[^1] += i;
        }

        return split;
    }
}