using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth = 100;
    [SerializeField] private RectTransform healthRect = null;
    [SerializeField] private bool isDummy = false;
    [SerializeField] private float outOfCombatCooldown = 5f;
    private float outOfCombatTimer = 0;
    [SerializeField] private int regenPerSec = 5;
    private bool isRegening = false;

    [ServerCallback]
    private void Start() {
        currentHealth = maxHealth;
    }

    [ServerCallback]
    private void Update() {
        if (outOfCombatTimer <= 0 && !isRegening) {
            isRegening = true;
            StartCoroutine(Regen());
        } else {
            outOfCombatTimer -= Time.deltaTime;
        }
    }

    IEnumerator Regen() {
        while(true) {
            if (outOfCombatTimer > 0) break;
            if (currentHealth + regenPerSec >= maxHealth) {
                currentHealth = maxHealth;
                RpcChangeHealth(1920);
                break;
            }
            currentHealth += regenPerSec;
            RpcChangeHealth(((float)currentHealth/(float)maxHealth) * 1920);
            yield return new WaitForSeconds(1f);
        }
        isRegening = false;
    }

    public void TakeDamage(int damage) {
        outOfCombatTimer = outOfCombatCooldown;
        if (isDummy && currentHealth - damage <= 5) {
            currentHealth = 5;
            return;
        }
        if (currentHealth - damage <= 0) {
            currentHealth = 0;
            Destroy(gameObject);
        } else {
            currentHealth -= damage;
            Debug.Log((currentHealth/maxHealth) * 1920);
            RpcChangeHealth(((float)currentHealth/(float)maxHealth) * 1920);
        }
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Bullet") {
            TakeDamage(collision.gameObject.GetComponent<Bullet>().damage);
            Destroy(collision.gameObject);
        }
    }

    [ClientRpc]
    private void RpcChangeHealth(float health) {
        healthRect.sizeDelta = new Vector2(health, healthRect.sizeDelta.y);
    }
}