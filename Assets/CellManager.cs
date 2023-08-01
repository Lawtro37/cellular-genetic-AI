using UnityEngine;
using System.Collections.Generic;

public class CellManager : MonoBehaviour
{
    // Singleton pattern
    public static CellManager instance;
    
    // List to store all cells
    public List<Cell> cells;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (cells.Count > 1500)
        {
            int cellsToRemove = cells.Count - 1500;

            for (int i = 0; i < cellsToRemove; i++)
            {
                cells[1500].Die();
                //cells.RemoveAt(1500);
            }
        }
    }


    private void Start()
    {
        // Initialize the list of cells
        cells = new List<Cell>();
    }

    public void RegisterCell(Cell cell)
    {
        // Add the cell to the list
        cells.Add(cell);
    }

    public void UnregisterCell(Cell cell)
    {
        // Remove the cell from the list
        cells.Remove(cell);
    }

    // Other methods to manage cells can be added here
}
