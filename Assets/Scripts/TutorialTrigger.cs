using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField] private string textInfo;
    private Text tutorialText;
    [SerializeField] private bool doDestroyThis;

    private void Start() {
        GameObject canvas = GameObject.Find("/Canvas");
        tutorialText = canvas.transform.Find("Panel-Tutorial").transform.Find("Panel-Body").transform.Find("Text-Info").GetComponent<Text>();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.tag != "Player") return;
        if (collision.gameObject.GetComponent<PlayerController>().isLocal == false) return;
        tutorialText.text = textInfo;
        if (doDestroyThis) Destroy(gameObject);
    }
}
