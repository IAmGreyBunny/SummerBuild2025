using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A persistent Singleton that holds all critical game data.
/// This object will not be destroyed when loading new scenes.
/// </summary>
public class GameDataManager : MonoBehaviour
{
    // The static instance that makes this a Singleton.
    public static GameDataManager Instance { get; private set; }

    // Data to be persisted across scenes.
    public List<PetData> ownerPets = new List<PetData>();
    public PetData selectedPet;

    private void Awake()
    {
        // --- Singleton Pattern Implementation ---
        // If an instance already exists and it's not me, destroy myself.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return; // Important: exit the method after destroying
        }
        // Otherwise, I am the one and only instance.
        Instance = this;

        // --- Persistence Implementation ---
        // Do not destroy this GameObject when a new scene is loaded.
        DontDestroyOnLoad(gameObject);
    }
}
