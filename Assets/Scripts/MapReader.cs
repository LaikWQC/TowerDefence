using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapReader : MonoBehaviour
{
    [SerializeField] Texture2D mapData;
    [SerializeField] ColoredMapElement[] mapElements;
    [SerializeField] MapElement defaultElement;

    public MapElement[,] ReadData()
    {
        var mapping = mapElements.ToDictionary(x => x.Color, x => x);
        var dataMatrix = new MapElement[mapData.width, mapData.height];
        for(int w = 0; w < mapData.width; w++)
        {
            for (int h = 0; h < mapData.height; h++)
            {
                mapping.TryGetValue(mapData.GetPixel(w, h), out var element);
                dataMatrix[w, mapData.height - 1 - h] = element ?? defaultElement;                 
            }
        }
        return dataMatrix;
    }
}

[System.Serializable]
public class MapElement
{
    [SerializeField] string tag;
    [SerializeField] CellType type;

    public string Tag => tag;
    public CellType Type => type;
}
[System.Serializable]
public class ColoredMapElement : MapElement
{
    [SerializeField] Color color;

    public Color Color => color;
}
public enum CellType
{
    Cell,
    Tower,
    Gate,
    Block,
    Empty
}
