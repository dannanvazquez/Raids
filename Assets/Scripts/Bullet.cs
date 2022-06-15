using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float lifetime = 10f;
    public int damage = 10;

    void Update()
    {
        if (lifetime <= 0) Destroy(gameObject);

        transform.position  += transform.right * Time.deltaTime * speed;

        lifetime -= Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.tag == "Enemy") return;
        Destroy(gameObject);
    }
}
