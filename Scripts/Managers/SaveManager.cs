using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    public static bool deleteGame = false, isSaving = false, isLoading = false;

    public async static void Save(GameData data)
    {
        try
        {
            isSaving = true;
            string generalPath = "";
#if UNITY_ANDROID && !UNITY_EDITOR
            generalPath = Application.persistentDataPath;
#endif
#if UNITY_EDITOR
            generalPath = Application.dataPath + "/TheArea51/Editor";
#endif

            string savePath = generalPath + "/Save.zg";

            if (!deleteGame)
            {
                using (FileStream fileStream = new FileStream(savePath, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    while (isLoading) { await System.Threading.Tasks.Task.Yield(); }

                    formatter.Serialize(fileStream, data);
                }

                if (Application.isEditor)
                {
                    File.WriteAllText($"{Application.dataPath}/TheArea51/Editor/Debug.json", JsonUtility.ToJson(data, true));
                }
            }
            else
            {
                File.Delete(savePath);
                File.Delete(savePath + ".meta");
            }
            isSaving = false;
        }
        catch (System.InvalidOperationException e) { throw new InvalidSaveOperationException(2, e); }
        catch (System.ArgumentNullException e) { throw new InvalidSaveOperationException(3, e); }
        catch (System.ArgumentException e) { throw new InvalidSaveOperationException(4, e); }
        catch (System.UnauthorizedAccessException e) { throw new InvalidSaveOperationException(5, e); }
        catch (System.NullReferenceException e) { throw new InvalidSaveOperationException(6, e); }
        catch (DirectoryNotFoundException e) { throw new InvalidSaveOperationException(7, e); }
        catch (System.TypeLoadException e) { throw new InvalidSaveOperationException(8, e); }
        catch (System.Exception e) { throw new InvalidSaveOperationException(1, e); }
    }

    public static GameData Load()
    {
        try
        {
            isLoading = true;
            string generalPath = "";
#if UNITY_ANDROID && !UNITY_EDITOR
            generalPath = Application.persistentDataPath;
#endif
#if UNITY_EDITOR
            generalPath = Application.dataPath + "/TheArea51/Editor";
#endif

            string savePath = generalPath + "/Save.zg";

            if (!File.Exists(savePath))
            {
                isLoading = false;
                return null;
            }

            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream fileStream = new FileStream(savePath, FileMode.OpenOrCreate))
            {
                Wait();
                
                GameData data = formatter.Deserialize(fileStream) as GameData;

                isLoading = false;

                return data;
            }
            
            async void Wait()
            {
                while (isSaving) { await System.Threading.Tasks.Task.Yield(); }
            }
        }
        catch (System.InvalidOperationException e) { throw new InvalidSaveOperationException(2, e); }
        catch (System.ArgumentNullException e) { throw new InvalidSaveOperationException(3, e); }
        catch (System.ArgumentException e) { throw new InvalidSaveOperationException(4, e); }
        catch (System.UnauthorizedAccessException e) { throw new InvalidSaveOperationException(5, e); }
        catch (System.NullReferenceException e) { throw new InvalidSaveOperationException(6, e); }
        catch (System.Exception e) { throw new InvalidSaveOperationException(1, e); }
    }
}
