using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class FGBalanceSheetScreenPanel : MonoBehaviour
{
    [SerializeField] Transform balanceSheetParent;
    [SerializeField] TMP_Text balanceSheetCell;

    FGManager manager;

    public void Initialize()
    {
        manager = FGManager.Instance;
    }
    
    public void InstantiateBalanceSheetCells()
    {
        AddBlankBalanceSheetRow();
        AddHeaderBalanceSheetRow("Costs");
        AddCategoryEntries(true);
        AddMonthlyTotals(true);

        AddBlankBalanceSheetRow();
        AddHeaderBalanceSheetRow("Income");
        AddCategoryEntries(false);
        AddMonthlyTotals(false);

        AddBlankBalanceSheetRow();
        AddBalanceBalanceSheetRow();
    }
    
    #region AddBalanceSheetRow
    
    List<TMP_Text> AddBalanceSheetRow(List<string> row)
    {
        if (row.Count != 15)
        {
            Debug.LogError($"Row of size {row.Count} is not valid (must be of size 15)");
            return null;
        }

        List<TMP_Text> temp = new();

        foreach (var i in row)
        {
            var cell = Instantiate(balanceSheetCell, balanceSheetParent);
            cell.text = string.IsNullOrEmpty(i) ? "" : i;
            cell.name = cell.text;
            temp.Add(cell);
        }

        return temp;
    }
    
    void AddBlankBalanceSheetRow() => AddBalanceSheetRow((new string[15]).ToList());

    void AddHeaderBalanceSheetRow(string header)
    {
        var temp = new string[15];
        temp[0] = $"<b><i>{header}</i></b>";
        
        AddBalanceSheetRow(temp.ToList());
    }

    void AddCategoryEntries(bool costs)
    {
        var colourMax = costs ? Color.red : Color.green;

        Dictionary<string, List<TMP_Text>> temp = new();
        
        foreach (var i in manager.Database.Categories)
        {
            List<string> row = new();

            var entries = manager.Database.EntriesInCategory(i, costs);
            if (entries.Count == 0 || manager.Database.TotalForCategory(entries) == 0) continue;
            
            row.Add(string.IsNullOrEmpty(i) ? "N/A" : i);

            for (int j = 0; j < 12; j++)
                row.Add(manager.Database.TotalEntriesInCategoryForMonth(entries, j + 1) == 0 ? "-" :
                    FGUtils.FormatLargeNumber(
                        manager.Database.TotalForMonthByCategory(entries, j + 1),
                        true,
                        colourMax,
                        3000));
            
            row.Add(FGUtils.FormatLargeNumber(
                manager.Database.AverageForCategoryByWeek(entries),
                true,
                colourMax,
                500));
            
            row.Add(FGUtils.FormatLargeNumber(
                manager.Database.AverageForCategoryByMonth(entries),
                true,
                colourMax,
                2000));

            AddBalanceSheetRow(row);
        }
    }

    void AddMonthlyTotals(bool costs)
    {
        var colourMax = costs ? Color.red : Color.green;
        
        List<string> row = new();
        row.Add("<b><i>Total</i></b>");

        for (int i = 0; i < 12; i++)
            row.Add(manager.Database.TotalEntriesForMonth(i + 1, costs) == 0 ? "-" :
                FGUtils.FormatLargeNumber(
                    manager.Database.TotalForMonth(i + 1, costs),
                    true,
                    colourMax,
                    3000));
        
        row.Add("");
        row.Add("");
        
        AddBalanceSheetRow(row);
    }

    void AddBalanceBalanceSheetRow()
    {
        List<string> row = new();
            
        row.Add("<b>Balance</b>");

        for (int i = 0; i < 12; i++)
        {
            if (manager.Database.TotalEntriesForMonth(i + 1, true) == 0 &&
                manager.Database.TotalEntriesForMonth(i + 1, false) == 0)
            {
                row.Add("-");
                continue;
            }
            
            var incomeTotal = manager.Database.TotalForMonth(i + 1, false);
            var costsTotal = manager.Database.TotalForMonth(i + 1, true);
            float balance = incomeTotal - costsTotal;
            
            row.Add(FGUtils.FormatLargeNumber(
                balance,
                true,
                balance >= 0 ? Color.green : Color.red,
                10000));
        }
        
        var weeklyTotal = manager.Database.ValueTotal / (manager.Database.TotalMonths() * (52f/12f));
        var monthlyTotal = manager.Database.ValueTotal / manager.Database.TotalMonths();
        
        row.Add(FGUtils.FormatLargeNumber(
            weeklyTotal,
            true,
            weeklyTotal >= 0 ? Color.green : Color.red,
            1000));
        
        row.Add(FGUtils.FormatLargeNumber(
            monthlyTotal,
            true,
            monthlyTotal >= 0 ? Color.green : Color.red,
            7500));
            
        AddBalanceSheetRow(row);
    }
    
    #endregion

    public void RefreshBalanceSheetRows()
    {
        for (int i = 0; i < balanceSheetParent.childCount - 15; i++)
            Destroy(balanceSheetParent.GetChild(15 + i).gameObject);
        
        InstantiateBalanceSheetCells();

        manager.TransactionsChanged = false;
    }
}