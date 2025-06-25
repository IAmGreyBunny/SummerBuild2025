using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AnimalHighlighter : MonoBehaviour
{
    public Color highlightColor = Color.yellow;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;
    private bool isHighlighted = false;
    public Canvas canvas;

    public GameObject interactionPanel; // Assign in Inspector
    public Button feedButton, petButton;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        // Check if the panel is assigned, but don't set it active here.
        if (interactionPanel != null)
        {
            interactionPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("AnimalHighlighter: interactionPanel is not assigned!");
        }

        // REMOVE the button listener assignments from here.
        // feedButton.onClick.AddListener(FeedAnimal);
        // petButton.onClick.AddListener(PetAnimal);
    }

    public void InitializeButtons()
    {
        // This method will be called by the spawner after it assigns the buttons.
        if (feedButton != null)
        {
            feedButton.onClick.AddListener(FeedAnimal);
        }
        else
        {
            Debug.LogError("AnimalHighlighter: feedButton reference is null during initialization!");
        }

        if (petButton != null)
        {
            petButton.onClick.AddListener(PetAnimal);
        }
        else
        {
            Debug.LogError("AnimalHighlighter: petButton reference is null during initialization!");
        }
    }

    void Update()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePosition);
        Vector2 mousePos2D = new Vector2(worldMousePos.x, worldMousePos.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
        if (hit.collider != null && hit.collider.gameObject == gameObject)
        {
            if (!isHighlighted)
            {
                spriteRenderer.color = highlightColor;
                isHighlighted = true;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                ShowInteractionPanel();
            }
        }
        else if (isHighlighted)
        {
            spriteRenderer.color = originalColor;
            isHighlighted = false;
        }
    }

    void ShowInteractionPanel()
    {
        Debug.Log("Showing panel");
        interactionPanel.SetActive(true);

        //Vector2 mousePos = Mouse.current.position.ReadValue();
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(
        //    canvas.transform as RectTransform,
        //    mousePos,
        //    canvas.worldCamera,
        //    out Vector2 localPoint
        //);

        //interactionPanel.GetComponent<RectTransform>().localPosition = localPoint;
    }

    void FeedAnimal()
    {
        Debug.Log("Feeding the animal.");
        interactionPanel.SetActive(false);
    }

    void PetAnimal()
    {
        Debug.Log("Petting the animal.");
        interactionPanel.SetActive(false);
    }
}
