using UnityEngine;

public static class SaveSystem 
{
    private const string TotalCurrencyKey = "TotalCurrency";
    private const string RunCurrencyKey = "RunCurrency";
    private const string LastRunKey = "LastRunEarnings";
    private const string WaveKey = "Wave";

    public static void SaveGame (int totalCurrency, int runCurrency, int lastRunEarnings, int wave)
    {
        PlayerPrefs.SetInt(TotalCurrencyKey, totalCurrency);
        PlayerPrefs.SetInt(RunCurrencyKey, runCurrency);
        PlayerPrefs.SetInt(LastRunKey, lastRunEarnings);
        PlayerPrefs.SetInt(WaveKey, wave);
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
    }

    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(TotalCurrencyKey);
        PlayerPrefs.DeleteKey(RunCurrencyKey);
        PlayerPrefs.DeleteKey(LastRunKey);
        PlayerPrefs.DeleteKey(WaveKey);
        Debug.Log("Save Cleared.");
    }
}
//Need to add upgrades here