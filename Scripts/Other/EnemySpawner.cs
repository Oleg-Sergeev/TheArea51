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
            PoolManager.Instance.Spawn(PoolType.Rectangle);

            GameDataManager.data.timeToWinLeft -= (int)GameDataManager.data.enemySpawnStep;

            await Task.Delay((int)GameDataManager.data.enemySpawnStep * 1000);
        }
        isSpawning = false;
    }
}
