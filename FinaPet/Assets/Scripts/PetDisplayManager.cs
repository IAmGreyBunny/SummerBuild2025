using UnityEngine;
using UnityEngine.U2D.Animation; // Required for SpriteLibraryAsset and SpriteResolver
using System.Linq; // Required for .Any() and .ToList()
using System.Collections.Generic; // Required for List

/// <summary>
/// Manages the display of a pet's sprite using a SpriteResolver.
/// This script should be attached to the pet prefab.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))] // Ensures a SpriteRenderer is present
[RequireComponent(typeof(SpriteResolver))] // Ensures a SpriteResolver is present
public class PetDisplayManager : MonoBehaviour
{
    private SpriteResolver _spriteResolver;
    private SpriteRenderer _spriteRenderer; // Reference to the SpriteRenderer

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Gets the SpriteResolver and SpriteRenderer components.
    /// </summary>
    void Awake()
    {
        _spriteResolver = GetComponent<SpriteResolver>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteResolver == null)
        {
            Debug.LogError("SpriteResolver component not found on this GameObject. Please add one to the pet prefab.");
        }
        if (_spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on this GameObject. Please add one to the pet prefab.");
        }
    }

    /// <summary>
    /// Sets the SpriteLibraryAsset for the pet's SpriteResolver and resolves a default sprite.
    /// This changes the visual appearance of the pet.
    /// </summary>
    /// <param name="newLibrary">The SpriteLibraryAsset to apply.</param>
    public void SetSpriteLibrary(SpriteLibraryAsset newLibrary)
    {
        if (_spriteResolver != null)
        {
            // Assign the new SpriteLibraryAsset to the SpriteResolver.
            // In a standard Unity environment, this property is writable.
            _spriteResolver.spriteLibrary = newLibrary;

            // Important: After setting the new library, you need to resolve a specific sprite.
            if (newLibrary != null)
            {
                // Retrieve category and entry names directly from the SpriteLibraryAsset.
                // These methods are standard APIs in the 2D Animation package.
                List<string> categoryNames = newLibrary.GetCategoryNames().ToList();

                if (categoryNames.Any())
                {
                    string firstCategory = categoryNames[0]; // Get the first category
                    List<string> entryNames = newLibrary.GetEntryNames(firstCategory).ToList();

                    if (entryNames.Any())
                    {
                        string firstEntry = entryNames[0]; // Get the first entry within that category
                        _spriteResolver.SetCategoryAndLabel(firstCategory, firstEntry);
                        // Force a refresh to ensure the sprite is updated
                        _spriteResolver.ResolveSprite();
                        Debug.Log($"Sprite resolved to category: '{firstCategory}', label: '{firstEntry}' from library: {newLibrary.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"No entries found in category '{firstCategory}' of SpriteLibraryAsset '{newLibrary.name}'. Please ensure sprites are correctly added and labeled within this category.");
                        if (_spriteRenderer != null) _spriteRenderer.sprite = null; // Clear sprite if no entry
                    }
                }
                else
                {
                    Debug.LogWarning($"No categories found in SpriteLibraryAsset '{newLibrary.name}'. Please ensure sprites are added and categorized within the Sprite Library Asset.");
                    if (_spriteRenderer != null) _spriteRenderer.sprite = null; // Clear sprite if no categories
                }
            }
            else
            {
                Debug.LogError("Attempted to set a null SpriteLibraryAsset. Cannot resolve sprites without a valid library.");
                if (_spriteRenderer != null) _spriteRenderer.sprite = null; // Clear sprite if library is null
            }

            Debug.Log($"Sprite Library assigned: {newLibrary?.name ?? "None"}");
        }
    }
}
