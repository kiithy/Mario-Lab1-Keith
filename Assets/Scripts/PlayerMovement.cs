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
    public TextMeshProUGUI bowserHealthGameOverText;

    [System.NonSerialized]
    public int marioHealthScore = 3; // we don't want this to show up in the inspector

    [System.NonSerialized]
    public int bowserHealthScore = 5;
    public Canvas gameWinScreen;
    public Canvas gameOverScreen;
    public Canvas hurtScreen;
    public Button startButton;
    public Animator marioAnimator;
    public Animator castleAnimator;
    public Transform gameCamera;
    public AudioSource marioAudio;
    public AudioClip marioDamageAudio;
    public AudioClip shootFireballAudio;

    void PlayJumpSound()
    {
        marioAudio.PlayOneShot(marioAudio.clip);
        Debug.Log("Jump Sound Played");
    }


    public float deathImpulse = 15;
    public AudioClip marioDeath;
    [System.NonSerialized]
    public bool alive = true;


    public void RestartButtonCallback(int input)
    {
        Debug.Log("Restarting the game");
        ResetGame();
        Time.timeScale = 1.0f;
    }

    private void ResetGame()
    {
        StopAllCoroutines();
        marioSprite.color = Color.white;
        castleAnimator.SetBool("castleDamaged", false);
        marioAnimator.SetTrigger("gameRestart");
        alive = true;
        hurtScreen.gameObject.SetActive(false);
        gameOverScreen.gameObject.SetActive(false);
        gameWinScreen.gameObject.SetActive(false);
        marioBody.velocity = Vector2.zero;
        marioBody.position = new Vector2(-17.15f, -1.91f);
        marioBody.rotation = 0;
        bowser.GetComponent<Bowser>().bowserBody.rotation = 0;
        onGroundState = true;
        marioAnimator.SetBool("onGround", onGroundState);
        toppledState = false;
        readyToAttack = false;
        marioAnimator.SetBool("fireballGrabbed", readyToAttack);
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
        gameCamera.position = new Vector3(1.96f, 2.03f, - 10);
    }


    // Start is called before the first frame update
    void Start()
    {
        castleAnimator.SetBool("castleDamaged", false);
        hurtScreen.gameObject.SetActive(false);
        gameOverScreen.gameObject.SetActive(false);
        gameWinScreen.gameObject.SetActive(false);
        // Set to be 30 FPS
        Application.targetFrameRate = 30;
        marioBody = GetComponent<Rigidbody2D>();

        marioSprite = GetComponent<SpriteRenderer>();
        Time.timeScale = 0.0f;

        marioAnimator.SetBool("onGround", onGroundState);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Ground") || col.gameObject.CompareTag("Obstacle"))
        {
            onGroundState = true;
            marioAnimator.SetBool("onGround", onGroundState);
        }

    }

    void PlayDeathImpulse()
    {
        marioBody.AddForce(Vector2.up * deathImpulse, ForceMode2D.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("a") && faceRightState)
        {
            faceRightState = false;
            marioSprite.flipX = true;
            if (marioBody.velocity.x > 0.1f)
                marioAnimator.SetTrigger("onSkid");
        }

        if (Input.GetKeyDown("d") && !faceRightState)
        {
            faceRightState = true;
            marioSprite.flipX = false;
            if (marioBody.velocity.x < -0.1f)
                marioAnimator.SetTrigger("onSkid");
        }

        marioAnimator.SetFloat("xSpeed", Mathf.Abs(marioBody.velocity.x));

        // if (!onGroundState && !readyToAttack && !damageLock)
        // {
        //     marioSprite.sprite = marioJumpSprite;
        // }
        // else if (!readyToAttack && !onGroundState && damageLock)
        // {
        //     marioSprite.sprite = damagedMarioJumpSprite;
        // }
        // else if (!readyToAttack && onGroundState && damageLock)
        // {
        //     marioSprite.sprite = damagedMarioGroundSprite;
        // }
        // else if (onGroundState && !readyToAttack)
        // {
        //     marioSprite.sprite = marioGroundSprite;
        // }

        float rotationZ = marioBody.rotation;
        if (Mathf.Abs(rotationZ) > toppleThreshold)
        {
            toppledState = true;
            if (Input.GetKeyDown("space") && toppledState)
            {
                GetUpMario();
            }
        }
        if (readyToAttack && onGroundState && !damageLock)
        {
            marioSprite.sprite = marioGroundSprite;
            // TODO: Get Damaged
            marioAudio.PlayOneShot(marioDamageAudio);
            marioHealthScore--;
            marioHealthText.text = "Health: " + marioHealthScore.ToString();
            readyToAttack = false;
            marioAnimator.SetBool("fireballGrabbed", readyToAttack);
            damageLock = true;
            // marioBody.rotation = 45;
            // toppledState = true;
            StartCoroutine(DamageLock());
        }
        else if (readyToAttack && !onGroundState)
        {
            marioSprite.sprite = attackMarioJumpSprite;
        }

        if (marioHealthScore <= 0)
        {
            // REVIEW - Game Over Logic
            Debug.Log("Game Over");
            Time.timeScale = 0.0f;
            bowserHealthGameOverText.text = "Bowser's Health: " + bowserHealthScore.ToString();
            gameOverScreen.gameObject.SetActive(true);
            marioAnimator.Play("mario-die");
            //FIXME - Play Death Sound sounds super weird
            // marioAudio.PlayOneShot(marioDeath);
            alive = false;
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
        marioSprite.color = new Color(1, 0.5f, 0.5f); // Slight tinge of red
        yield return new WaitForSeconds(2);
        hurtScreen.gameObject.SetActive(false);
        marioSprite.color = Color.white; // Reset to normal color
        damageLock = false;
        Debug.Log("Damage Lock Released");
    }

    // FixedUpdate is called 50 times a second
    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs(moveHorizontal) > 0)
        {
            Vector2 movement = new Vector2(moveHorizontal, 0);
            // check if it doesn't go beyond maxSpeed
            if (marioBody.velocity.magnitude < maxSpeed)
                marioBody.AddForce(movement * speed);
        }

        // stop
        if (Input.GetKeyUp("a") || Input.GetKeyUp("d"))
        {
            // stop
            marioBody.velocity = Vector2.zero;
        }

        if (Input.GetKeyDown("space") && onGroundState && !toppledState)
        {
            marioBody.AddForce(Vector2.up * upSpeed, ForceMode2D.Impulse);
            onGroundState = false;

            marioAnimator.SetBool("onGround", onGroundState);
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

        if (readyToAttack)
        {
            hurtScreen.gameObject.SetActive(false);

        }
        }

        IEnumerator WinGame()
        {
        yield return new WaitForSeconds(0.5f);
        Time.timeScale = 0.0f;
        Debug.Log("You Win");
    }

    IEnumerator MarioFireBall()
    {
        GameObject fireball = Instantiate(marioFireball, transform.position, Quaternion.identity);
        readyToAttack = false;
        marioAnimator.SetBool("fireballGrabbed", readyToAttack);

        while (fireball != null)
        {
            GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");
            if (enemy != null)
            {
                Vector2 direction = (enemy.transform.position - fireball.transform.position).normalized;
                fireball.GetComponent<Rigidbody2D>().velocity = direction * speed;
                marioAudio.PlayOneShot(shootFireballAudio);
                // REVIEW - Castle Damaged
                bowserHealthScore--;
                yield return new WaitForSeconds(0.5f);
                bowserHealthText.text = "Bowser's Health: " + bowserHealthScore.ToString();
                Destroy(fireball);
                castleAnimator.SetBool("castleDamaged", true);
                yield return new WaitForSeconds(1f);
                castleAnimator.SetBool("castleDamaged", false);

            }
            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Fireball") && onGroundState && !damageLock)
        {
            Debug.Log("Damaged by Fireball");
            //TODO - Get Damaged
            marioAudio.PlayOneShot(marioDamageAudio);
            marioHealthScore--;
            marioHealthText.text = "Health: " + marioHealthScore.ToString();
            // toppledState = true;
            // marioBody.rotation = 45;
            readyToAttack = false;
            marioAnimator.SetBool("fireballGrabbed", readyToAttack);
            damageLock = true;
            StartCoroutine(DamageLock());

        }
        else if (other.gameObject.CompareTag("Fireball") && !onGroundState && readyToAttack && !damageLock)
        {
            // TODO - Get Damaged
            marioAudio.PlayOneShot(marioDamageAudio);
            marioHealthScore--;
            marioHealthText.text = "Health: " + marioHealthScore.ToString();
            // toppledState = true;
            // marioBody.rotation = 45;
            readyToAttack = false;
            marioAnimator.SetBool("fireballGrabbed", readyToAttack);
            damageLock = true;
            StartCoroutine(DamageLock());
        }
        else if (other.gameObject.CompareTag("Fireball") && !onGroundState && !readyToAttack)
        {
            readyToAttack = true;
            marioAnimator.SetBool("fireballGrabbed", readyToAttack);
            Destroy(other.gameObject);
        }
        // else if (other.gameObject.CompareTag("Fireball") && !onGroundState && readyToAttack)
        // {
        //     //REVIEW - Get Damaged
        //     marioHealthScore--;
        //     marioHealthText.text = "Health: " + marioHealthScore.ToString();
        //     readyToAttack = false;
        //     toppledState = true;
        //     marioBody.rotation = 45;
        //     damageLock = true;
        //     StartCoroutine(DamageLock());
        // }
    }

    void GetUpMario()
    {
        marioBody.AddForce(Vector2.up, ForceMode2D.Impulse);
        marioBody.rotation = 0;
        toppledState = false;
    }

    // SECTION - Sounds





}
