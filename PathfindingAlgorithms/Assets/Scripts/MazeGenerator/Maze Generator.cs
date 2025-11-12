using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    [SerializeField] private MazeCell _mazeCellPrefab;  //prefab for each individual maze cell
    [SerializeField] private int _mazeWidth;            //number of cells along the X-axis
    [SerializeField] private int _mazeDepth;            //number of cells along the Z-axis
    [SerializeField] private Transform mazeParent;      //to group all maze cells together in heirarchy

    private MazeCell[,] _mazeGrid;                      //array to store all maze cells

    void Start()
    {
        //create a new empty parent object if none was assigned in the inspector
        if (mazeParent == null)
        {
            GameObject parentObj = new GameObject("MazeCells");
            parentObj.transform.SetParent(transform);
            parentObj.transform.localPosition = Vector3.zero;
            mazeParent = parentObj.transform;
        }

        //initialize the grid array based on the specified maze dimensions
        _mazeGrid = new MazeCell[_mazeWidth, _mazeDepth];

        //instantiate all maze cells in a grid layout
        for (int x = 0; x < _mazeWidth; x++)
        {
            for (int z = 0; z < _mazeDepth; z++)
            {
                //create a new cell at position (x, z)
                MazeCell newCell = Instantiate(_mazeCellPrefab, new Vector3(x, 0, z), Quaternion.identity);

                //parent the new cell under the mazeParent for easy cleanup and organization
                newCell.transform.SetParent(mazeParent, true);

                //store the cell reference in the grid
                _mazeGrid[x, z] = newCell;
            }
        }

        //start generating the maze recursively from the top-left cell (0, 0)
        GenerateMaze(null, _mazeGrid[0, 0]);

        //now that the maze is fully built, generate nodes once
        GetComponent<NodeGenerator>()?.GenerateNodes();
    }

    //recursively generates a maze using depth-first search
    private void GenerateMaze(MazeCell previousCell, MazeCell currentCell)
    {
        currentCell.Visit();                      //mark current cell as visited
        ClearWalls(previousCell, currentCell);    //remove walls between current and previous cell

        MazeCell nextCell;

        do
        {
            //try to get a random unvisited neighbor of the current cell
            nextCell = GetNextUnvisitedCell(currentCell);

            if (nextCell != null)
            {
                //continue the maze generation recursively
                GenerateMaze(currentCell, nextCell);
            }
        } while (nextCell != null); //stop when there are no unvisited neighbors left
    }

    //returns a random unvisited neighbor of the current cell
    private MazeCell GetNextUnvisitedCell(MazeCell currentCell)
    {
        //collect all possible unvisited neighboring cells
        var unvisitedCells = GetUnvisitedCells(currentCell);

        //randomize the order and pick the first one
        return unvisitedCells.OrderBy(_ => Random.Range(1, 10)).FirstOrDefault();
    }

    //returns all unvisited neighboring cells (up, down, left, right) of the current cell
    private IEnumerable<MazeCell> GetUnvisitedCells(MazeCell currentCell)
    {
        int x = (int)currentCell.transform.position.x;
        int z = (int)currentCell.transform.position.z;

        //check cell to the right (x + 1)
        if (x + 1 < _mazeWidth)
        {
            var cellToRight = _mazeGrid[x + 1, z];
            if (!cellToRight.IsVisited)
                yield return cellToRight;
        }

        //check cell to the left (x - 1)
        if (x - 1 >= 0)
        {
            var cellToLeft = _mazeGrid[x - 1, z];
            if (!cellToLeft.IsVisited)
                yield return cellToLeft;
        }

        //check cell to the front (z + 1)
        if (z + 1 < _mazeDepth)
        {
            var cellToFront = _mazeGrid[x, z + 1];
            if (!cellToFront.IsVisited)
                yield return cellToFront;
        }

        //check cell to the back (z - 1)
        if (z - 1 >= 0)
        {
            var cellToBack = _mazeGrid[x, z - 1];
            if (!cellToBack.IsVisited)
                yield return cellToBack;
        }
    }

    //removes the walls between two adjacent cells so that a passage is created
    private void ClearWalls(MazeCell previousCell, MazeCell currentCell)
    {
        if (previousCell == null)
            return; //no walls to remove for the first cell

        //compare positions to determine movement direction and remove appropriate walls
        //moving right
        if (previousCell.transform.position.x < currentCell.transform.position.x)
        {
            previousCell.ClearRightWall();
            currentCell.ClearLeftWall();
            return;
        }

        //moving left
        if (previousCell.transform.position.x > currentCell.transform.position.x)
        {
            previousCell.ClearLeftWall();
            currentCell.ClearRightWall();
            return;
        }

        //moving forward
        if (previousCell.transform.position.z < currentCell.transform.position.z)
        {
            previousCell.ClearFrontWall();
            currentCell.ClearBackWall();
            return;
        }

        //moving backward
        if (previousCell.transform.position.z > currentCell.transform.position.z)
        {
            previousCell.ClearBackWall();
            currentCell.ClearFrontWall();
            return;
        }
    }
}