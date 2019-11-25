using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public static class MovingObjList
{
    private static Dictionary<string, MovingObj> movingObjs = new Dictionary<string, MovingObj>();


    public static MovingObj GetObj(string name)
    {
        if (!movingObjs.ContainsKey(name)) return null;
        return movingObjs[name];
    }

    public static void AddObj(MovingObj obj)
    {
        if (movingObjs == null) movingObjs = new Dictionary<string, MovingObj>();

        if (movingObjs.ContainsKey(obj.name))
        {
            MyDebug.LogWarning($"Object with key {obj.name} already exists in list");
            return;
        }
        movingObjs.Add(obj.name, obj);
    }

    public static void Clear() => movingObjs.Clear();
}

public class MovingObj : MonoBehaviour
{
    public Vector2 defaultTarget, minimalDistance = new Vector2(1,1);
    [HideInInspector] public Vector2 startPos;
    public float defaultSpeed = 1;
    public bool hideOnAwake = true;
    private bool isRunning;


    private async void Awake()
    {
        MovingObjList.AddObj(this);
        startPos = transform.localPosition;

        MovingObj obj = this;

        await Task.Delay(1);

        if (obj == null) return;

        obj?.gameObject?.SetActive(!hideOnAwake);
    }

    public void MoveToTarget(Vector2 newTarget = default, float newSpeed = default, Action<GameObject> onEnd = default)
    {
        Move(newTarget, newSpeed, onEnd);
    }

    public void MoveToStartPos(float newSpeed = default, Action<GameObject> onEnd = default)
    {
        Move(startPos, newSpeed, onEnd);
    }

    private async void Move(Vector2 target, float newSpeed, Action<GameObject> onEnd)
    {
        isRunning = false;
        await Task.Yield();
        isRunning = true;
        
        Vector2 currentTarget = target == default ? defaultTarget : target;
        float currentSpeed = newSpeed == default ? defaultSpeed : newSpeed;
        float speed = currentSpeed / 5;
        while ((Vector2)transform.localPosition != currentTarget && isRunning)
        {
            transform.localPosition = Vector2.Lerp(transform.localPosition, currentTarget, speed * Time.unscaledDeltaTime);

            if (Mathf.Abs(((Vector2)transform.localPosition - currentTarget).x) <= Mathf.Abs(minimalDistance.x)
                && Mathf.Abs(((Vector2)transform.localPosition - currentTarget).y) <= Mathf.Abs(minimalDistance.y))
                break;

            if (speed < currentSpeed * 2)
            {
                speed += Time.unscaledDeltaTime * currentSpeed * 2;
            }

            await Task.Yield();
        }

        if (isRunning) onEnd?.Invoke(gameObject);
    }
}
