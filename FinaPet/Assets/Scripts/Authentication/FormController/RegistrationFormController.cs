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

        Debug.Log("[04:53:41] --- Starting Input Verification ---"); // This is from your screenshot, keep it here.

        // --- CRITICAL: Perform client-side input verification FIRST ---
        if (!verifyInputs())
        {
            // If verifyInputs() returns false, it means there was a validation error.
            // The error message has already been set by verifyInputs().
            Debug.LogWarning("Verification FAILED. Stopping registration process.");
            yield break; // IMMEDIATELY exit the coroutine if validation fails.
        }

        // If we reach here, it means client-side verification PASSED.
        // Now, proceed with preparing the form for the server request.
        WWWForm form = new WWWForm();
        form.AddField("username", _usernameInput.text);
        form.AddField("email", _emailInput.text);
        form.AddField("password", _passwordInput.text);

        // Load API path from ServerConfig, following the existing project pattern.
        string apiPath = "";
        // RESTORED: Use LoadFromFile as it exists in your ServerConfig
        ServerConfig loadedConfig = ServerConfig.LoadFromFile("Config/ServerConfig.json");
        if (loadedConfig != null)
        {
            apiPath = loadedConfig.GetApiPath();
        }
        else
        {
            Debug.LogError("[RegistrationFormController] Failed to load ServerConfig. Cannot proceed with registration.");
            SetErrorText("Failed to load server configuration. Please try again later.");
            yield break;
        }

        Debug.Log("API Path: " + apiPath);
        UnityWebRequest request = UnityWebRequest.Post(apiPath + "/register.php", form);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Registration request error: " + request.error);
            SetErrorText("Network error during registration. Please check your internet connection.");
        }
        else
        {
            Debug.Log("Raw Registration Response: " + request.downloadHandler.text);
            _RegistrationResponse registrationResponse = JsonUtility.FromJson<_RegistrationResponse>(request.downloadHandler.text);

            if (registrationResponse.status_code == 0) // Registration Successful (server-side)
            {
                Debug.Log("Registration Successful for player ID: " + registrationResponse.player_id + ". Attempting auto-login.");
                SetErrorText("Registration successful! Logging in...", true);

                // --- Automatic login and scene loading ---
                PlayerDataManager.ResetPlayerData(); // Clear any old player data.

                Debug.Log("Attempting to fetch player data for auto-login for player ID: " + registrationResponse.player_id);
                yield return Login(_usernameInput.text, _passwordInput.text); // Perform auto-login

                if (PlayerAuthSession.IsLoggedIn) // Check if auto-login was successful
                {
                    Debug.Log($"Successfully auto-logged in player ID: {PlayerAuthSession.PlayerId}. Loading Intro Video scene.");
                    // This is the ONLY place where scene loading should happen after a successful registration and auto-login.
                    GetComponent<SceneLoaderHelper>().LoadTargetScene();
                }
                else
                {
                    Debug.LogError("Auto-login failed after registration: " + PlayerDataManager.LastErrorMessage);
                    SetErrorText("Auto-login failed. Please try logging in manually.", false);
                    // If auto-login fails, show the login form.
                    GameObject formRenderController = GameObject.Find("FormRenderController");
                    formRenderController.GetComponent<FormRenderScript>().showLoginForm();
                }
            }
            else // Registration Failed on server side (status_code != 0)
            {
                Debug.LogWarning($"User registration Failed with status code: {registrationResponse.status_code}, message: {registrationResponse.error_message}");
                SetErrorText(registrationResponse.error_message, false); // Output server error message to UI
                                                                         // Crucially, NO scene change here.
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
            yield break;
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
                // Wait for the data fetch to complete
                yield return PlayerDataManager.FetchPlayerData(loginResponse.player_data.player_id);
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
        // Replace your existing verifyInputs() method with this one.
    public bool verifyInputs()
    {
        Debug.Log("--- Starting Input Verification ---");

        if (_errorText != null)
        {
            _errorText.text = "";
        }

        if (string.IsNullOrEmpty(_usernameInput.text))
        {
            SetErrorText("Username cannot be empty.");
            Debug.LogWarning("Verification FAILED: Username is empty.");
            return false;
        }

        if (string.IsNullOrEmpty(_emailInput.text))
        {
            SetErrorText("Email cannot be empty.");
            Debug.LogWarning("Verification FAILED: Email is empty.");
            return false;
        }

        if (!IsValidEmail(_emailInput.text))
        {
            SetErrorText("Please enter a valid email address.");
            Debug.LogWarning("Verification FAILED: Email format is invalid.");
            return false;
        }

        if (_emailInput.text != _confirmEmailInput.text)
        {
            SetErrorText("Emails do not match.");
            Debug.LogWarning("Verification FAILED: Emails do not match.");
            return false;
        }

        if (string.IsNullOrEmpty(_passwordInput.text))
        {
            SetErrorText("Password cannot be empty.");
            Debug.LogWarning("Verification FAILED: Password is empty.");
            return false;
        }

        // --- THIS IS THE MOST IMPORTANT PART ---
        Debug.Log($"Checking password length. Text is '{_passwordInput.text}', Length is {_passwordInput.text.Length}");
        if (_passwordInput.text.Length < 6)
        {
            SetErrorText("Password must be at least 6 characters long.");
            Debug.LogWarning("Verification FAILED: Password is too short.");
            return false; // This should be triggering
        }
        // -----------------------------------------

        if (_passwordInput.text != _confirmPasswordInput.text)
        {
            SetErrorText("Passwords do not match.");
            Debug.LogWarning("Verification FAILED: Passwords do not match.");
            return false;
        }

        Debug.Log("--- Verification PASSED ---");
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