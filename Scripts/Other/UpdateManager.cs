using System.Collections.Generic;
using UnityEngine;

public class UpdateManager : MonoBehaviour
{
    public static UpdateManager Instance;
    public static List<IFixedUpdateableObj> objToFixedUpdate;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        objToFixedUpdate = new List<IFixedUpdateableObj>();
    }
}

public interface IUpdateableObj
{
    void Update_();
}
public interface IFixedUpdateableObj
{
    void FixedUpdate_();
}
