using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NativeFileBrowser;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FGTransactionsScreenPanel : MonoBehaviour
{
    [SerializeField] Transform transactionsParent;
    [SerializeField] FGTransactionController transactionPrefab;
    [SerializeField] Scrollbar scrollbar;

    [Header("Months")]
    [SerializeField] Button previousMonth;
    [SerializeField] TMP_Text month;
    [SerializeField] Button nextMonth;
    
    List<FGTransactionController> transactions = new();
    int addEntriesAmount = 1;
    
    FGManager manager;
    int currentMonth;

    const string SCROLL_AMOUNT = "scrollAmount";
    bool saveScroll = true;

    public void Initialize()
    {
        manager = FGManager.Instance;
    }

    public void Refresh(int month)
    {
        foreach (var i in transactions)
            Destroy(i.gameObject);
        
        transactions.Clear();

        manager.Database.Entries = manager.Database.SortedEntries;

        InstantiateTransactions(month);
        
        OnValueChanged();
    }
    
    public void InstantiateTransactions(int month)
    {
        currentMonth = month;
        
        this.month.text = FGUtils.GetMonth(month);

        var hasPreviousMonth = month > 0 &&
                               (manager.Database.TotalEntriesForMonth(month - 1, true) +
                                manager.Database.TotalEntriesForMonth(month - 1, false) > 0);
        previousMonth.interactable = hasPreviousMonth;
        
        var hasNextMonth = month < 12 &&
                               (manager.Database.TotalEntriesForMonth(month + 1, true) +
                                manager.Database.TotalEntriesForMonth(month + 1, false) > 0);
        nextMonth.interactable = hasNextMonth;
        
        var entries = manager.Database.Entries.Where(entry => entry.Date.Month == month).ToList();
        
        for (int i = 0; i < entries.Count; i++)
            AddTransaction(entries[i], i + 1, false);
    }
    
    public void RefreshTransactions() => transactions.ForEach(transaction => transaction.Refresh());
    
    public void AddTransaction(FGEntry entry, int lineNumber, bool undoable)
    {
        var transaction = Instantiate(transactionPrefab, transactionsParent);
        transaction.Initialize(lineNumber, entry, OnValueChanged);
        transactions.Add(transaction);
        
        transaction.OnRemove += (entry, undoable) =>
        {
            manager.Database.Entries.Remove(entry);
            OnValueChanged();
            
            for (int i = transactions.IndexOf(transaction); i < transactions.Count; i++)
                transactions[i].ModifyLineNumber(-1);

            transactions.Remove(transaction);
            Destroy(transaction.gameObject); // TODO: potential undo bug
            
            if (undoable) FGUndoController.Instance.SaveUndo(() =>
            {
                manager.Database.Entries.Add(entry);
                AddTransaction(entry, manager.Database.Entries.Count, false);
                
                OnValueChanged();
                
                manager.SetTransactions();
            });
        };

        transaction.OnSubmitPressed += index =>
        {
            int transactionIndex = transactions.IndexOf(transaction) + 1;

            while (transactionIndex < transactions.Count && transactions[transactionIndex].Entry.Ignore)
                transactionIndex++;
            
            if (transactionIndex >= transactions.Count) transactionIndex = 0;
            
            transactions[transactionIndex].Select(index);
        };

        if (undoable) FGUndoController.Instance.SaveUndo(() =>
        {
            transaction.OnRemove?.Invoke(entry, false);
                
            manager.SetTransactions();
        });
    }

    public void SetAddEntriesAmount(string amount) =>
        addEntriesAmount = !string.IsNullOrEmpty(amount) && int.Parse(amount) > 0 ? int.Parse(amount) : 1;

    public void AddEntries()
    {
        for (int i = 0; i < addEntriesAmount; i++)
        {
            var entry = new FGEntry(new DateTime(DateTime.Today.Year, currentMonth, DateTime.Today.Day));
            manager.Database.Entries.Add(entry);
            AddTransaction(entry, manager.Database.Entries.Count(entry => entry.Date.Month == currentMonth), true);
        }
        
        OnValueChanged();
    }

    public void OnValueChanged()
    {
        Debug.Log("Transactions changed (balance sheet will reload when next shown)");
        
        manager.TransactionsChanged = true;
        
        manager.Save();
    }

    #region Scrolling
    
    public IEnumerator PauseScroll()
    {
        saveScroll = false;
        yield return new WaitForFixedUpdate();
        saveScroll = true;
        SetScroll();
    }

    public void OnScroll(float amount)
    {
        if (!saveScroll) return;
        
        PlayerPrefs.SetFloat(SCROLL_AMOUNT, FGUtils.RoundTo(amount, 2));
    }

    public void SetScroll()
    {
        scrollbar.value = PlayerPrefs.GetFloat(SCROLL_AMOUNT);
    }
    
    #endregion

    public void PreviousMonth()
    {
        Refresh(currentMonth - 1);
        OnScroll(0);
        SetScroll();
    }

    public void NextMonth()
    {
        Refresh(currentMonth + 1);
        OnScroll(0);
        SetScroll();
    }

    public void CurrentMonth()
    {
        Refresh(DateTime.Today.Month);
        OnScroll(0);
        SetScroll();
    }
}