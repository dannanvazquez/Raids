using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;

public class Portal : NetworkBehaviour
{
    [SerializeField] private string teleportScene;
    [SerializeField] private GameObject readyPanel;
    [SerializeField] private Text readyTitle;
    [SerializeField] private Text readyStatus;
    [SerializeField] private RectTransform rectTimer;
    [SerializeField] private float readyTime = 30f;
    private float countdown = 0;
    private int readyAmount = 0;
    private GameObject[] players = null;

    [ServerCallback]
    private void Start() {
        countdown = readyTime;
    }

    [ServerCallback]
    private void Update() {
        if (!readyPanel.activeSelf) return;
        if (countdown <= 0) {
            RpcDeclineReady();
            readyPanel.SetActive(false);
            countdown = readyTime;
            readyAmount = 0;
        } else {
            countdown -= Time.deltaTime;
            RpcTimer(countdown, readyAmount);
        }
        if (readyAmount == players.Length) {
            //RpcTravel(teleportScene);
            FindObjectOfType<GameManager>().RpcNewScene(teleportScene);
        }
    }

    [ClientRpc]
    private void RpcTimer(float currentCountdown, int readyCount) {
        if (players == null) players = GameObject.FindGameObjectsWithTag("Player");
        readyStatus.text = $"{readyCount}/{players.Length} are ready to join.";
        rectTimer.sizeDelta = new Vector2 ((currentCountdown/readyTime) * 400f, rectTimer.sizeDelta.y);
    }

    [Command(requiresAuthority = false)]
    public void CmdAcceptReady() {
        readyAmount++;
    }

    [Command(requiresAuthority = false)]
    public void CmdDeclineReady() {
        RpcDeclineReady();
        countdown = readyTime;
        readyAmount = 0;
    }

    [ClientRpc]
    private void RpcDeclineReady() {
        readyPanel.SetActive(false);
    }

    [ClientRpc]
    private void RpcTravel(string sceneName) {
        //if (isClient) return;
        StartCoroutine(GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().LoadScene(sceneName));
        /*SceneManager.LoadSceneAsync("Level_1", LoadSceneMode.Additive);
        foreach (GameObject player in players) {
            SceneManager.MoveGameObjectToScene(player, SceneManager.GetSceneByName("Level_1"));
            player.transform.position = new Vector2(0, 0);
        }
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("SampleScene"));*/
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag != "Player" || readyPanel.activeSelf) return;
        players = GameObject.FindGameObjectsWithTag("Player");
        readyTitle.text = $"{collision.gameObject.GetComponent<PlayerController>().playerName} wants to travel.";
        readyStatus.text = $"0/{players.Length} are ready to join.";
        readyPanel.SetActive(true);
    }
}
