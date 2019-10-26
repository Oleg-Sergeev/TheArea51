using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class SaveManager
{
    public static bool deleteGame = false;

    public static void Save(GameData data)
    {
        try
        {
        string generalPath = "";
#if UNITY_ANDROID && !UNITY_EDITOR
        generalPath = Application.persistentDataPath;
#endif
#if UNITY_EDITOR
        generalPath = Application.dataPath;
#endif
        
            string savePath = generalPath + "/Save.zg";

            if (!deleteGame)
            {
                using (FileStream fileStream = new FileStream(savePath, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    formatter.Serialize(fileStream, data);
                }

                if (Application.isEditor)
                {
                    string json = JsonConvert.SerializeObject(data);
                    PlayerPrefs.SetString("DEBUG", json);
                }
            }
            else
            {
                File.Delete(savePath);
                File.Delete(savePath + ".meta");
            }
        }
        catch (System.InvalidOperationException e) { throw new InvalidSaveOperationException(2, e); }
        catch (System.ArgumentNullException e) { throw new InvalidSaveOperationException(3, e); }
        catch (System.UnauthorizedAccessException e) { throw new InvalidSaveOperationException(4, e); }
        catch (System.NullReferenceException e) { throw new InvalidSaveOperationException(5, e); }
        catch (System.Exception e) { throw new InvalidSaveOperationException(1, e); }
    }

    public static GameData Load()
    {
        try
        { 
        string generalPath = "";
#if UNITY_ANDROID && !UNITY_EDITOR
        generalPath = Application.persistentDataPath;
#endif
#if UNITY_EDITOR
        generalPath = Application.dataPath;
#endif
        
            string savePath = generalPath + "/Save.zg";

            if (!File.Exists(savePath)) return null;

            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream fileStream = new FileStream(savePath, FileMode.OpenOrCreate))
            {
                GameData data = formatter.Deserialize(fileStream) as GameData;

                return data;
            }
        }
        catch (System.InvalidOperationException e) { throw new InvalidSaveOperationException(2, e); }
        catch (System.ArgumentNullException e) { throw new InvalidSaveOperationException(3, e); }
        catch (System.UnauthorizedAccessException e) { throw new InvalidSaveOperationException(4, e); }
        catch (System.NullReferenceException e) { throw new InvalidSaveOperationException(5, e); }
        catch (System.Exception e) { throw new InvalidSaveOperationException(1, e); }
    }
}
