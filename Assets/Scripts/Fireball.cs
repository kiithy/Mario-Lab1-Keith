using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    public GameObject player;
    public float minSpeed = 3f;
    public float maxSpeed = 7f;
    // Start is called before the first frame update
    void Start()
    {
        float minHeight = player.transform.position.y;
        float maxHeight = transform.position.y;

        Vector2 direction = player.transform.position - transform.position;
        float speed = Random.Range(minSpeed, maxSpeed);
        GetComponent<Rigidbody2D>().velocity = direction.normalized * speed;

        // Set a random height within this range
        float randomHeight = Random.Range(minHeight, maxHeight);
        transform.position = new Vector3(transform.position.x, randomHeight, transform.position.z);

        Destroy(gameObject, 10);
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(this.gameObject);
    }
}
