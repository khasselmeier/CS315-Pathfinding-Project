using System.Collections.Generic;
using UnityEngine;

public class NodeGenerator : MonoBehaviour
{
    [Header("References")]
    public Transform mazeParent;
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private float nodeYOffset = 0.2f;
    [SerializeField] private float cellSpacing = 2f; // distance between adjacent cells

    private List<Node> generatedNodes = new List<Node>();

    public void GenerateNodes()
    {
        ClearExistingNodes();

        MazeCell[] cells = mazeParent.GetComponentsInChildren<MazeCell>();
        Debug.Log($"Found {cells.Length} MazeCells under {mazeParent.name}");

        if (cells.Length == 0)
        {
            Debug.LogWarning("No MazeCells found — nodes cannot be generated.");
            return;
        }

        Dictionary<MazeCell, Node> cellNodeMap = new Dictionary<MazeCell, Node>();

        //step 1: create nodes
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
            Debug.Log($"Created Node at {nodePos} for cell {cell.name}");
        }

        //step 2: connect nodes based on adjacency
        foreach (MazeCell cell in cells)
        {
            Node node = cellNodeMap[cell];
            List<Node> connections = new List<Node>();

            foreach (MazeCell neighbor in cells)
            {
                if (neighbor == cell) continue;

                float dist = Vector3.Distance(cell.transform.position, neighbor.transform.position);

                //connect nodes that are close enough
                if (dist > 0f && dist <= cellSpacing + 0.1f) // 0.1 buffer for floating point
                {
                    connections.Add(cellNodeMap[neighbor]);
                }
            }

            node.ConnectsTo = connections.ToArray();
            Debug.Log($"{node.name} connected to {connections.Count} nodes");
        }

        Debug.Log($"Generated {generatedNodes.Count} total nodes.");
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