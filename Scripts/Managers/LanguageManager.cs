using System.Collections.Generic;
using UnityEngine;

public static class LanguageManager
{
    private static Dictionary<string, string> localizedText;

    public static void ChangeLanguage(string lng)
    {
        const string DEFAULT_LANGUAGE = "ru";

        UI.Instance.debugLng.text = lng;

        if (Resources.Load<Object>(lng) == null)
        {
            UI.Instance.debugLng.text = $"{lng}/";
            lng = DEFAULT_LANGUAGE;
            UI.Instance.debugLng.text += lng;
        }

        GameDataManager.data.language = lng;

        localizedText = new Dictionary<string, string>();

        string json = Resources.Load<TextAsset>(lng).text;

        LocalizationData localizationData = JsonUtility.FromJson<LocalizationData>(json);

        foreach (var item in localizationData.items) localizedText.Add(item.key, item.value);
    }

    public static string GetLocalizedText(string key)
    {
        if (localizedText == null || !localizedText.ContainsKey(key)) return null;

        return localizedText[key];
    }
}

public class LocalizationData
{
    public LocalizationItem[] items;
}

[System.Serializable]
public class LocalizationItem
{
    public string key, value;
}
