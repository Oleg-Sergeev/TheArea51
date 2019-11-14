using UnityEngine;

public class AnimationEvent : MonoBehaviour
{
    public void OnAnimationEnd()
    {
        EventManager.eventManager.EndAnimation(gameObject);
    }
}
