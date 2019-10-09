using UnityEngine;

public class PooledObject : MonoBehaviour, IPooledObj
{
    private Vector2 direction;
    private PoolType type;
    private float lifeTime;

    public void OnSpawn(PoolType objType)
    {
        lifeTime = 15;
        type = objType;
        GetComponent<UnityEngine.UI.Image>().color = new Color(Random.Range(0.6f, 1f), Random.Range(0.25f, 0.7f), Random.Range(0f, 0.5f));
        Vector3 bias = new Vector3(Random.Range(-300, 300), Random.Range(-300, 300));
        direction = (PoolManager.Instance.target.localPosition + bias) - transform.localPosition;
    }

    private void FixedUpdate()
    {
        transform.Translate(direction.normalized * Time.deltaTime * Random.Range(0.7f + GameDataManager.data.prestigeLvl,1.4f + GameDataManager.data.prestigeLvl));
        lifeTime -= Time.fixedDeltaTime;
        if (lifeTime <= 0) PoolManager.Instance.Hide(gameObject, type);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        EventManager.eventManager.ChangeHp(Random.Range(-100 - (1000 * GameDataManager.data.prestigeLvl),-500 - (1000 * GameDataManager.data.prestigeLvl)));
        
        PoolManager.Instance.Hide(gameObject, type);
    }
}

public interface IPooledObj
{
    void OnSpawn(PoolType poolType);
}
