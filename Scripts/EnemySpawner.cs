using System.Threading.Tasks;

public class EnemySpawner
{
    private static bool isSpawning = false;

    public static async void SpawnEnemy()
    {
        if (isSpawning) return;
        isSpawning = true;

        while (GameManager.data.timeToWinLeft >= 0 && GameManager.data.isDefend)
        {
            PoolManager.Instance.Spawn(PoolType.Circle);
            PoolManager.Instance.Spawn(PoolType.Rectangle);

            GameManager.data.timeToWinLeft -= (int)GameManager.data.enemySpawnStep;

            await Task.Delay((int)GameManager.data.enemySpawnStep * 1000);
        }
        isSpawning = false;
    }
}
