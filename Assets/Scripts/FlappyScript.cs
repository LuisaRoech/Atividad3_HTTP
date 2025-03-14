using UnityEngine;
using System.Collections;

public class FlappyScript : MonoBehaviour
{
    public Sprite GetReadySprite;
    public float RotateUpSpeed = 1, RotateDownSpeed = 1;
    public GameObject PanelAuth; 
    public GameObject DeathGUI;
    public GameObject LeaderboardPanel;  
    public Collider2D restartButtonGameCollider;
    public float VelocityPerJump = 3;
    public float XSpeed = 1;

    private AuthManager authManager;
    private FlappyYAxisTravelState flappyYAxisTravelState;

    enum FlappyYAxisTravelState
    {
        GoingUp, GoingDown
    }

    Vector3 birdRotation = Vector3.zero;

    void Start()
    {
        authManager = FindObjectOfType<AuthManager>();
        if (authManager == null)
        {
            Debug.LogError("AuthManager no encontrado en la escena");
            return;
        }


        if (!authManager.IsUserAuthenticated())
        {
            PanelAuth.SetActive(true);
        }
        else
        {
            PanelAuth.SetActive(false);
        }

        DeathGUI.SetActive(false);
        LeaderboardPanel.SetActive(false);
    }

    public void OnUserAuthenticated() 
    {
        PanelAuth.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        if (GameStateManager.GameState == GameState.Intro)
        {
            MoveBirdOnXAxis();
            
            if (WasTouchedOrClicked() && authManager.IsUserAuthenticated())
            {
                BoostOnYAxis();
                GameStateManager.GameState = GameState.Playing;
                PanelAuth.SetActive(false);
                ScoreManagerScript.AddScore(-ScoreManagerScript.Score);
            }
        }
        else if (GameStateManager.GameState == GameState.Playing)
        {
            MoveBirdOnXAxis();
            if (WasTouchedOrClicked())
            {
                BoostOnYAxis();
            }
        }
        else if (GameStateManager.GameState == GameState.Dead)
        {
            // Detectar clic en botón de reinicio
            Vector2 contactPoint = Vector2.zero;

            if (Input.touchCount > 0)
                contactPoint = Input.touches[0].position;
            if (Input.GetMouseButtonDown(0))
                contactPoint = Input.mousePosition;

            if (restartButtonGameCollider == Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(contactPoint)))
            {
                RestartGame();
            }
        }
    }

    void FixedUpdate()
    {
        if (GameStateManager.GameState == GameState.Intro)
        {
            if (GetComponent<Rigidbody2D>().linearVelocity.y < -1) 
                GetComponent<Rigidbody2D>().AddForce(new Vector2(0, GetComponent<Rigidbody2D>().mass * 5500 * Time.deltaTime));
        }
        else if (GameStateManager.GameState == GameState.Playing || GameStateManager.GameState == GameState.Dead)
        {
            FixFlappyRotation();
        }
    }

    bool WasTouchedOrClicked()
    {
        return Input.GetButtonUp("Jump") || Input.GetMouseButtonDown(0) ||
               (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Ended);
    }

    void MoveBirdOnXAxis()
    {
        transform.position += new Vector3(Time.deltaTime * XSpeed, 0, 0);
    }

    void BoostOnYAxis()
    {
        GetComponent<Rigidbody2D>().linearVelocity = new Vector2(0, VelocityPerJump);
    }

    private void FixFlappyRotation()
    {
        flappyYAxisTravelState = GetComponent<Rigidbody2D>().linearVelocity.y > 0 ?
            FlappyYAxisTravelState.GoingUp : FlappyYAxisTravelState.GoingDown;

        float degreesToAdd = flappyYAxisTravelState == FlappyYAxisTravelState.GoingUp ?
            6 * RotateUpSpeed : -3 * RotateDownSpeed;

        birdRotation = new Vector3(0, 0, Mathf.Clamp(birdRotation.z + degreesToAdd, -90, 45));
        transform.eulerAngles = birdRotation;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (GameStateManager.GameState == GameState.Playing)
        {
            if (col.gameObject.tag == "Pipeblank") 
            {
                ScoreManagerScript.AddScore(1);
            }
            else if (col.gameObject.tag == "Pipe")
            {
                FlappyDies();
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (GameStateManager.GameState == GameState.Playing && col.gameObject.tag == "Floor")
        {
            FlappyDies();
        }
    }

    void FlappyDies()
    {
        PanelAuth.SetActive(false);
        GameStateManager.GameState = GameState.Dead;
        DeathGUI.SetActive(true);
        LeaderboardPanel.SetActive(true);  
        ScoreManagerScript.Instance.SubmitScore();  // Enviar el puntaje antes de mostrar el leaderboard
        ScoreManagerScript.Instance.GetLeaderboard();
    }

    void RestartGame()
    {
        GameStateManager.GameState = GameState.Intro;
        Application.LoadLevel(Application.loadedLevelName);
        DeathGUI.SetActive(false);
        LeaderboardPanel.SetActive(false); 
        ScoreManagerScript.AddScore(-ScoreManagerScript.Score);
    }
}
