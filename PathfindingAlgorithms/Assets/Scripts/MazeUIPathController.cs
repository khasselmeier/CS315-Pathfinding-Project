using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

public class MazeUIPathController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button generateMazeButton;
    [SerializeField] private Button setStartButton;
    [SerializeField] private Button setEndButton;
    [SerializeField] private TMP_Text selectedText;

    [Header("Maze Settings")]
    [SerializeField] private MazeGenerator mazeGeneratorPrefab;
    [SerializeField] private Transform mazeParent;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask wallLayerMask; //for deleting
    [SerializeField] private LayerMask selectableLayerMask; //for selecting

    private MazeGenerator currentMaze;
    private Camera mainCamera;

    private GameObject selectedObject;
    private Renderer selectedRenderer;
    private Color selectedOriginalColor;

    private GameObject startObject;
    private Renderer startRenderer;

    private GameObject endObject;
    private Renderer endRenderer;

    //colors for different selection states (start=green, end=red, selected=yellow)
    private readonly Color startColor = Color.green;
    private readonly Color endColor = Color.red;
    private readonly Color selectedColor = Color.yellow;

    //stores the original colors of maze cells to restore later (walls are pink, floor is gray)
    private readonly Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();

    //so multiple scripts don't interfere with the same click event in a single frame
    public static bool ClickHandledByUI = false;

    void Start()
    {
        mainCamera = Camera.main;

        //asign button listeners (so we don't have to in the button component itself)
        if (generateMazeButton != null)
            generateMazeButton.onClick.AddListener(GenerateNewMaze);

        if (setStartButton != null)
            setStartButton.onClick.AddListener(ConfirmStartPoint);

        if (setEndButton != null)
            setEndButton.onClick.AddListener(ConfirmEndPoint);

        //set default UI text
        if (selectedText != null)
            selectedText.text = "Selected: None";

        //create a maze parent container if none is assigned (helps with deleting maze)
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
        //handles raycast-based object selection each frame
        HandleSelection();
    }

    void LateUpdate()
    {
        // Resets click flag each frame so other scripts can read fresh input next frame
        ClickHandledByUI = false;
    }

    // -------------------- Maze Generation --------------------
    private void GenerateNewMaze()
    {
        //remove any previously generated maze before creating a new one (so mazes don't overlap)
        ClearOldMaze();

        //instantiate new maze from prefab
        if (mazeGeneratorPrefab != null)
        {
            GameObject instance = Instantiate(mazeGeneratorPrefab.gameObject, Vector3.zero, Quaternion.identity, mazeParent);

            //reset transform to keep maze aligned correctly (0,0,0)
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            //find MazeGenerator component in the instance
            currentMaze = instance.GetComponentInChildren<MazeGenerator>();
            Debug.Log("New maze generated successfully");
        }
        else
        {
            Debug.LogWarning("mazeGeneratorPrefab is not assigned");
        }

        //reset UI state after generation
        ClearSelection();
        selectedText.text = "Selected: None";
    }

    private void ClearOldMaze()
    {
        //destroy all maze child objects before generating a new one
        if (mazeParent != null)
        {
            for (int i = mazeParent.childCount - 1; i >= 0; i--)
                Destroy(mazeParent.GetChild(i).gameObject);
        }

        //destroy the maze generator object if it exists
        if (currentMaze != null)
        {
            Destroy(currentMaze.gameObject);
            currentMaze = null;
        }
    }

    // -------------------- Selection Logic --------------------
    private void HandleSelection()
    {
        //prevent clicks from interacting with the scene if UI elements are under the mouse
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (mainCamera == null)
            return;

        //left-click -> select cell
        if (Input.GetMouseButtonDown(0))
        {
            ClickHandledByUI = true;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            //check if we hit something on the selectable layers (walls, floors, etc.)
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableLayerMask))
            {
                GameObject hitObj = hit.collider.gameObject;
                SelectObject(hitObj);
            }
            else
            {
                ClearSelection();
            }
        }

        //right-click -> delete wall/floor
        if (Input.GetMouseButtonDown(1) && selectedObject != null && selectedObject.CompareTag("Wall"))
        {
            ClickHandledByUI = true;
            Destroy(selectedObject);        //permanently removes wall from scene
            ClearSelection();               //reset selection highlight
            selectedText.text = "Deleted wall";
        }
    }

    private void SelectObject(GameObject obj)
    {
        //deselect any previous object first
        ClearSelection();

        selectedObject = obj;
        selectedRenderer = obj.GetComponent<Renderer>();

        if (selectedRenderer != null)
        {
            //give each wall/floor its own material instance so color changes don’t affect others
            selectedRenderer.material = new Material(selectedRenderer.material);

            //save its original color the first time we interact with it
            if (!originalColors.ContainsKey(obj))
                originalColors[obj] = selectedRenderer.material.color;

            selectedOriginalColor = originalColors[obj];

            //highlight the selected object (yellow)
            selectedRenderer.material.color = selectedColor;
        }

        //update UI text
        if (selectedText != null)
            selectedText.text = $"Selected: {obj.name}";
    }

    private void ClearSelection()
    {
        //restore original color only if this object isn’t the start or end point
        if (selectedRenderer != null && selectedObject != startObject && selectedObject != endObject)
        {
            if (originalColors.TryGetValue(selectedObject, out Color original))
                selectedRenderer.material.color = original;
        }

        selectedObject = null;
        selectedRenderer = null;
    }

    // -------------------- Start / End Confirmation --------------------
    private void ConfirmStartPoint()
    {
        if (selectedObject == null) return;

        //if a previous start point exists (and isn’t also the end), restore its color
        if (startRenderer != null && startObject != endObject)
            RestoreOriginalColor(startObject, startRenderer);

        //assign new start object
        startObject = selectedObject;
        startRenderer = startObject.GetComponent<Renderer>();

        //remember its default color if not already tracked
        if (!originalColors.ContainsKey(startObject))
            originalColors[startObject] = startRenderer.material.color;

        //apply start color (green)
        startRenderer.material.color = startColor;

        Debug.Log($"Start point set: {startObject.name}");
    }

    private void ConfirmEndPoint()
    {
        if (selectedObject == null) return;

        //if a previous end point exists (and isn’t also the start), restore its color
        if (endRenderer != null && endObject != startObject)
            RestoreOriginalColor(endObject, endRenderer);

        //assign new end object
        endObject = selectedObject;
        endRenderer = endObject.GetComponent<Renderer>();

        if (!originalColors.ContainsKey(endObject))
            originalColors[endObject] = endRenderer.material.color;

        //apply end color (red)
        endRenderer.material.color = endColor;

        Debug.Log($"End point set: {endObject.name}");
    }

    private void RestoreOriginalColor(GameObject obj, Renderer renderer)
    {
        //avoid missing references
        if (obj == null || renderer == null)
            return;

        //restore original color (pinkish for walls, gray for floor)
        if (originalColors.TryGetValue(obj, out Color original))
            renderer.material.color = original;
    }

    // -------------------- Getters --------------------
    public GameObject GetStartPoint() => startObject; //accessor for pathfinding scripts
    public GameObject GetEndPoint() => endObject;     //accessor for pathfinding scripts
}