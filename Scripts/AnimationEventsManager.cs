using UnityEngine;

public class AnimationEventsManager : MonoBehaviour
{
    public void OnClose(string name)
    {
        UI.GetPanel(name).panel.SetActive(false);
    }
    public void OnAnyClose(string name)
    {
        GameObject.Find(name).SetActive(false);
    }
}
