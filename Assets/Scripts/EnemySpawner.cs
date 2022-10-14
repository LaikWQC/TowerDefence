using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : LoadedObject
{
    [SerializeField] CellManager cellManager;
    [SerializeField] PathData[] paths;

    public override void Init()
    {
        foreach(var path in paths)
        {
            path.Init(
                cellManager.Gates[path.StartGateIndex].Cell,
                cellManager.Gates[path.EndGateIndex].Cell,
                cellManager);  
        }
    }

    public void TryOccupiedCell(Cell cell, OccupiedCellArgs args)
    {
        foreach (var path in paths)
            path.TryOccupiedCell(cell, args);
    }
}
public class OccupiedCellArgs
{
    public bool CanOccupied { get; set; } = true;
    public List<Action> DoOnOccupied { get; } = new List<Action>();
}

[System.Serializable]
public class PathData
{
    [SerializeField] int startGateIndex;
    [SerializeField] int endGateIndex;
    [SerializeField] Color highlitedColor;

    private CellManager _cellManager;

    public void Init(Cell start, Cell end, CellManager cellManager)
    {
        Start = start;
        End = end;
        _cellManager = cellManager;

        CalculatePath();
    }

    private void CalculatePath()
    {
        UnHighlight();
        Cells.Clear();

        if (_cellManager.TryCreatePath(Start, End, out var cells))
            Cells.AddRange(cells);

        Highlight();
    }
    private void CalculatePath(List<Cell> cells)
    {
        UnHighlight();
        Cells.Clear();

        Cells.AddRange(cells);

        Highlight();
    }

    public void TryOccupiedCell(Cell cell, OccupiedCellArgs args)
    {
        if (!Cells.Contains(cell)) return;
        if (!_cellManager.TryCreatePath(Start, End, out var cells))
            args.CanOccupied = false;
        else
            args.DoOnOccupied.Add(() => CalculatePath(cells));
    }

    public int StartGateIndex => startGateIndex;
    public int EndGateIndex => endGateIndex;
    public Color HighlitedColor => highlitedColor;
    public Cell Start { get; private set; }
    public Cell End { get; private set; }
    public List<Cell> Cells { get; } = new List<Cell>();

    public void Highlight()
    {
        foreach (var cell in Cells)
            cell.Select(highlitedColor);
    }
    public void UnHighlight()
    {
        foreach (var cell in Cells)
            cell.UnSelect();
    }
}
