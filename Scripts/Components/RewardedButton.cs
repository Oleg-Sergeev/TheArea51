using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RewardedButton : MonoBehaviour
{
    [SerializeField] private string placementId = "";
    public RewardBonus rewardBonus;
    public Button Button { get; private set; }

    private void Awake()
    {
        Button = GetComponent<Button>();

        RewardAds.RewardedButtons.Add(placementId, this);
    }
}

[System.Serializable]
public struct RewardBonus
{
    public Bonus bonus;
    public int amount;

    public enum Bonus
    {
        HeartsBonus,
        SkipTimer
    }
}
