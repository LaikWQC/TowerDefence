using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CellManager : LoadedObject
{
    [SerializeField] MapReader mapReader;
    [SerializeField] float widthSpacing = 0.1f;
    [SerializeField] float heightSpacing = 0.1f;
    [SerializeField] EmptyCell cellPf;
    [SerializeField] BlockCell blockPf;
    [SerializeField] NoCell noCellPf;
    [SerializeField] Transform CellFolder;
    [SerializeField] Transform GateFolder;
    [SerializeField] Transform ObstacleFolder;

    private SquareCellData[,] _cellsMatrix; 
    private Dictionary<Cell, SquareCellData> _cellsData;

    public List<Gate> Gates { get; } = new List<Gate>();

    public override void Init()
    {
        CreateCells();
    }

    #region Load data
    private void CreateCells()
    {
        var elements = mapReader.ReadData();
        var xLength = elements.GetLength(0);
        var zLenght = elements.GetLength(1);

        var xSize = 1f + widthSpacing;
        var zSize = 1f + heightSpacing;
        var xOffset = -xLength * xSize / 2f;
        var zOffset = zLenght * zSize / 2f;

        _cellsMatrix = new SquareCellData[xLength, zLenght];
        _cellsData = new Dictionary<Cell, SquareCellData>();

        for (int x = 0; x < xLength; x++)
        {
            for (int z = 0; z < zLenght; z++)
            {
                var element = elements[x, z];
                if (element == null)
                {
                    Debug.Log($"No element at {x},{z}");
                    continue;
                }

                Cell cell = null;
                switch(element.Type)
                {
                    case CellType.Empty:
                        cell = Instantiate(noCellPf, ObstacleFolder);
                        break;
                    case CellType.Block:
                        cell = Instantiate(blockPf, ObstacleFolder);
                        break;
                    case CellType.Cell:
                        cell = Instantiate(cellPf, CellFolder);
                        break;
                    case CellType.Tower:
                        var tower = Instantiate(cellPf, CellFolder);
                        tower.CreateTower();
                        cell = tower;
                        break;
                    case CellType.Gate:
                        var gate = Instantiate(cellPf, GateFolder);
                        gate.CreateGate();
                        Gates.Add(gate.Gate);
                        cell = gate;
                        break;
                }

                cell.name = $"{element.Tag} ({x},{z})";
                cell.transform.position = new Vector3(xOffset + x * zSize, 0, zOffset - z * zSize);

                var data = _cellsData[cell] = new SquareCellData(cell) { X = x, Y = z };
                _cellsMatrix[x, z] = data;
            }
        }

        FindNeighbors();
    }

    private void FindNeighbors()
    {
        foreach (var cell in _cellsData.Values)
        {
            var neighbors = new List<SquareCellData>();
            if (TryFindNeighbor(cell.X, cell.Y + 1, out var neighbor)) 
                neighbors.Add(neighbor);
            if (TryFindNeighbor(cell.X, cell.Y - 1, out neighbor)) 
                neighbors.Add(neighbor);
            if (TryFindNeighbor(cell.X + 1, cell.Y, out neighbor)) 
                neighbors.Add(neighbor);
            if (TryFindNeighbor(cell.X - 1, cell.Y, out neighbor)) 
                neighbors.Add(neighbor);
            cell.Neighbors = neighbors.ToArray();
        }
    }
    private bool TryFindNeighbor(int x, int y, out SquareCellData neighbor)
    {
        neighbor = null;
        if (x < 0 || x >= _cellsMatrix.GetLength(0) || y < 0 || y >= _cellsMatrix.GetLength(1)) 
            return false;
        neighbor = _cellsMatrix[x, y];
        return neighbor != null;
    }
    #endregion

    public bool TryCreatePath(Cell begin, Cell end, out List<Cell> path)
    {
        path = new List<Cell>();
        if (!_cellsData[begin].TryCreatePath(_cellsData[end], out var result))
            return false;
        path.AddRange(result.Select(x => x.Cell));
        return true;
    }
    public void UnselectAll()
    {
        foreach (var cell in _cellsData.Keys)
            cell.UnSelect();
    }

    public ICollection<Cell> GetVisibleCells(Cell cell, int range)
    {
        return _cellsData[cell].GetVisibleCells(range, _cellsMatrix).Select(x=>x.Cell).ToList();
    }
    private class SquareCellData
    {
        public SquareCellData(Cell cell)
        {
            Cell = cell;
        }
        public Cell Cell { get; }
        public SquareCellData[] Neighbors { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        private IEnumerable<SquareCellData> DrawLine(SquareCellData to, SquareCellData[,] cells, Func<SquareCellData, bool> isCorrect)
        {
            SquareCellData previousCell = this;
            float N = GetLenght(to);
            for (int i = 1; i <= N; i++) 
            {
                var t = i / N;
                var x = Mathf.Lerp(X, to.X, t);
                var y = Mathf.Lerp(Y, to.Y, t);
                SquareCellData cell = null;
                if (IsExatclyHalf(x) && IsExatclyHalf(y)) //точка находится на пересечении 4х ячеек, 2 из которых будут лежать на линии (либо /, либо \)
                {
                    if (previousCell.X > x && previousCell.Y > y) //прямая /
                    {
                        var xCell = cells[Mathf.FloorToInt(x), Mathf.CeilToInt(y)];
                        var yCell = cells[Mathf.CeilToInt(x), Mathf.FloorToInt(y)];
                        if (isCorrect(xCell) && !isCorrect(yCell))
                            cell = xCell;
                        else cell = yCell;
                    }
                    else //прямая \
                    {
                        var floorCell = cells[Mathf.FloorToInt(x), Mathf.FloorToInt(y)];
                        var ceilCell = cells[Mathf.CeilToInt(x), Mathf.CeilToInt(y)];
                        if (isCorrect(ceilCell) && !isCorrect(floorCell))
                            cell = ceilCell;
                        else cell = floorCell;
                    }
                }
                else
                {
                    cell = cells[Mathf.RoundToInt(x), Mathf.RoundToInt(y)];
                }  
                previousCell = cell;
                yield return cell;
            }
        }
        private bool IsExatclyHalf(float value) => value % 1 == 0.5f;
        private int GetLenght(SquareCellData to) 
        {
            return Mathf.Abs(X - to.X) + Mathf.Abs(Y - to.Y);
        }
        private HashSet<SquareCellData> GetCellsInRange(int range)
        {
            var cells = new HashSet<SquareCellData>() { this };
            var oldCells = new HashSet<SquareCellData>() { this };
            while (range > 0) 
            {
                var newCells = new HashSet<SquareCellData>();
                foreach (var oldCell in oldCells)
                {
                    foreach(var cell in oldCell.Neighbors)
                    {
                        if (cells.Add(cell))
                            newCells.Add(cell);
                    }
                }
                oldCells = newCells;
                range--;
            }
            cells.Remove(this);
            return cells;
        }

        public bool TryCreatePath(SquareCellData to, out List<SquareCellData> result)
        {
            result = new List<SquareCellData>();
            var cells = new HashSet<SquareCellData>() { this };
            var oldCells = new HashSet<SquareCellData>() { this };
            var finded = new List<HashSet<SquareCellData>>();
            int range = 0;
            var isFinded = false;
            while (true) 
            {
                var findedThisRange = new HashSet<SquareCellData>();
                finded.Add(findedThisRange);
                foreach (var oldCell in oldCells)
                {
                    foreach (var cell in oldCell.Neighbors)
                    {
                        if (!cell.Cell.CanWalk) continue;
                        if (cell == to) isFinded = true;
                        if (cells.Add(cell))
                            findedThisRange.Add(cell);
                    }
                }
                if (isFinded) break;
                range++;
                oldCells = findedThisRange;
                if (oldCells.Count == 0)
                    return false;
            }

            var resultArray = new SquareCellData[range + 1];
            resultArray[range] = to;
            var previousCell = to;
            for (int i = range - 1; i >= 0; i--)
            {
                var findedThisRange = finded[i];
                var cell = previousCell.Neighbors.First(x => findedThisRange.Contains(x));
                resultArray[i] = previousCell = cell;                
            }
            result.AddRange(resultArray);
            return true;
        }

        public IEnumerable<SquareCellData> GetVisibleCells(int range, SquareCellData[,] cells)
        {
            foreach (var cellInRange in GetCellsInRange(range))
            {
                var line = DrawLine(cellInRange, cells, c => c.Cell.CanShoot);
                if (line.All(x => x.Cell.CanShoot))
                    yield return cellInRange;
            }
        }
    }
}
