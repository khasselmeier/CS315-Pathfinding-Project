using UnityEngine;
using UnityEngine.EventSystems;

public class PathfindingUIController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask cellLayerMask;

    private Camera mainCamera;

    private MazeCell startCell;
    private MazeCell endCell;
    private MazeCell pendingCell;

    private Renderer startRenderer;
    private Renderer endRenderer;
    private Renderer pendingRenderer;

    private Color startColor = Color.green;
    private Color endColor = Color.red;
    private Color pendingColor = new Color(1f, 0.8f, 0.2f); //yellow color
    private Color defaultColor = Color.white;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleCellSelection();
    }

    private void HandleCellSelection()
    {
        //skip if clicking UI
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (mainCamera == null)
            return;

        //left-click to pick a pending cell
        if (Input.GetMouseButtonDown(0))
        {
            MazeCell hitCell = RaycastForCell();
            if (hitCell != null)
            {
                SetPendingCell(hitCell);
            }
        }
    }

    private MazeCell RaycastForCell()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, cellLayerMask))
        {
            //only accept hit if the collider or its parent has a MazeCell
            MazeCell cell = hit.collider.GetComponentInParent<MazeCell>();

            //ignore if we accidentally hit something tagged "Wall"
            if (cell != null && !hit.collider.CompareTag("Wall"))
                return cell;
        }
        return null;
    }


    private void SetPendingCell(MazeCell cell)
    {
        //reset previous pending color (if it’s not start or end)
        if (pendingRenderer != null && pendingCell != startCell && pendingCell != endCell)
            pendingRenderer.material.color = defaultColor;

        pendingCell = cell;
        pendingRenderer = pendingCell.GetComponentInChildren<Renderer>();

        if (pendingRenderer != null && pendingCell != startCell && pendingCell != endCell)
            pendingRenderer.material.color = pendingColor;

        Debug.Log($"Pending cell selected: {pendingCell.name}");
    }

    //assign to the set start button
    public void ConfirmStartCell()
    {
        if (pendingCell == null) return;

        // Reset old start color
        if (startRenderer != null && startCell != endCell)
            startRenderer.material.color = defaultColor;

        startCell = pendingCell;
        startRenderer = startCell.GetComponentInChildren<Renderer>();
        if (startRenderer != null)
            startRenderer.material.color = startColor;

        Debug.Log($"Start cell set to: {startCell.name}");
    }

    //assign to the set end button
    public void ConfirmEndCell()
    {
        if (pendingCell == null) return;

        //reset old end color
        if (endRenderer != null && endCell != startCell)
            endRenderer.material.color = defaultColor;

        endCell = pendingCell;
        endRenderer = endCell.GetComponentInChildren<Renderer>();
        if (endRenderer != null)
            endRenderer.material.color = endColor;

        Debug.Log($"End cell set to: {endCell.name}");
    }

    public MazeCell GetStartCell() => startCell;
    public MazeCell GetEndCell() => endCell;
}
