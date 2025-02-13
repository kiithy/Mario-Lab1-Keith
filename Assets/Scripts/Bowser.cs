using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bowser : MonoBehaviour
{

    public GameObject fireball;
    public float fireballInterval = 1.0f;
    public float timeToStartFiring = 2.0f;
    public Rigidbody2D bowserBody;
    public AudioSource bowserAudio;

    // Start is called before the first frame update
    void Start()
    {
        InitializeBowser();
    }

    public void InitializeBowser()
    {
        StopAllCoroutines(); // Stop any existing coroutines
        StartCoroutine(ShootFireball(new WaitForSeconds(fireballInterval)));
    }




    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator ShootFireball(WaitForSeconds wait)
    {
        while (true)
        {
            Instantiate(fireball, transform.position, Quaternion.identity);
            yield return wait;
        }
    }
}
