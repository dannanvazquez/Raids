using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnNameChanged))] public string playerName;
    [SerializeField] private GameObject playerBody;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private GameObject playerCamera;
    public Text usernameUI;
    private GameObject weaponText;
    private Rigidbody2D rb;
    private Animator anim;
    private float horizontal;
    private float vertical;
    private Quaternion aimDir;
    [SerializeField] private float movementSpeed = 20f;
    [SerializeField] private float shootCooldown = 1f;
    private float shootTimer = 0f;
    public bool isLocal = false;

    /*public override void OnStartClient() {
        OnNameChanged(connectionToClient.connectionId.ToString(), connectionToClient.connectionId.ToString());
    }*/

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = playerBody.GetComponent<Animator>();
        DontDestroyOnLoad(gameObject);
        //usernameUI.text = connectionToClient.connectionId.ToString();
        if (!isLocalPlayer) return;
        isLocal = true;
        Camera.main.gameObject.SetActive(false);
        playerCamera.SetActive(true);
        GameObject canvas = GameObject.Find("/Canvas");
        canvas.transform.Find("Panel-Tutorial").gameObject.SetActive(true);
        canvas.transform.Find("Panel-Inventory").gameObject.SetActive(true);
        weaponText = GameObject.Find("/Canvas/Panel-Inventory/Text-WeaponHolding");
        playerBody.GetComponent<SpriteRenderer>().sortingOrder += 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        Vector3 mousePos = playerCamera.GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition);
        float angleRad = Mathf.Atan2(mousePos.y - transform.position.y, mousePos.x - transform.position.x);
        float angleDeg = (180 / Mathf.PI) * angleRad;
        aimDir = Quaternion.Euler(0, 0, angleDeg);

        if (Input.GetButton("Fire1") && shootTimer <= 0 && anim.GetBool("hasGun")) {
            CmdShoot(aimDir);
            shootTimer = shootCooldown;
        }
        shootTimer -= Time.deltaTime;

        if(Input.GetKeyDown(KeyCode.Q)) {
            CmdDropWeapon();
            weaponText.GetComponent<Text>().text = "Weapon: None";
        }
    }

    public void OnNameChanged(string oldName, string newName) {
        usernameUI.text = newName;
    }

    [Command]
    void CmdShoot(Quaternion _aimDir) {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, _aimDir);
        NetworkServer.Spawn(bullet);
        RpcShoot(_aimDir);
    }

    [Command]
    void CmdDropWeapon() {
        RpcDropWeapon();
    }

    [ClientRpc]
    void RpcDropWeapon() {
        anim.SetBool("hasGun", false);
    }

    [Command]
    void CmdPickupWeapon(GameObject weapon) {
        RpcPickupWeapon(weapon);
    }

    [ClientRpc]
    void RpcPickupWeapon(GameObject weapon) {
        Destroy(weapon);
        anim.SetBool("hasGun", true);
    }

    [ClientRpc]
    void RpcShoot(Quaternion _aimDir) {
        //GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        //bullet.transform.rotation = _aimDir;
        //shootTimer = shootCooldown;
    }

    private void FixedUpdate() {
        if (!isLocalPlayer) return;
        if (horizontal != 0 && vertical != 0) {
            horizontal *= 0.7f;
            vertical *= 0.7f;
        }
        rb.velocity = new Vector2(horizontal * movementSpeed, vertical * movementSpeed);
        this.gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

        playerBody.transform.rotation = aimDir;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (!isLocalPlayer) return;
        if (collision.tag == "Gun") {
            CmdPickupWeapon(collision.gameObject);
            weaponText.GetComponent<Text>().text = "Weapon: Gun";
        }
    }
}
