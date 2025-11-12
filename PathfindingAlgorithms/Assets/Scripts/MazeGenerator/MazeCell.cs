using UnityEngine;

public class MazeCell : MonoBehaviour
{
    //-----Wall References-----
    //each wall is a child GameObject that visually represents one side of the cell
    //these will be toggled on/off as the maze is generated

    [SerializeField]
    private GameObject _leftWall;

    [SerializeField]
    private GameObject _rightWall;

    [SerializeField]
    private GameObject _frontWall;

    [SerializeField]
    private GameObject _backWall;

    //this block covers the cell before it’s visited — often used for debugging or visualizing progress
    [SerializeField]
    private GameObject _unvisitedBlock;

    //tracks whether this cell has already been visited during maze generation
    public bool IsVisited { get; private set; }

    //marks this cell as visited and disables the visual "unvisited" block
    public void Visit()
    {
        IsVisited = true;

        //once the cell is visited, hide the unvisited overlay to show progress
        _unvisitedBlock.SetActive(false);
    }

    //disables the left wall to create a passage
    public void ClearLeftWall()
    {
        _leftWall.SetActive(false);
    }

    //disables the right wall to create a passage
    public void ClearRightWall()
    {
        _rightWall.SetActive(false);
    }

    //disables the front wall to create a passage
    public void ClearFrontWall()
    {
        _frontWall.SetActive(false);
    }

    //disables the back wall to create a passage
    public void ClearBackWall()
    {
        _backWall.SetActive(false);
    }

    public GameObject GetLeftWall() => _leftWall;
    public GameObject GetRightWall() => _rightWall;
    public GameObject GetFrontWall() => _frontWall;
    public GameObject GetBackWall() => _backWall;
}