using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public GameObject tutorialPanel;
    public Text tutorialText;

    private void Start() {
        DontDestroyOnLoad(gameObject);
    }

    [ClientRpc]
    public void RpcNewScene(string sceneName) {
        StartCoroutine(LoadScene(sceneName));
    }

    public IEnumerator LoadScene(string sceneName) {
        Debug.Log("LOADING!");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;
        while (asyncLoad.progress < 0.9f) {
            //Show progress here
            yield return null;
        }
        //Continue the progress bar after 90% here
        asyncLoad.allowSceneActivation = true;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players) {
            Debug.Log("Teleporting!");
            player.transform.position = new Vector2(0, 0);
        }
    }
}
