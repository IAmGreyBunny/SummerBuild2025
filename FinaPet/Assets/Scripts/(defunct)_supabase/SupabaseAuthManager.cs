using UnityEngine;
using Supabase; // The main Supabase namespace
using Supabase.Gotrue; // For Auth-specific models like Session and User, and Constants.AuthState
using Supabase.Gotrue.Exceptions; // Specifically for GotrueException
using System.Threading.Tasks;
using Cysharp.Threading.Tasks; // For UniTask (assuming you've installed it)
using System; // For Action

public class SupabaseAuthManager : MonoBehaviour
{
    public static Supabase.Client SupabaseClient;

    [Header("Supabase Credentials")]
    public string supabaseUrl;
    public string supabaseAnonKey;

    // Optional: Event to let other scripts know about auth state changes
    public event Action<Constants.AuthState> OnSupabaseAuthStateChange;

    private void Awake()
    {
        InitializeSupabaseClient();
    }

    private async void InitializeSupabaseClient()
    {
        var options = new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = false
        };

        SupabaseClient = new Supabase.Client(supabaseUrl, supabaseAnonKey, options);
        Debug.Log("Supabase client initialized!");

        // --- CORRECTED: Subscribing to Auth State Changes using AddStateChangedListener ---
        // This is the current recommended approach for the official Supabase C# client.
        SupabaseClient.Auth.AddStateChangedListener(HandleAuthStateChange);

        // Also, attempt to retrieve the session on startup to check if a user is already logged in
        // This ensures the client's internal state is updated from persistence.
        await SupabaseClient.Auth.RetrieveSessionAsync();
        Debug.Log($"Initial user status: {(SupabaseClient.Auth.CurrentUser != null ? "Logged In" : "Logged Out")}");

        // Optionally, immediately trigger the UI update based on current state
        // This handles cases where the user is already signed in on app launch.
        OnSupabaseAuthStateChange?.Invoke(SupabaseClient.Auth.CurrentUser != null ?
                                          Constants.AuthState.SignedIn :
                                          Constants.AuthState.SignedOut);
    }

    // Event handler method for the Auth state change
    // The signature `object sender, Constants.AuthState state` is what AddStateChangedListener expects.
    private void HandleAuthStateChange(object sender, Constants.AuthState state)
    {
        Debug.Log($"Auth state changed: {state}");
        // Trigger our custom event for other scripts to listen to
        OnSupabaseAuthStateChange?.Invoke(state);
    }

    // --- Remember to unsubscribe when the GameObject is destroyed to prevent memory leaks ---
    private void OnDestroy()
    {
        if (SupabaseClient != null && SupabaseClient.Auth != null)
        {
            SupabaseClient.Auth.RemoveStateChangedListener(HandleAuthStateChange);
        }
    }


    // Example: Sign up with email and password
    public async UniTask<Session> SignUp(string email, string password)
    {
        try
        {
            var response = await SupabaseClient.Auth.SignUp(email, password);
            Debug.Log($"Signed up user: {response.User.Email}");
            return response;
        }
        catch (GotrueException e)
        {
            Debug.LogError($"Supabase SignUp error: {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"An unexpected error occurred during SignUp: {e.Message}");
            return null;
        }
    }

    // Example: Sign in with email and password
    public async UniTask<Session> SignIn(string email, string password)
    {
        try
        {
            var response = await SupabaseClient.Auth.SignIn(email, password);
            Debug.Log($"Signed in user: {response.User.Email}");
            return response;
        }
        catch (GotrueException e)
        {
            Debug.LogError($"Supabase SignIn error: {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"An unexpected error occurred during SignIn: {e.Message}");
            return null;
        }
    }

    // Example: Sign out
    public async UniTask SignOut()
    {
        try
        {
            await SupabaseClient.Auth.SignOut();
            Debug.Log("User signed out.");
        }
        catch (GotrueException e)
        {
            Debug.LogError($"Supabase SignOut error: {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"An unexpected error occurred during SignOut: {e.Message}");
        }
    }

    // Example: Get current user
    public User GetCurrentUser()
    {
        return SupabaseClient.Auth.CurrentUser;
    }

    // Example: Check if a user is logged in
    public bool IsLoggedIn()
    {
        return GetCurrentUser() != null;
    }

    // Example: Refresh current session
    public async UniTask<Session> RefreshSession()
    {
        try
        {
            var session = await SupabaseClient.Auth.RetrieveSessionAsync();
            if (session != null)
            {
                Debug.Log("Session refreshed successfully.");
            }
            else
            {
                Debug.LogWarning("Failed to refresh session or no active session.");
            }
            return session;
        }
        catch (GotrueException e)
        {
            Debug.LogError($"Supabase RefreshSession error: {e.Message}");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"An unexpected error occurred during RefreshSession: {e.Message}");
            return null;
        }
    }
}