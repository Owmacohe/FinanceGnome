using System;
using System.Linq;
using UnityEngine;

public static class FGUtils
{
    public static float RoundTo(float f, int decimalPoints)
    {
        float multiplier = Mathf.Pow(10, decimalPoints);
        return Mathf.Round(f * multiplier) / multiplier;
    }

    public static Color GraduatedColourLerp(Color a, Color b, float amount, int graduations)
    {
        if (amount <= 0) return a;
        if (amount >= 1) return b;
        
        float graduationAmount = 1f / graduations;
        float lerpAmount = 0;

        while (amount > lerpAmount)
            lerpAmount += graduationAmount;
        
        return Color.Lerp(a, b, lerpAmount);
    }
    
    #region Formatting
    
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
    
    public static string FormatLargeNumber(float value, bool addDollarSign, Color colourMax, float amountMax)
    {
        var colour = ColorUtility.ToHtmlStringRGB(GraduatedColourLerp(
            Color.white,
            colourMax,
            Mathf.Abs(value) / amountMax,
            4));
        
        return $"<color=#{colour}>{FormatLargeNumber(value, addDollarSign)}</color>";
    }

    public static string FormatLargeNumber(float value, bool addDollarSign)
    {
        value = RoundTo(value, 2);
        
        var split = value.ToString().Split('.');
        var dollars = "";

        for (int i = 0; i < split[0].Length; i++)
        {
            var digit = split[0][split[0].Length - 1 - i];
            
            if (i != 0 && i % 3 == 0 && NUMERIC.Contains(digit))
                dollars = dollars.Insert(0, ",");

            dollars = dollars.Insert(0, digit.ToString());
        }
        
        return split.Length > 1
            ? $"{(addDollarSign ? "$" : "")}{dollars}.{split[1]}"
            : $"{(addDollarSign ? "$" : "")}{dollars}.00";
    }
    
    #endregion
    
    #region DateTime
    
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
    
    #endregion
}