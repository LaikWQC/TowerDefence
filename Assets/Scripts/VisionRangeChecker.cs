using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VisionRangeChecker : MonoBehaviour
{
    [SerializeField] CellManager cellManager;
    [SerializeField] int range;

    private Cell _selectedCell;

    void Update()
    {
        SelectCell();    
    }

    private void SelectCell()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var selected = CellUtils.GetCellInMousePosition();
            cellManager.UnselectAll();
            if (selected == _selectedCell) 
                _selectedCell = null;
            else _selectedCell = selected;
            if (_selectedCell == null) return;

            _selectedCell.Select(Color.yellow);
            var visible = cellManager.GetVisibleCells(_selectedCell, range);
            foreach (var cell in visible)
                cell.Select(Color.green);
        }
    }
}
