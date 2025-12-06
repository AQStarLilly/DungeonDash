using UnityEngine;
using System.Collections.Generic;

public static class SaveSystem 
{
    // --- Centralized Key Management ---
    private const string Prefix = "MyGame_";  // Prevents PlayerPrefs collisions

    private const string TotalCurrencyKey = Prefix + "TotalCurrency";
    private const string RunCurrencyKey = Prefix + "RunCurrency";
    private const string LastRunKey = Prefix + "LastRunEarnings";
    private const string WaveKey = Prefix + "Wave";
    private const string UpgradePrefix = Prefix + "Upgrade_";

    // --- SAVE ---
    public static void SaveGame (int totalCurrency, int runCurrency, int lastRunEarnings, int wave)
    {
        PlayerPrefs.SetInt(TotalCurrencyKey, totalCurrency);
        PlayerPrefs.SetInt(RunCurrencyKey, runCurrency);
        PlayerPrefs.SetInt(LastRunKey, lastRunEarnings);
        PlayerPrefs.SetInt(WaveKey, wave);

        // Save upgrades if UpgradeManager exists
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.upgrades != null)
        {
            foreach (var up in UpgradeManager.Instance.upgrades)
            {
                if (string.IsNullOrEmpty(up.id))
                {
                    Debug.LogWarning("[SaveSystem] Upgrade found with missing ID. Skipping.");
                    continue;
                }

                PlayerPrefs.SetInt(UpgradePrefix + up.id, up.level);
            }
        }
        else
        {
            Debug.LogWarning("[SaveSystem] UpgradeManager missing — skipping upgrade save.");
        }

        PlayerPrefs.Save();
        Debug.Log("Game Saved!");
    }

    // --- Check SAVE ---
    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(TotalCurrencyKey) && PlayerPrefs.HasKey(WaveKey);
    }

    // --- Load ---
    public static void LoadGame(out int totalCurrency, out int runCurrency, out int lastRunEarnings, out int wave)
    {
        totalCurrency = PlayerPrefs.GetInt(TotalCurrencyKey, 0);
        runCurrency = PlayerPrefs.GetInt(RunCurrencyKey, 0);
        lastRunEarnings = PlayerPrefs.GetInt(LastRunKey, 0);
        wave = PlayerPrefs.GetInt(WaveKey, 1);

        // Load upgrades
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.upgrades != null)
        {
            foreach (var up in UpgradeManager.Instance.upgrades)
            {
                if (string.IsNullOrEmpty(up.id))
                {
                    Debug.LogWarning("[SaveSystem] Upgrade found with missing ID. Skipping load.");
                    continue;
                }

                up.level = PlayerPrefs.GetInt(UpgradePrefix + up.id, 0);
            }

            // Apply effects AFTER loading
            UpgradeManager.Instance.ApplyAllUpgradeEffects();
            UpgradeManager.Instance.UpdateAllButtons();
        }
        else
        {
            Debug.LogWarning("[SaveSystem] UpgradeManager missing — upgrade load skipped.");
        }
    }

    // --- CLEAR ---
    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(TotalCurrencyKey);
        PlayerPrefs.DeleteKey(RunCurrencyKey);
        PlayerPrefs.DeleteKey(LastRunKey);
        PlayerPrefs.DeleteKey(WaveKey);

        // Delete upgrade levels
        if (UpgradeManager.Instance != null && UpgradeManager.Instance.upgrades != null)
        {
            foreach (var up in UpgradeManager.Instance.upgrades)
            {
                if (!string.IsNullOrEmpty(up.id))
                {
                    PlayerPrefs.DeleteKey(UpgradePrefix + up.id);
                }
            }
        }
        else
        {
            Debug.LogWarning("[SaveSystem] UpgradeManager missing — skipping upgrade deletion.");
        }

        // Also clear the UI tutorial popup flag
        PlayerPrefs.DeleteKey("HasShownAbilityPopup");

        Debug.Log("Save Cleared.");
    }
}
