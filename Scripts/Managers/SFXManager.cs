using System.Collections.Generic;
using UnityEngine;

public class SFXManager : MonoBehaviour
{
    private static Dictionary<SoundTypes, AudioSource> sfx;
    private static new bool enabled;
    public Transform sfxArr;

    private void Awake()
    {
        sfx = new Dictionary<SoundTypes, AudioSource>();

        for (int i = 0; i < sfxArr.childCount; i++)
        {
            sfx.Add((SoundTypes)System.Enum.Parse(typeof(SoundTypes), sfxArr.GetChild(i).name), sfxArr.GetChild(i).GetComponent<AudioSource>());
        }
    }

    public static void PlaySound(SoundTypes sound)
    {
        if (!sfx.ContainsKey(sound))
        {
            MyDebug.LogError($"Sound {sound} not found");
            return;
        }
        if (!enabled) return;
        
        sfx[sound].Play();
    }

    public static void EnableSound(bool enable)
    {
        enabled = enable;

        GameDataManager.data.enableSFX = enable;
    }
}

[System.Serializable] public enum SoundTypes
{
    Click,
    Panel,
    Button,
    Buy
}
