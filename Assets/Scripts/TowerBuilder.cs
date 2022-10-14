using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBuilder : MonoBehaviour
{
    [SerializeField] EnemySpawner enemySpawner;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var selected = CellUtils.GetCellInMousePosition();
            if (selected == null) return;
            var handler = selected.TryCreateTower();
            if (handler == null) return;

            var args = new OccupiedCellArgs();
            enemySpawner.TryOccupiedCell(selected, args);
            if (args.CanOccupied)
            {
                handler.Accept();
                foreach (var a in args.DoOnOccupied) a();
            }
            else
            {
                handler.Cancel();
                Alert.Instance.CantBuildTower();
            }
        }
    }
}
