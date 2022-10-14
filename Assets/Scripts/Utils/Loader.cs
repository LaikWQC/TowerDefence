using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    [SerializeField] LoadedObject[] objects;
    private void Awake()
    {
        foreach (var obj in objects)
            obj.Init();
    }
}
public abstract class LoadedObject : MonoBehaviour
{
    public abstract void Init();
}
