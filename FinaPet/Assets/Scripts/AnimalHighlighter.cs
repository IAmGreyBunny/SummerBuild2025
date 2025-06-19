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
        interactionPanel.SetActive(false);

        // Set up button click listeners
        feedButton.onClick.AddListener(FeedAnimal);
        petButton.onClick.AddListener(PetAnimal);
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
