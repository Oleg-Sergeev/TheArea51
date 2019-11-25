using System.Threading.Tasks;

public class EnemySpawner
{
    private static bool isSpawning = false;


    public static async void SpawnEnemy()
    {
        if (isSpawning) return;
        isSpawning = true;

        while (GameDataManager.data.timeToWinLeft >= 0 && GameDataManager.data.isDefend)
        {
            PoolManager.Instance.Spawn(PoolType.Circle);

            GameDataManager.data.timeToWinLeft -= GameDataManager.data.enemySpawnStep * UnityEngine.Time.timeScale;        

            await Task.Delay((int)(GameDataManager.data.enemySpawnStep * 1000));
        }
        EventManager.eventManager.EndAttack(GameDataManager.data.soldiersCount > 0);
        isSpawning = false;
    }
}
