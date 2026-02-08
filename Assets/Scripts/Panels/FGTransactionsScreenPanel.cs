using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NativeFileBrowser;
using UnityEngine;
using UnityEngine.UI;

public class FGTransactionsScreenPanel : MonoBehaviour
{
    [SerializeField] Transform transactionsParent;
    [SerializeField] FGTransactionController transactionPrefab;
    [SerializeField] Scrollbar scrollbar;
    
    List<FGTransactionController> transactions = new();
    int addEntriesAmount = 1;
    
    FGManager manager;

    const string SCROLL_AMOUNT = "scrollAmount";
    bool saveScroll = true;

    public void Initialize()
    {
        manager = FGManager.Instance;
    }
    
    public void InstantiateTransactions()
    {
        for (int i = 0; i < manager.Database.Entries.Count; i++)
            AddTransaction(manager.Database.Entries[i], i + 1, false);
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
            var entry = new FGEntry(DateTime.Today);
            manager.Database.Entries.Add(entry);
            AddTransaction(entry, manager.Database.Entries.Count, true);
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
}