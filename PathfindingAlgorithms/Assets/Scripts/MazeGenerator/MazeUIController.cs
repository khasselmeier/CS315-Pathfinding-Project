using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MazeUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button generateMazeButton;
    [SerializeField] private TMP_Text selectedWallText;

    [Header("Maze Settings")]
    [SerializeField] private MazeGenerator mazeGeneratorPrefab;
    [SerializeField] private Transform mazeParent;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask wallLayerMask;

    private MazeGenerator currentMaze;
    private GameObject selectedWall;
    private Renderer selectedWallRenderer;
    private Color originalColor;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        if (generateMazeButton != null)
            generateMazeButton.onClick.AddListener(GenerateNewMaze);

        if (selectedWallText != null)
            selectedWallText.text = "Selected Wall: None";

        //create parent if not assigned
        if (mazeParent == null)
        {
            GameObject go = new GameObject("MazeParent");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;
            mazeParent = go.transform;
        }
    }

    void Update()
    {
        HandleWallSelection();
    }

    private void GenerateNewMaze()
    {
        //Debug.Log("generating new maze...");

        //clear previous maze
        ClearOldMaze();

        //instantiate new maze from prefab
        if (mazeGeneratorPrefab != null)
        {
            //instantiate the prefab
            GameObject instance = Instantiate(mazeGeneratorPrefab.gameObject, Vector3.zero, Quaternion.identity, mazeParent);

            //make sure it's visible and positioned correctly (0,0,0)
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            //seperate MazeGenerator from the instance or its children
            currentMaze = instance.GetComponent<MazeGenerator>();
            if (currentMaze == null)
            {
                currentMaze = instance.GetComponentInChildren<MazeGenerator>();
            }

            if (currentMaze == null)
            {
                return;
            }

            Debug.Log("New maze generated successfully");
        }
        else
        {
            Debug.LogWarning("mazeGeneratorPrefab is not assigned");
        }

        //reset UI state
        selectedWall = null;
        if (selectedWallText != null)
            selectedWallText.text = "Selected Wall: None";
    }

    private void ClearOldMaze()
    {
        //destroy all children of mazeParent
        if (mazeParent != null)
        {
            for (int i = mazeParent.childCount - 1; i >= 0; i--)
            {
                Destroy(mazeParent.GetChild(i).gameObject);
            }
        }

        //destroy the current maze gameobject, if it exists
        if (currentMaze != null)
        {
            Destroy(currentMaze.gameObject);
            currentMaze = null;
        }
    }

    private void HandleWallSelection()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (mainCamera == null)
            return;

        //left-click to select wall
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, wallLayerMask))
            {
                SelectWall(hit.collider.gameObject);
            }
            else
            {
                ClearWallSelection();
            }
        }

        //right-click to delete selected wall
        if (Input.GetMouseButtonDown(1) && selectedWall != null)
        {
            Destroy(selectedWall);
            ClearWallSelection();

            if (selectedWallText != null)
                selectedWallText.text = "Deleted wall";
        }
    }

    private void SelectWall(GameObject wall)
    {
        ClearWallSelection();

        selectedWall = wall;
        selectedWallRenderer = selectedWall.GetComponent<Renderer>();

        if (selectedWallRenderer != null)
        {
            //give each wall its own material instance to avoid all walls turning red
            selectedWallRenderer.material = new Material(selectedWallRenderer.material);

            originalColor = selectedWallRenderer.material.color;
            selectedWallRenderer.material.color = Color.red;
        }

        if (selectedWallText != null)
            selectedWallText.text = $"Selected Wall: {selectedWall.name}";
    }

    private void ClearWallSelection()
    {
        if (selectedWallRenderer != null)
        {
            selectedWallRenderer.material.color = originalColor;
            selectedWallRenderer = null;
        }

        selectedWall = null;
    }
}