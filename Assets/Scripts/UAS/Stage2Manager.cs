using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Stage2Manager : MonoBehaviour
{
    public static Stage2Manager Instance;
    public GameObject[] Angels;
    public float time;
    public Transform[] waypoint;
    public Grid gridRef;

    public int ghostSoul = 10;
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
        soulText.text = $"Ghost Soul : {ghostSoul}";
        Time.timeScale = 0.0f;
        foreach (GameObject angel in Angels)
        {
            angel.GetComponent<WeepingAngel>().wanderSpots = waypoint;
            angel.GetComponent<WeepingAngel>().gridRef = gridRef;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (ghostSoul == 0)
        {
            GameOver("Soul Collected, Escape Successfull");
        }
    }

    private IEnumerator WakeUpAngelRoutine()
    {
        List<GameObject> dormantAngels = Angels.ToList();

        while (dormantAngels.Count > 0)
        {
            yield return new WaitForSeconds(time);
            Debug.Log("WAKE UP");

            int randomIndex = Random.Range(0, dormantAngels.Count);
            GameObject chosenAngel = dormantAngels[randomIndex];

            WeepingAngel angelSM = chosenAngel.GetComponent<WeepingAngel>();
            if (angelSM != null)
            {
                angelSM.ChangeState(angelSM.wanderState); 
            }

            dormantAngels.RemoveAt(randomIndex);
        }
    }

    public void StartGame()
    {
        if (!isPlaying)
        {
            StartCoroutine(WakeUpAngelRoutine());
            panel.SetActive(false);
            Time.timeScale = 1.0f;
            isPlaying = true;

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
