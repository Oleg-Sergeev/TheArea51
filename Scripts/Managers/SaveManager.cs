using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    public static bool deleteGame = false;

    public static void Save(GameData data)
    {
        string generalPath = "";
#if UNITY_ANDROID && !UNITY_EDITOR
        generalPath = Application.persistentDataPath;
#endif
#if UNITY_EDITOR
        generalPath = Application.dataPath;
#endif
        try
        {
            string savePath = generalPath + "/Save.zg";

            if (!deleteGame)
            {
                using (FileStream fileStream = new FileStream(savePath, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    formatter.Serialize(fileStream, data);
                }
            }
            else
            {
                File.Delete(savePath);
                File.Delete(savePath + ".meta");
            }
        }
        catch (System.Exception e)
        {
            string logPath = generalPath + "/Log.txt";

            string lastLogs = "";
            string currentLog = $"*** Error: {e.StackTrace} /// {e.Message} ***";

            MyDebug.LogError(currentLog);

            if (File.Exists(logPath)) lastLogs = File.ReadAllText(logPath);

            File.WriteAllText(logPath, $"{lastLogs}\n{currentLog}");
        }
    }

    public static GameData Load()
    {
        string generalPath = "";
#if UNITY_ANDROID && !UNITY_EDITOR
        generalPath = Application.persistentDataPath;
#endif
#if UNITY_EDITOR
        generalPath = Application.dataPath;
#endif
        try
        {
            string savePath = generalPath + "/Save.zg";

            if (!File.Exists(savePath)) return null;

            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream fileStream = new FileStream(savePath, FileMode.OpenOrCreate))
            {
                GameData data = formatter.Deserialize(fileStream) as GameData;

                return data;
            }
        }
        catch (System.Exception e)
        {
            string logPath = generalPath + "/Log.txt";

            string lastLogs = "";
            MyDebug.LogError($"*** Error: {e.StackTrace} /// {e.Message} ***");
            if (File.Exists(logPath)) lastLogs = File.ReadAllText(logPath);
            File.WriteAllText(logPath, $"{lastLogs}\n***Error: {e.StackTrace} /// {e.Message} ***");

            return null;
        }
    }
}
