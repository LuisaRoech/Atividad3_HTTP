using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro; 

public class AuthManager : MonoBehaviour
{
    public TMP_InputField inputFieldUsername; 
    public TMP_InputField inputFieldPassword;

    private string baseUrl = "https://sid-restapi.onrender.com"; // URL de la API
    private string token; // Token de autenticación
    private string username;

    void Start()
    {
        token = PlayerPrefs.GetString("token");
        username = PlayerPrefs.GetString("username");

        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(username))
        {
            Debug.Log("No hay token");
        }
        else
        {
            StartCoroutine(GetPerfil());
        }
    }

    public void Login()
    {
        Credentials credentials = new Credentials
        {
            username = inputFieldUsername.text,
            password = inputFieldPassword.text
        };

        string postData = JsonUtility.ToJson(credentials);
        StartCoroutine(LoginPost(postData));
    }

    public void Registro()
    {
        Credentials credentials = new Credentials
        {
            username = inputFieldUsername.text,
            password = inputFieldPassword.text
        };

        string postData = JsonUtility.ToJson(credentials);
        StartCoroutine(RegisterPost(postData));
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

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error en registro: " + www.error);
            }
            else
            {
                if (www.responseCode == 200 || www.responseCode == 201)
                {
                    Debug.Log("Registro exitoso: " + www.downloadHandler.text);
                    StartCoroutine(LoginPost(postData)); // Iniciar sesión después del registro
                }
                else
                {
                    Debug.LogError("Error en registro. Código: " + www.responseCode + "\nRespuesta: " + www.downloadHandler.text);
                }
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

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Error en login: " + www.error);
            }
            else
            {
                if (www.responseCode == 200)
                {
                    string json = www.downloadHandler.text;
                    AuthResponse response = JsonUtility.FromJson<AuthResponse>(json);

                    // Guardar el token y nombre de usuario
                    PlayerPrefs.SetString("token", response.token);
                    PlayerPrefs.SetString("username", response.usuario.username);
                    PlayerPrefs.Save();

                    // Ocultar la interfaz de autenticación
                    GameObject panelAuth = GameObject.Find("PanelAuth");
                    if (panelAuth != null)
                    {
                        panelAuth.SetActive(false);
                    }

                    Debug.Log("Login exitoso. Usuario: " + response.usuario.username);
                }
                else
                {
                    Debug.LogError("Error en login. Código: " + www.responseCode + "\nRespuesta: " + www.downloadHandler.text);
                }
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
