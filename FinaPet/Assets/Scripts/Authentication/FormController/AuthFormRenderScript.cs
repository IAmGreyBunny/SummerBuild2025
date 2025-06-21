using UnityEngine;

public class FormRenderScript : MonoBehaviour
{

    public GameObject loginFormPrefab;
    public GameObject registrationFormPrefab;
    public Transform formParent; // Keeps track of the location that form will shown
    private GameObject _currentForm; // Keeps track of the currently rendered form


    //Load up initial state, ensuring that a form will always be shown
    private void Start()
    {
        _currentForm = Instantiate(loginFormPrefab, formParent);
    }

    public void showLoginForm()
    {
        // Remove existing form
        if (_currentForm != null)
        {
            Destroy(_currentForm);
        }
        // Load login form under formParent
        _currentForm = Instantiate(loginFormPrefab, formParent);
    }

    public void showRegistrationForm()
    {
        // Remove existing form
        if (_currentForm != null)
        {
            Destroy(_currentForm);
        }
        // Load login form under formParent
        _currentForm = Instantiate(registrationFormPrefab, formParent);
    }
}
