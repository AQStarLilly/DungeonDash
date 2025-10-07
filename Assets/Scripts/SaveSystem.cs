using UnityEngine;

public static class SaveSystem 
{
    private const string TotalCurrencyKey = "TotalCurrency";
    private const string WaveKey = "Wave";

    public static void SaveGame (int totalCurrency, int wave)
    {
        PlayerPrefs.SetInt(TotalCurrencyKey, totalCurrency);
        PlayerPrefs.SetInt(WaveKey, wave);
        PlayerPrefs.Save();
        Debug.Log("Game Saved!");
    }

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(TotalCurrencyKey) && PlayerPrefs.HasKey(WaveKey);
    }

    public static void LoadGame(out int totalCurrency, out int wave)
    {
        totalCurrency = PlayerPrefs.GetInt(TotalCurrencyKey, 0);
        wave = PlayerPrefs.GetInt(WaveKey, 1);
    }

    public static void ClearSave()
    {
        PlayerPrefs.DeleteKey(TotalCurrencyKey);
        PlayerPrefs.DeleteKey(WaveKey);
        Debug.Log("Save Cleared.");
    }
}
