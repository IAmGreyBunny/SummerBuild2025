using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A persistent Singleton that holds all critical game data.
/// This object will not be destroyed when loading new scenes.
/// </summary>
public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    public List<PetData> ownerPets = new List<PetData>();
    public PetData selectedPet;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
