using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 10;
    private Rigidbody2D marioBody;
    public float maxSpeed = 20;
    public float upSpeed = 10;
    private bool onGroundState = true;
    private SpriteRenderer marioSprite;
    private bool faceRightState = true;
    public Sprite marioJumpSprite;
    public Sprite marioGroundSprite;
    public float toppleThreshold = 45;
    private bool toppledState = false;
    public Sprite attackMarioJumpSprite;
    public Sprite attackMarioGroundSprite;
    public Sprite damagedMarioJumpSprite;
    public Sprite damagedMarioGroundSprite;
    private bool readyToAttack = false;
    public GameObject marioFireball;
    public TextMeshProUGUI marioHealthText;
    public GameObject bowser;
    public Button restartButton;
    private bool damageLock = false;
    public TextMeshProUGUI bowserHealthText;

    [System.NonSerialized]
    public int marioHealthScore = 3; // we don't want this to show up in the inspector

    [System.NonSerialized]
    public int bowserHealthScore = 5;
    public Canvas gameWinScreen;
    public Canvas gameOverScreen;
    public Canvas hurtScreen;

    public void RestartButtonCallback (int input)
    {
        Debug.Log("Restarting the game");
        ResetGame();
        Time.timeScale = 1.0f;
    }

    private void ResetGame()
    {
        StopAllCoroutines();
        hurtScreen.gameObject.SetActive(false);
        gameOverScreen.gameObject.SetActive(false);
        gameWinScreen.gameObject.SetActive(false);
        marioBody.velocity = Vector2.zero;
        marioBody.position = new Vector2(-17.15f, -1.91f);
        marioBody.rotation = 0;
        bowser.GetComponent<Bowser>().bowserBody.rotation = 0;
        marioSprite.sprite = marioGroundSprite;
        onGroundState = true;
        faceRightState = true;
        toppledState = false;
        readyToAttack = false;
        damageLock = false;
        marioHealthScore = 3;
        marioHealthText.text = "Health: " + marioHealthScore.ToString();
        bowserHealthScore = 5;
        bowserHealthText.text = "Bowser's Health: " + bowserHealthScore.ToString();
        GameObject[] fireballs = GameObject.FindGameObjectsWithTag("Fireball");
        foreach (GameObject fireball in fireballs)
        {
            Destroy(fireball);
        }
        bowser.GetComponent<Bowser>().InitializeBowser();
    }


    // Start is called before the first frame update
    void Start()
    {
        hurtScreen.gameObject.SetActive(false);
        gameOverScreen.gameObject.SetActive(false);
        gameWinScreen.gameObject.SetActive(false);
        // Set to be 30 FPS
        Application.targetFrameRate =  30;
        marioBody = GetComponent<Rigidbody2D>();

        marioSprite = GetComponent<SpriteRenderer>();
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground")) onGroundState = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Toggle state
        if (Input.GetKeyDown("a") && faceRightState){
            faceRightState = false;
            marioSprite.flipX = true;
        }

        if (Input.GetKeyDown("d") && !faceRightState){
            faceRightState = true;
            marioSprite.flipX = false;
        }

        if (!onGroundState && !readyToAttack && !damageLock)
        {
            marioSprite.sprite = marioJumpSprite;
        }
        else if (!readyToAttack && !onGroundState && damageLock)
        {
            marioSprite.sprite = damagedMarioJumpSprite;
        }
        else if (!readyToAttack && onGroundState && damageLock)
        {
            marioSprite.sprite = damagedMarioGroundSprite;
        }
        else if (onGroundState && !readyToAttack)
        {
            marioSprite.sprite = marioGroundSprite;
        }

        float rotationZ = marioBody.rotation;
        if (Mathf.Abs(rotationZ) > toppleThreshold)
        {
            toppledState = true;
            if (Input.GetKeyDown("space") && toppledState){
                GetUpMario();
            }
        }
        if (readyToAttack && onGroundState && !damageLock)
        {
            marioSprite.sprite = marioGroundSprite;
            // TODO: Get Damaged
            marioHealthScore--;
            marioHealthText.text = "Health: " + marioHealthScore.ToString();
            readyToAttack = false;
            damageLock = true;
            // marioBody.rotation = 45;
            // toppledState = true;
            StartCoroutine(DamageLock());
        } else if (readyToAttack && !onGroundState)
        {
            marioSprite.sprite = attackMarioJumpSprite;
        }

        if (marioHealthScore <= 0)
        {
            // REVIEW - Game Over Logic
            Debug.Log("Game Over");
            Time.timeScale = 0.0f;
            gameOverScreen.gameObject.SetActive(true);


        }

        if (bowserHealthScore <= 0)
        {
            // REVIEW - Game Win Logic
            bowser.GetComponent<Bowser>().bowserBody.rotation = -45;
            StartCoroutine(WinGame());
            gameWinScreen.gameObject.SetActive(true);

        }

    }

    IEnumerator DamageLock()
    {
        hurtScreen.gameObject.SetActive(true);
        yield return new WaitForSeconds(2);
        hurtScreen.gameObject.SetActive(false);
        damageLock = false;
        Debug.Log("Damage Lock Released");
    }

    // FixedUpdate is called 50 times a second
    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs(moveHorizontal) > 0){
            Vector2 movement = new Vector2(moveHorizontal, 0);
            // check if it doesn't go beyond maxSpeed
            if (marioBody.velocity.magnitude < maxSpeed)
                    marioBody.AddForce(movement * speed);
        }

        // stop
        if (Input.GetKeyUp("a") || Input.GetKeyUp("d")){
            // stop
            marioBody.velocity = Vector2.zero;
        }

        if (Input.GetKeyDown("space") && onGroundState && !toppledState) {
            marioBody.AddForce(Vector2.up * upSpeed, ForceMode2D.Impulse);
            onGroundState = false;
        }

        if (readyToAttack && !onGroundState)
        {
            if (Input.GetKeyDown("space"))
            {
                StartCoroutine(MarioFireBall());
            }
            marioSprite.sprite = marioGroundSprite;
        }

        if (marioBody.rotation == 0 && !onGroundState)
        {
            toppledState = false;
        }

    }

    IEnumerator WinGame()
    {
        yield return new WaitForSeconds(1);
        Time.timeScale = 0.0f;
        Debug.Log("You Win");
    }

    IEnumerator MarioFireBall()
    {
        GameObject fireball = Instantiate(marioFireball, transform.position, Quaternion.identity);
        readyToAttack = false;

        while (fireball != null)
        {
            GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
            if (enemy != null)
            {
                Vector2 direction = (enemy.transform.position - fireball.transform.position).normalized;
                fireball.GetComponent<Rigidbody2D>().velocity = direction * speed;
                // REVIEW - Castle Damaged
                bowserHealthScore--;
                yield return new WaitForSeconds(0.5f);
                bowserHealthText.text = "Bowser's Health: " + bowserHealthScore.ToString();
                Destroy(fireball);

            }
            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Fireball") && onGroundState && !damageLock)
        {
            Debug.Log("Damaged by Fireball");
            //TODO - Get Damaged
            marioHealthScore--;
            marioHealthText.text = "Health: " + marioHealthScore.ToString();
            // toppledState = true;
            // marioBody.rotation = 45;
            damageLock = true;
            StartCoroutine(DamageLock());
        } else if (other.gameObject.CompareTag("Fireball") && !onGroundState && readyToAttack && !damageLock)
        {
            // TODO - Get Damaged
            marioHealthScore--;
            marioHealthText.text = "Health: " + marioHealthScore.ToString();
            // toppledState = true;
            // marioBody.rotation = 45;
            damageLock = true;
            StartCoroutine(DamageLock());
        }
        else if (other.gameObject.CompareTag("Fireball") && !onGroundState && !readyToAttack) {
            readyToAttack = true;
            marioSprite.sprite = attackMarioJumpSprite;
            Destroy(other.gameObject);
        } else if (other.gameObject.CompareTag("Fireball") && !onGroundState && readyToAttack)
        {
            //REVIEW - Get Damaged
            marioHealthScore--;
            marioHealthText.text = "Health: " + marioHealthScore.ToString();
            readyToAttack = false;
            toppledState = true;
            marioBody.rotation = 45;
            damageLock = true;
            StartCoroutine(DamageLock());
        }
    }

    void GetUpMario()
    {
        marioBody.AddForce(Vector2.up, ForceMode2D.Impulse);
        marioBody.rotation = 0;
        toppledState = false;
    }



}
