using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Required for SceneManager.LoadScene
using System.IO; // Required for File.Exists and File.ReadAllText in ServerConfig context

public class RegistrationFormController : MonoBehaviour
{
    private TMP_InputField _usernameInput;
    private TMP_InputField _emailInput;
    private TMP_InputField _confirmEmailInput;
    private TMP_InputField _passwordInput;
    private TMP_InputField _confirmPasswordInput;
    private TMP_Text _errorText; // NEW: Reference to the TextMeshProUGUI for error messages

    public void Awake()
    {
        Debug.Log("Parent: " + transform.parent?.name);
        _usernameInput = transform.parent.Find("UsernameTextInput/InputField").GetComponent<TMP_InputField>();
        _emailInput = transform.parent.Find("EmailTextInput/InputField").GetComponent<TMP_InputField>();
        _confirmEmailInput = transform.parent.Find("ConfirmEmailTextInput/InputField").GetComponent<TMP_InputField>();
        _passwordInput = transform.parent.Find("PasswordTextInput/InputField").GetComponent<TMP_InputField>();
        _confirmPasswordInput = transform.parent.Find("ConfirmPasswordTextInput/InputField").GetComponent<TMP_InputField>();

        // NEW: Get reference to the ErrorText GameObject
        // Assuming ErrorText is a direct child of the parent of RegistrationFormController,
        // or you might need to adjust this path based on your UI hierarchy.
        Transform errorTextTransform = transform.parent.Find("ErrorMessagePanel/ErrorText");
        if (errorTextTransform != null)
        {
            _errorText = errorTextTransform.GetComponent<TMP_Text>();
            if (_errorText == null)
            {
                Debug.LogError("[RegistrationFormController] ErrorText GameObject found, but no TMP_Text component attached.");
            }
        }
        else
        {
            Debug.LogError("[RegistrationFormController] ErrorText GameObject not found as a child of the parent. Make sure your UI hierarchy is correct.");
        }
    }

    public void CallRegister()
    {
        StartCoroutine(Register());
    }

