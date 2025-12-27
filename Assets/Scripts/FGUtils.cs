using System;
using System.Linq;
using UnityEngine;

public static class FGUtils
{
    public static string ALPHA => "abcdefghijklmnopqrstuvwxyz" +
                                  "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    public static string NUMERIC => "0123456789";
    public static string SPECIAL => "!@#$%^&*()-_=+[]{}\\|;:<>./? ";
    public static string ALPHANUMERIC => ALPHA + NUMERIC;
    
    public static string FormatString(string s, string whitelist, string blacklist = "", string remove = "")
    {
        return string.Join("", (string.IsNullOrEmpty(remove) ? s : s.Replace(remove, ""))
            .Where(c => !blacklist.Contains(c))
            .Where(c => whitelist.Contains(c)));
    }

    public static string FormatLargeNumber(float value)
    {
        var split = value.ToString().Split('.');
        var dollars = "";

        for (int i = 0; i < split[0].Length; i++)
        {
            if (i != 0 && i % 3 == 0) dollars = dollars.Insert(0, ",");

            dollars = dollars.Insert(0, split[0][split[0].Length - 1 - i].ToString());
        }

        return split.Length > 1 ? $"{dollars}.{split[1]}" : $"{dollars}.00";
    }
    
    public static string DateToString(DateTime value) => $"{value.Day:00}/{value.Month:00}/{value.Year:0000}";
    
    public static DateTime TryParseDateTime(string value, DateTime fallback)
    {
        try
        {
            var split = value.Split('/');

            return new DateTime(
                int.Parse(split[2]),
                int.Parse(split[1]),
                int.Parse(split[0]));
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return fallback;
        }
    }
}