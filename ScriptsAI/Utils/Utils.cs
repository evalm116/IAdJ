using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Utils
{
    public static List<GridCell> GetEspacioBusqueda(GridCell currentCell, int tamanoEspacio)
    {
        Queue<(GridCell, int)> nodosBusquedas = new Queue<(GridCell, int)>();
        List<GridCell> espacioBusqueda = new List<GridCell>();

        nodosBusquedas.Enqueue((currentCell, 1));

        // Agregar los vecinos de la celda actual al espacio de búsqueda

        while (nodosBusquedas.Any())
        {
            (var nodo, var radio) = nodosBusquedas.Dequeue();
            espacioBusqueda.Add(nodo);
            var neighborCells = GameManager.Instance.GameGrid.GetNeighbors(nodo);
            foreach (GridCell neighbor in neighborCells)
            {
                espacioBusqueda.Add(neighbor);
                if (radio < tamanoEspacio)
                {
                    nodosBusquedas.Enqueue((neighbor, radio + 1));
                }
            }
        }

        return espacioBusqueda;
    }
}
