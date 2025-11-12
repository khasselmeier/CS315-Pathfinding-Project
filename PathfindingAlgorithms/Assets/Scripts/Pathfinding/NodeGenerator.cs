using System.Collections.Generic;
using UnityEngine;

public class NodeGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform mazeParent;
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private float nodeYOffset = 0.2f;
    [SerializeField] private float cellSpacing = 1f; //distance between adjacent cells

    private List<Node> generatedNodes = new List<Node>();
    public static Dictionary<GameObject, Node> CellToNodeMap = new Dictionary<GameObject, Node>();

    public void GenerateNodes()
    {
        ClearExistingNodes();

        MazeCell[] cells = mazeParent.GetComponentsInChildren<MazeCell>();
        Debug.Log($"Found {cells.Length} MazeCells under {mazeParent.name}");

        if (cells.Length == 0)
        {
            Debug.LogWarning("No MazeCells found — nodes cannot be generated");
            return;
        }

        Dictionary<MazeCell, Node> cellNodeMap = new Dictionary<MazeCell, Node>();

        //step 1: create a node for each maze cell
        foreach (MazeCell cell in cells)
        {
            Vector3 nodePos = cell.transform.position + Vector3.up * nodeYOffset;
            GameObject nodeObj = Instantiate(nodePrefab, nodePos, Quaternion.identity, transform);

            Node node = nodeObj.GetComponent<Node>();
            if (node == null)
            {
                Debug.LogError($"Node prefab {nodePrefab.name} does not have a Node component");
                Destroy(nodeObj);
                continue;
            }

            cellNodeMap[cell] = node;
            generatedNodes.Add(node);
        }

        //step 2: connect nodes based on open walls
        foreach (MazeCell cell in cells)
        {
            Node node = cellNodeMap[cell];
            List<Node> connections = new List<Node>();

            //find potential neighbors based on position offsets
            Vector3 pos = cell.transform.position;

            MazeCell rightNeighbor = FindCellAtPosition(cells, pos + Vector3.right * cellSpacing);
            MazeCell leftNeighbor = FindCellAtPosition(cells, pos + Vector3.left * cellSpacing);
            MazeCell frontNeighbor = FindCellAtPosition(cells, pos + Vector3.forward * cellSpacing);
            MazeCell backNeighbor = FindCellAtPosition(cells, pos + Vector3.back * cellSpacing);

            //connect only if there's no wall in between
            if (rightNeighbor != null && !cell.GetRightWall().activeSelf && !rightNeighbor.GetLeftWall().activeSelf)
                connections.Add(cellNodeMap[rightNeighbor]);

            if (leftNeighbor != null && !cell.GetLeftWall().activeSelf && !leftNeighbor.GetRightWall().activeSelf)
                connections.Add(cellNodeMap[leftNeighbor]);

            if (frontNeighbor != null && !cell.GetFrontWall().activeSelf && !frontNeighbor.GetBackWall().activeSelf)
                connections.Add(cellNodeMap[frontNeighbor]);

            if (backNeighbor != null && !cell.GetBackWall().activeSelf && !backNeighbor.GetFrontWall().activeSelf)
                connections.Add(cellNodeMap[backNeighbor]);

            node.ConnectsTo = connections.ToArray();
            Debug.Log($"{node.name} connected to {connections.Count} nodes");
        }

        Debug.Log($"Generated {generatedNodes.Count} total nodes and established connections");
    }

    private MazeCell FindCellAtPosition(MazeCell[] cells, Vector3 targetPos)
    {
        foreach (MazeCell cell in cells)
        {
            if (Vector3.Distance(cell.transform.position, targetPos) < 0.1f)
                return cell;
        }
        return null;
    }

    private void ClearExistingNodes()
    {
        foreach (Node node in generatedNodes)
        {
            if (node != null)
                Destroy(node.gameObject);
        }
        generatedNodes.Clear();
    }
}