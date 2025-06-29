using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManagers : MonoBehaviour
{
    public static GameManagers Instance;
    public int ghostSoul = 3;
    public GameObject panel;

    public TMP_Text soulText, message;
    bool isPlaying = false;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
      
        
    }
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(ghostSoul == 0)
        {
            GameOver("Ghost Purified");
        }
    }

    public void StartGame()
    {
        if(!isPlaying)
        {
            panel.SetActive(false);
            Time.timeScale = 1.0f;
            isPlaying= true;
        }
        else
        {
            SceneManager.LoadScene("SampleScene");
        }


    }

    public void GameOver(string text)
    {
        panel.SetActive(true);
        message.text = text;
        Time.timeScale = 0.0f;
    }

    public void updateSoul()
    {
        ghostSoul -= 1;
        soulText.text = $"Ghost Soul : {ghostSoul}";
    }
}
