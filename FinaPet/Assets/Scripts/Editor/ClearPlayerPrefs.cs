using UnityEditor;
using UnityEngine;

public class ClearPlayerPrefs : Editor
{
    // This adds a new menu item under "Tools > Expense App > Clear Daily Submission Key"
    [MenuItem("Tools/Expense App/Clear Daily Submission Key")]
    public static void ClearExpenseKey()
    {
        string key = "LastExpenseSubmissionDate"; // The exact key from ExpenseManager.cs

        if (PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            Debug.Log($"SUCCESS: PlayerPrefs key '{key}' has been deleted.");
        }
        else
        {
            Debug.LogWarning($"NOTE: PlayerPrefs key '{key}' was not found. Nothing to delete.");
        }
    }
}