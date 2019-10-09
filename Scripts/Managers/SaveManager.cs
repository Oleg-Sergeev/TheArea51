using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    public static bool deleteGame = false;

    public static void Save(GameData data)
    {
        string path = "";
#if UNITY_ANDROID && !UNITY_EDITOR
    path = Application.persistentDataPath + "/Save.zg";
#endif
#if UNITY_EDITOR
        path = Application.dataPath + "/Save.zg";
#endif
        try
        {
            if (!deleteGame)
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    formatter.Serialize(fileStream, data);
                }
                File.WriteAllText(Application.dataPath + "/DebugSave.json", JsonUtility.ToJson(data));
            }
            else
            {
                File.Delete(path);
                File.Delete(path + ".meta");
            }
        }
        catch (System.Exception e)
        {
            MyDebug.LogError($"*** Error: {e.StackTrace} /// {e.Message} ***");
        }
    }

    public static GameData Load()
    {
        string path = "";
#if UNITY_ANDROID && !UNITY_EDITOR
        path = Application.persistentDataPath + "/Save.zg";
#endif
#if UNITY_EDITOR
        path = Application.dataPath + "/Save.zg";
#endif
        if (!File.Exists(path)) return null;

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate))
            {
                GameData data = formatter.Deserialize(fileStream) as GameData;

                return data;
            }
        }
        catch (System.Exception e)
        {
            MyDebug.LogError($"*** Error: {e.StackTrace} /// {e.Message} ***");
            return null;
        }
    }
}
