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
        if (!deleteGame)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            
            using (FileStream fileStream = new FileStream(path, FileMode.Create))
            {
                formatter.Serialize(fileStream, data);
            }
        }
        else
        {
            File.Delete(path);
            File.Delete(path+".meta");
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

        BinaryFormatter formatter = new BinaryFormatter();

        using (FileStream fileStream = new FileStream(path, FileMode.OpenOrCreate))
        {
            GameData data = formatter.Deserialize(fileStream) as GameData;
            return data;
        }
    }
}
