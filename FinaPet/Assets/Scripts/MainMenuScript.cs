using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    public void LoadHomeScene()
    {
        Debug.Log("Loading Home Scene...");
        //SceneManager.LoadScene("Home");
        Debug.Log("Home Scene Loaded");
    }

    public void LoadShopScene()
    {
        Debug.Log("Loading Shop Scene...");
        SceneManager.LoadScene("Shop");
        Debug.Log("Shop Scene Loaded");
    }

    public void LoadInventoryScene()
    {
        Debug.Log("Loading Inventory Scene...");
        SceneManager.LoadScene("Inventory");
        Debug.Log("Inventory Scene Loaded");
    }

    public void LoadMyCollectionScene()
    {
        Debug.Log("Load My Collection Scene...");
        SceneManager.LoadScene("My Collection");
        Debug.Log("My Collection Scene Loaded");
    }

    public void LoadDiaryScene()
    {
        Debug.Log("Load Diary Scene...");
        SceneManager.LoadScene("Diary");
        Debug.Log("My Diary Scene Loaded");
    }

    public void LoadMyPetsScene()
    {
        Debug.Log("Load My Pets Scene...");
        SceneManager.LoadScene("My Pets");
        Debug.Log("My Pets Scene Loaded");
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
    }

}
