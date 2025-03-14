using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro; 

public class AuthManager : MonoBehaviour
{
    public TMP_InputField inputFieldUsername;
    public TMP_InputField inputFieldPassword;
    public TMP_Text errorText;
    public GameObject panelAuth; // Panel de autenticación
    
    private string baseUrl = "https://sid-restapi.onrender.com";
    private string token;
    private string username;

    void Start()
    {
        Logout();
        token = PlayerPrefs.GetString("token");
        username = PlayerPrefs.GetString("username");

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(username))
        {
            Debug.Log("No hay token, mostrar login");
            panelAuth.SetActive(true);
        }
        else
        {
            StartCoroutine(GetPerfil());
        }
    }

    public void Login()
    {
        Credentials credentials = new Credentials { username = inputFieldUsername.text, password = inputFieldPassword.text };
        StartCoroutine(LoginPost(JsonUtility.ToJson(credentials)));
    }

    public void Registro()
    {
        Credentials credentials = new Credentials { username = inputFieldUsername.text, password = inputFieldPassword.text };
        StartCoroutine(RegisterPost(JsonUtility.ToJson(credentials)));
    }

    IEnumerator RegisterPost(string postData)
    {
    string path = "/api/usuarios";
    using (UnityWebRequest www = new UnityWebRequest(baseUrl + path, "POST"))
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(postData);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Registro exitoso");
            StartCoroutine(LoginPost(postData)); 
        }
        else
        {
            string errorMessage = www.downloadHandler.text; 
            Debug.LogError("Error en registro: " + errorMessage);

            ShowError(errorMessage); 
        }
    }
    }
    
    IEnumerator LoginPost(string postData)
    {
    string path = "/api/auth/login";
    using (UnityWebRequest www = new UnityWebRequest(baseUrl + path, "POST"))
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(postData);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            AuthResponse response = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);
            PlayerPrefs.SetString("token", response.token);
            PlayerPrefs.SetString("username", response.usuario.username);
            PlayerPrefs.Save();
            panelAuth.SetActive(false);
            Debug.Log("Login exitoso");
        }
        else
        {
            string errorMessage = www.downloadHandler.text;
            Debug.LogError("Error en login: " + errorMessage);

            ShowError(errorMessage); 
        }
    }
    }

    IEnumerator GetPerfil()
    {
        string path = "/api/usuarios";
        using (UnityWebRequest www = UnityWebRequest.Get(baseUrl + path))
        {
            string storedToken = PlayerPrefs.GetString("token", "");
            if (string.IsNullOrEmpty(storedToken))
            {
                Debug.LogError("No hay token almacenado. Redirigir a login.");
                yield break;
            }

            www.SetRequestHeader("x-token", storedToken);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error al obtener perfil: " + www.error);
            }
            else
            {
                if (www.responseCode == 200)
                {
                    string json = www.downloadHandler.text;
                    AuthResponse response = JsonUtility.FromJson<AuthResponse>(json);

                    // Ocultar la interfaz de autenticación
                    GameObject panelAuth = GameObject.Find("PanelAuth");
                    if (panelAuth != null)
                    {
                        panelAuth.SetActive(false);
                    }

                    Debug.Log("Perfil obtenido exitosamente.");
                }
                else
                {
                    Debug.LogError("Token vencido. Redirigir a login.");
                }
            }
        }
    }

    public bool IsUserAuthenticated()
    {
    string storedToken = PlayerPrefs.GetString("token", "");
    return !string.IsNullOrEmpty(storedToken);
    }

    public void Logout()
    {
    PlayerPrefs.DeleteKey("token");    
    PlayerPrefs.DeleteKey("username"); 
    PlayerPrefs.Save();                

    panelAuth.SetActive(true);
    Debug.Log("Sesión cerrada."); 
    }

    void ShowError(string message)
    {
    if (errorText != null)
    {
        errorText.text = "Error: " + message;
        errorText.gameObject.SetActive(true);
    }
    }

}

// Clases para deserializar JSON 
[System.Serializable]
public class Credentials
{
    public string username;
    public string password;
}

[System.Serializable]
public class AuthResponse
{
    public string token;
    public User usuario;
}

[System.Serializable]
public class User
{
    public string username;
}