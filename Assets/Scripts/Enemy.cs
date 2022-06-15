using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Enemy : NetworkBehaviour
{
    [SerializeField] private float aggroRange = 5f;
    [SerializeField] private float speed = 1f;

    [ServerCallback]
    private void Update() {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, aggroRange);
        Collider2D closestPlayer = null;
        foreach(var hitCollider in hitColliders) {
            if (hitCollider.tag == "Player" && (closestPlayer == null || Vector2.Distance(hitCollider.transform.position, transform.position) < Vector2.Distance(closestPlayer.transform.position, transform.position))) {
                closestPlayer = hitCollider;
            }
        }
        if (closestPlayer != null) {
            float step = speed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, closestPlayer.transform.position, step);

            Vector3 target = closestPlayer.transform.position;
            target.z = 0f;

            target.x -= transform.position.x;
            target.y -= transform.position.y;
            float angle = Mathf.Atan2(target.y, target.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Player") {
            Destroy(collision.gameObject);
        }
    }
}
