using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    private static Dictionary<string, AudioSource> sfx;
    private static new bool enabled;
    public Transform sfxArr;

    private void Awake()
    {
        sfx = new Dictionary<string, AudioSource>();

        for (int i = 0; i < sfxArr.childCount; i++)
        {
            sfx.Add(sfxArr.GetChild(i).name, sfxArr.GetChild(i).GetComponent<AudioSource>());
        }
    }

    public static void PlaySound(string name)
    {
        if (!sfx.ContainsKey(name))
        {
            MyDebug.LogError($"Sound {name} not found");
            return;
        }
        if (!enabled) return;
        
        sfx[name].Play();
    }

    public static void EnableSound(bool enable)
    {
        enabled = enable;

        GameDataManager.data.enableSFX = enable;
    }
}
