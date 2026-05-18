using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public string player1Name;
    public string player2Name; 

    public DateTime player1FinishTime;
    public DateTime player2FinishTime;

    public bool isplayer1Won = true; 
   public static GameManager Instance { get; private set; }
   public GameObject ReadyOrNotText1;
   public GameObject ReadyOrNotText2;
    public GameObject HintPanel;


   private void OnEnable()
    {
        // Unity'nin sahne yükleme event'ine kendi fonksiyonumuzu abone ediyoruz
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Hafıza sızıntısını (Memory Leak) önlemek için abonelikten çıkıyoruz
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GirisScene")
        {
            if (!isplayer1Won)
            {
                GameObject.Find("OncekiOyunOyuncu1").GetComponent<TMPro.TMP_Text>().text ="1. " + player1Name;
                GameObject.Find("OncekiOyunOyuncu2").GetComponent<TMPro.TMP_Text>().text = "2. "+ player2Name;
            }
            else
            {
                GameObject.Find("OncekiOyunOyuncu1").GetComponent<TMPro.TMP_Text>().text = "1. " + player2Name;
                GameObject.Find("OncekiOyunOyuncu2").GetComponent<TMPro.TMP_Text>().text = "2. " + player1Name;
            }

            if (HintPanel == null)
            {
                HintPanel = GameObject.Find("HintPanel");
            }

            
        
        }

        if (scene.name == "GameScene")
        {
            GameObject.Find("Player1Text").GetComponent<TMPro.TMP_Text>().text = player1Name;
            GameObject.Find("Player2Text").GetComponent<TMPro.TMP_Text>().text = player2Name;
        }
    }

    public void Update()
    {
       if (SceneManager.GetActiveScene().name == "GirisScene")
        {
             if (ReadyOrNotText1 == null)
        {
            ReadyOrNotText1 = GameObject.Find("ReadyOrNotText1");

        }
        if (ReadyOrNotText2 == null)
        {
            ReadyOrNotText2 = GameObject.Find("ReadyOrNotText2");
        }

             if (Input.GetKeyDown(KeyCode.Space))
        {
            ReadyOrNotText1.GetComponent<TMPro.TMP_Text>().text = "Hazır!";
            ReadyOrNotText1.GetComponent<TMPro.TMP_Text>().color = Color.green;
            if (ReadyOrNotText1.GetComponent<TMPro.TMP_Text>().text == "Hazır!" && ReadyOrNotText2.GetComponent<TMPro.TMP_Text>().text == "Hazır!")
            {
                GetPlayerNames();
                LoadScene("GameScene");
            }
        }

    if (Input.GetKeyDown(KeyCode.Return))
        {
            ReadyOrNotText2.GetComponent<TMPro.TMP_Text>().text = "Hazır!";
            ReadyOrNotText2.GetComponent<TMPro.TMP_Text>().color = Color.green;
            if (ReadyOrNotText1.GetComponent<TMPro.TMP_Text>().text == "Hazır!" && ReadyOrNotText2.GetComponent<TMPro.TMP_Text>().text == "Hazır!")
            {
                GetPlayerNames();
                LoadScene("GameScene");
            }
        }

    if (Input.GetKeyDown(KeyCode.H))
        {
            if (HintPanel != null)
            {
                HintPanel.SetActive(!HintPanel.activeSelf);
            }

        }
        }}
    private void Awake()
    {
       
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

     public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    } 

    public void GetPlayerNames()
    {
        player1Name = GameObject.Find("Oyuncu1InputField").GetComponent<TMPro.TMP_InputField>().text;
        player2Name = GameObject.Find("Oyuncu2InputField").GetComponent<TMPro.TMP_InputField>().text;
    }

   

}