    IEnumerator Register()
    {
        // NEW: Clear any previous error messages when starting a new registration attempt
        if (_errorText != null)
        {
            _errorText.text = "";
        }

        WWWForm form = new WWWForm();
        // Sanity check on inputfields
        if (verifyInputs())
        {
            form.AddField("username", _usernameInput.text);
            form.AddField("email", _emailInput.text);
            form.AddField("password", _passwordInput.text);
        }
        else
        {
            // verifyInputs() already handles setting the error text.
            yield break; // Exit coroutine if inputs are not valid
        }

        // Load API path from ServerConfig, following the existing project pattern.
        string apiPath = "";
        ServerConfig loadedConfig = ServerConfig.LoadFromFile("Config/ServerConfig.json"); //
        if (loadedConfig != null)
        {
            apiPath = loadedConfig.GetApiPath(); //
        }
        else
        {
            Debug.LogError("[RegistrationFormController] Failed to load ServerConfig. Cannot proceed with registration.");
            SetErrorText("Failed to load server configuration. Please try again later."); // NEW: Output to UI
            yield break;
        }

        Debug.Log("API Path: " + apiPath);
        UnityWebRequest request = UnityWebRequest.Post(apiPath + "/register.php", form); //
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Registration request error: " + request.error);
            SetErrorText("Network error during registration. Please check your internet connection."); // NEW: Output to UI
        }
        else
        {
            Debug.Log("Raw Registration Response: " + request.downloadHandler.text);
            _RegistrationResponse registrationResponse = JsonUtility.FromJson<_RegistrationResponse>(request.downloadHandler.text);

            if (registrationResponse.status_code == 0) // Registration Successful
            {
                Debug.Log("Registration Successful for player ID: " + registrationResponse.player_id + ". Attempting auto-login."); //
                SetErrorText("Registration successful! Logging in...", true); // NEW: Positive message to UI

                // --- Automatic login and scene loading ---
                PlayerDataManager.ResetPlayerData(); // Clear any old player data.

                Debug.Log("Attempting to fetch player data for auto-login for player ID: " + registrationResponse.player_id); //
                //yield return PlayerDataManager.FetchPlayerData(registrationResponse.player_id); // Fetch player data.
                yield return Login(_usernameInput.text, _passwordInput.text);

                if (PlayerAuthSession.IsLoggedIn) //
                {
                    Debug.Log($"Successfully auto-logged in player ID: {PlayerAuthSession.PlayerId}. Loading Intro Video scene."); //
                    SceneManager.LoadScene("Intro Video"); // Load the Intro Video scene.
                }
                else
                {
                    Debug.LogError("Auto-login failed after registration: " + PlayerDataManager.LastErrorMessage); //
                    SetErrorText("Auto-login failed. Please try logging in manually.", false); // NEW: Output to UI
                    // Fallback to showing the login form if auto-login fails
                    GameObject formRenderController = GameObject.Find("FormRenderController");
                    // Assuming FormRenderScript is actually AuthFormRenderScript based on your project structure.
                    formRenderController.GetComponent<FormRenderScript>().showLoginForm(); //
                }
            }
            else // Registration Failed on server side
            {
                Debug.LogWarning($"User registration Failed with status code: {registrationResponse.status_code}, message: {registrationResponse.error_message}"); //
                SetErrorText(registrationResponse.error_message, false); // NEW: Output server error message to UI
            }
        }
    }


    IEnumerator Login(String Username, String Password)
    {
        WWWForm form = new WWWForm();
        // Sanity check on inputfields
        if (verifyInputs())
        {
            form.AddField("username", Username);
            form.AddField("password", Password);
        }
        else
        {
            yield return null;
        }

        string apiPath = ServerConfig.LoadFromFile("Config/ServerConfig.json").GetApiPath();
        Debug.Log("API Path: " + apiPath);
        UnityWebRequest request = UnityWebRequest.Post(apiPath + "/login.php", form);
        yield return request.SendWebRequest();


        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Request error: " + request.error);
        }
        else
        {
            _LoginResponse loginResponse = JsonUtility.FromJson<_LoginResponse>(request.downloadHandler.text);

            if (loginResponse.status_code == 0)
            {
                Debug.Log("Login Successful");
                PlayerAuthSession.StartSession(loginResponse.player_data.username, loginResponse.player_data.player_id);
                PlayerDataManager.FetchPlayerData(loginResponse.player_data.player_id);
            }
            else
            {
                Debug.Log("Login Failed with message: " + loginResponse.error_message);
            }
        }
    }

    /// <summary>
    /// Performs client-side input verification for the registration form.
    /// Outputs error messages to the _errorText UI element.
    /// </summary>
    /// <returns>True if inputs are valid, false otherwise.</returns>
    public bool verifyInputs()
    {
        // Clear previous error messages for a new verification attempt
        if (_errorText != null)
        {
            _errorText.text = "";
        }

        // Checks if username is valid
        if (string.IsNullOrEmpty(_usernameInput.text))
        {
            SetErrorText("Username cannot be empty.");
            return false;
        }

        // Checks if email is valid
        if (string.IsNullOrEmpty(_emailInput.text))
        {
            SetErrorText("Email cannot be empty.");
            return false;
        }
        // Basic email format check (can be expanded)
        if (!IsValidEmail(_emailInput.text))
        {
            SetErrorText("Please enter a valid email address.");
            return false;
        }

        // Checks if confirmEmail is same as email
        if (_emailInput.text != _confirmEmailInput.text)
        {
            SetErrorText("Emails do not match.");
            return false;
        }

        // Checks if password is valid
        if (string.IsNullOrEmpty(_passwordInput.text))
        {
            SetErrorText("Password cannot be empty.");
            return false;
        }
        // Example: Password length requirement
        if (_passwordInput.text.Length < 6)
        {
            SetErrorText("Password must be at least 6 characters long.");
            return false;
        }

        // Checks if confirmPassword is same as password
        if (_passwordInput.text != _confirmPasswordInput.text)
        {
            SetErrorText("Passwords do not match.");
            return false;
        }

        // All checks passed
        return true;
    }

    /// <summary>
    /// Helper method to set the error text on the UI.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="isSuccess">Optional: set to true for success messages (e.g., green color), false for errors (red).</param>
    private void SetErrorText(string message, bool isSuccess = false)
    {
        if (_errorText != null)
        {
            _errorText.text = message;
            // Optionally change color based on success/error
            _errorText.color = isSuccess ? Color.green : Color.red;
        }
    }

    /// <summary>
    /// Basic email validation regex.
    /// </summary>
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    // --- Backend Response Data Structure (Matches your register.php output) ---
    [Serializable]
    private class _RegistrationResponse
    {
        public int status_code; //
        public string error_message; //
        public int player_id; // CRITICAL: This must be returned by your register.php on success.
    }

    [Serializable]
    private class _LoginResponse
    {
        public int status_code;
        public string error_message;
        public _PlayerData player_data;
    }

    [Serializable]
    private class _PlayerData
    {
        public int player_id;
        public string username;
    }
}