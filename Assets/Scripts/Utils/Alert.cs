using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Alert : LoadedObject
{
    [SerializeField] Image alertImage;
    [SerializeField] Color alertColor;
    [SerializeField] float alertTime = 0.1f;

    public static Alert Instance { get; private set; }
    
    public override void Init()
    {
        Instance = this;
    }

    public void CantBuildTower()
    {
        StartCoroutine(AlertCoroutine());
    }

    private IEnumerator AlertCoroutine()
    {
        var color = alertImage.color;
        alertImage.color = alertColor;
        yield return new WaitForSeconds(alertTime);
        alertImage.color = color;
    }
}
