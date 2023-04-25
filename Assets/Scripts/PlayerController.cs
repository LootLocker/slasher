using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float movementSpeed;
    private Rigidbody rb;

    public float rotationLerpSpeed;

    Vector3 movementVector = Vector3.zero;

    public Transform model;

    Animator playerAnimator;

    public Transform cameraTarget;

    public GameObject slashVFX;

    public ulong damage;

    public bool dead;

    bool levelEnded = false;

    public AudioSource audioSource;

    public WorldText popUpText;
    // Start is called before the first frame update
    void Start()
    {
        //health = PlayerPrefs.GetInt("PlayerHealth", PersistentData.instance.PlayerMaxHealth);
        damage = PlayerStats.instance.PlayerDamage;
        rb = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (dead == true || levelEnded == true)
        {
            return;
        }
        Move();
        Rotate();
        Attack();
    }

    private void Rotate()
    {
        if (movementVector.magnitude > 0.1f)
        {
            model.transform.rotation = Quaternion.Lerp(model.transform.rotation, Quaternion.LookRotation(movementVector), rotationLerpSpeed * Time.deltaTime);
        }
    }

    private void Attack()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            playerAnimator.SetTrigger("attack");
            Instantiate(slashVFX, transform.position, model.transform.rotation, transform);
        }
    }

    private void Move()
    {
        movementVector.x = Input.GetAxisRaw("Horizontal");
        movementVector.z = Input.GetAxisRaw("Vertical");
        movementVector = movementVector.normalized;
        playerAnimator.SetFloat("speed", movementVector.sqrMagnitude);
        if (movementVector.sqrMagnitude > 0.1f)
        {
            audioSource.volume = Mathf.Lerp(audioSource.volume, 1f, Time.deltaTime * 18f);
        }
        else
        {
            audioSource.volume = Mathf.Lerp(audioSource.volume, 0f, Time.deltaTime * 18f);
        }
    }

    private void FixedUpdate()
    {
        if (dead == true || levelEnded == true)
        {
            return;
        }
        if (playerAnimator.GetCurrentAnimatorStateInfo(0).IsName("PlayerAttack") == false)
        {
            rb.AddForce(movementVector * movementSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (dead == true)
        {
            return;
        }
        // End the current level
        if (other.gameObject.CompareTag("EndBlock"))
        {
            Debug.Log("Next level");
            levelEnded = true;
            GameController.instance.NextLevel();
        }
        else if(other.gameObject.CompareTag("EnemySwordCollision"))
        {
            // Take damage
            ulong enemyDamage = other.gameObject.GetComponentInParent<EnemyAI>().attack;
            PlayerStats.instance.TakeDamage(enemyDamage);
            Instantiate(popUpText, transform.position + Vector3.up*2f, Quaternion.identity).SetText("-" + enemyDamage.ToString(), Color.red);
            GetComponent<SoundEffect>().PlaySound(0);
            if (PlayerStats.instance.PlayerHealth <= 0)
            {
                dead = true;
                Debug.Log("You Lose!");
                movementSpeed = 0f;
                playerAnimator.SetTrigger("dead");
                PlayerStats.instance.UpdateHealth();
                GameController.instance.SwitchGameState(GameController.Menu.GameOver, false);
            }
        }
    }

    public void LevelUpPopUp()
    {
        Instantiate(popUpText, transform.position + Vector3.up * 2f, Quaternion.identity).SetText("Level Up!", Color.blue, 5f);
    }

}
