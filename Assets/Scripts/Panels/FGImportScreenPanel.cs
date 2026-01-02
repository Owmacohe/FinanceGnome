using System.IO;
using NativeFileBrowser;
using UnityEngine;

public class FGImportScreenPanel : MonoBehaviour
{
    FGManager manager;

    public void Initialize()
    {
        manager = FGManager.Instance;
    }
    
    public void Import()
    {
        Debug.Log("Importing...");
        
        var path = FileBrowser.PickFile();
        
        if (string.IsNullOrEmpty(path)) Debug.LogError("Path is null");
        else if (!path.Substring(path.IndexOf('.')).Equals(".csv")) Debug.LogError("File is not of a valid file type");
        else
        {
            var entries = manager.Database.Import(File.ReadAllText(path));

            for (int i = 0; i < entries.Count; i++)
                manager.transactionsScreen.AddTransaction(entries[i], manager.Database.Entries.Count - entries.Count + i + 1);
            
            manager.transactionsScreen.OnValueChanged();
            
            Debug.Log($"Imported from <i>{path}</i>");
        }
        
        manager.SetTransactions();
    }
}