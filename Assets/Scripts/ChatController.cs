using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Linq;

public class ChatController : NetworkBehaviour
{
    [SerializeField] private GameObject ChatUI = null;
    [SerializeField] private Text chatText = null;
    [SerializeField] private InputField inputField = null;
    [SerializeField] private GameObject content = null;
    [SerializeField] private float awakeTime;
    private float awakeCountdown = 0;

    private static event Action<string> OnMessage;

    [Client]
    private void Update() {
        if (!isLocalPlayer) return;
        if (ChatUI.activeSelf) {
            if (inputField.isFocused) {
                awakeCountdown = awakeTime;
            } else {
                awakeCountdown -= Time.deltaTime;
                if (awakeCountdown <= 0f) {
                    ChatUI.SetActive(false);
                }
            }
        } else {
            if (Input.GetKeyDown(KeyCode.Return)) {
                ChatUI.SetActive(true);
                awakeCountdown = awakeTime;
            }
        }
    }

    public override void OnStartAuthority() {
        OnMessage += HandleNewMessage;
        CmdJoinGame();
    }

    [ServerCallback]
    private void OnDestroy() {
        if (!hasAuthority) return;
        OnMessage -= HandleNewMessage;

    }

    private void HandleNewMessage(string message) {
        ChatUI.SetActive(true);
        awakeCountdown = awakeTime;
        chatText.text += message;
        RectTransform rect = content.GetComponent<RectTransform>();
        if (rect.sizeDelta.y > 500) return;
        rect.sizeDelta = new Vector2(rect.sizeDelta.x, rect.sizeDelta.y + 20);
    }

    [Command]
    public void CmdJoinGame() {
        RpcHandleMessage($"{gameObject.GetComponent<PlayerController>().playerName} has joined the game!");
    }


    [Client]
    public void Send() {
        if (!Input.GetKeyDown(KeyCode.Return)) return;
        if (string.IsNullOrWhiteSpace(inputField.text)) return;
        if (inputField.text[0] == '/') { //Checking if a command is being executed
            SendCommand(inputField.text);
        } else { //Otherwise, send as message
            CmdSendMessage(inputField.text);
        }
        inputField.text = string.Empty;
    }

    [Command(requiresAuthority = false)]
    private void CmdSendMessage(string message) {
        RpcHandleMessage($"[{gameObject.GetComponent<PlayerController>().playerName}]: {message}");
    }

    [Client]
    private void SendCommand(string commandMessage) {
        string command = commandMessage.Split(new char[] { ' ' })[0]; //Puts first word into command
        command = command.Remove(0, 1); //Removes the index of the command

        List<string> parameters = commandMessage.Split(' ').ToList(); //Splits each word into parameters
        parameters.RemoveAt(0); //Removes the command from the parameters

        string message = null;
        if (command == "ping") { //Ping command; Responds with pong
            message = "Pong!";
        } else if (command == "tp" || command == "teleport") { //Teleport command; Teleports the player to a player or specific coords
            message = TeleportCommand(parameters);
        } else if (command == "coords") { //Coords command; Tells the player their coords
            message = $"Your coords are ({gameObject.transform.position.x.ToString("F2")}, {gameObject.transform.position.y.ToString("F2")})";
        } else if (command == "nick") { //Nick command; Changes the name of a player
            message = NickCommand(parameters);
        }

        if (message == null) {
            message = "Command cannot be found.";
        }

        OnMessage?.Invoke($"\n{message}");
    }

    [ClientRpc]
    private void RpcHandleMessage(string message) {
        OnMessage?.Invoke($"\n{message}");
    }

    private String TeleportCommand(List<string> parameters) {
        if (parameters.Count == 0) {
            return "Not enough parameters!";
        } else if (parameters.Count == 1) {
            try {
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                Vector2 tpLocation = Vector2.zero;
                foreach (GameObject player in players) {
                    if (player.GetComponent<PlayerController>().playerName == parameters[0]) {
                        tpLocation = player.transform.position;
                        break;
                    }
                }
                gameObject.transform.position = tpLocation;
                return $"You have been teleported to player {parameters[0]}!";
            } catch {
                return "Cannot find player to teleport to.";
            }
        } else if (parameters.Count == 2) {
            Vector2 tpLocation = new Vector2(float.Parse(parameters[0]), float.Parse(parameters[1]));
            gameObject.transform.position = tpLocation;
            return $"You have been teleported to ({tpLocation.x}, {tpLocation.y})!";
        } else {
            return "Too many parameters! \"/tp <player>\" or \"/tp <x> <y>\"";
        }
    }

    private String NickCommand(List<string> parameters) {
        if (parameters.Count == 0) {
            return "Not enough parameters! \"/nick (player) <new name>\"";
        } else if (parameters.Count == 1) {
            CmdChangeNick(gameObject.GetComponent<PlayerController>(), parameters[0]);
            return $"Your name has been changed to {parameters[0]}";
        } else if (parameters.Count == 2) {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            PlayerController playerChange = null;
            string pastName = null;
            foreach (GameObject player in players) {
                if (playerChange.playerName == parameters[0]) {
                    playerChange = player.GetComponent<PlayerController>();
                    pastName = playerChange.playerName;
                    break;
                }
            }
            CmdChangeNick(playerChange, parameters[1]);
            return $"{pastName}'s name has been changed to {parameters[1]}";
        } else {
            return "Too many parameters! \"/nick (player) <new name>\"";
        }
    }

    [Command]
    private void CmdChangeNick(PlayerController player, String name) {
        player.playerName = name;
    }

    [TargetRpc]
    private void TargetSendMessage(NetworkConnection target, string message) {
        OnMessage?.Invoke($"\n{message}");
    }
}
