using UnityEngine;

public static class CellUtils
{
    public static EmptyCell GetCellInMousePosition()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit)) return null;
        return hit.transform.GetComponent<EmptyCell>();
    }
}
