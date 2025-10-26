using UnityEngine;
using System.Collections.Generic;

public static class SaveSystem 
{
    private const string TotalCurrencyKey = "TotalCurrency";
    private const string RunCurrencyKey = "RunCurrency";
    private const string LastRunKey = "LastRunEarnings";
    private const string WaveKey = "Wave";
    private const string UpgradePrefix = "Upgrade_";

    public static void SaveGame (int totalCurrency, int runCurrency, int lastRunEarnings, int wave)
    {
        PlayerPrefs.SetInt(TotalCurrencyKey, totalCurrency);
        PlayerPrefs.SetInt(RunCurrencyKey, runCurrency);
        PlayerPrefs.SetInt(LastRunKey, lastRunEarnings);
        PlayerPrefs.SetInt(WaveKey, wave);

        if (UpgradeManager.Instance != null)
        {
            foreach (var up in UpgradeManager.Instance.upgrades)
            {
                if (!string.IsNullOrEmpty(up.id))
                {
                    PlayerPrefs.SetInt(UpgradePrefix + up.id, up.level);
                }
            }
        }
        PlayerPrefs.Save();
        Debug.Log("Game Saved!");
    }

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(TotalCurrencyKey) && PlayerPrefs.HasKey(WaveKey);
    }

    public static void LoadGame(out int totalCurrency, out int runCurrency, out int lastRunEarnings, out int wave)
    {
        totalCurrency = PlayerPrefs.GetInt(TotalCurrencyKey, 0);
        runCurrency = PlayerPrefs.GetInt(RunCurrencyKey, 0);
        lastRunEarnings = PlayerPrefs.GetInt(LastRunKey, 0);
        wave = PlayerPrefs.GetInt(WaveKey, 1);

        if (UpgradeManager.Instance != null)
        {
            foreach (var up in UpgradeManager.Instance.upgrades)
            {
                if (!string.IsNullOrEmpty(up.id))
                {
                    up.level = PlayerPrefs.GetInt(UpgradePrefix + up.id, 0);
                }
            }

            UpgradeManager.Instance.ApplyAllUpgradeEffects();
            UpgradeManager.Instance.UpdateAllButtons();
        }
    }

    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(TotalCurrencyKey);
        PlayerPrefs.DeleteKey(RunCurrencyKey);
        PlayerPrefs.DeleteKey(LastRunKey);
        PlayerPrefs.DeleteKey(WaveKey);

        if (UpgradeManager.Instance != null)
        {
            foreach (var up in UpgradeManager.Instance.upgrades)
            {
                if (!string.IsNullOrEmpty(up.id))
                {
                    PlayerPrefs.DeleteKey(UpgradePrefix + up.id);
                }
            }
        }
        Debug.Log("Save Cleared.");
    }
}
//Need to add upgrades here