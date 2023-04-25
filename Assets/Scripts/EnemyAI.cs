using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    public GameObject player;
    public float movementSpeed;

    public int health;

    public ulong attack;

    public int level;
    private Rigidbody rb;
    private Animator enemyAnimator;

    Vector3 movementVector = Vector3.zero;

    public Transform model;

    public float rotationLerpSpeed;

    Quaternion lookRotationQuaternion;

    Coroutine hitRoutine;

    public List<Material> originalMaterials = new List<Material>();

    public Material hitMaterial;

    public MeshRenderer[] renderers;

    public bool canSeePlayer;

    public LayerMask playerLayerMask;

    public AnimationCurve partCurve;

    public float currentMovementSpeed;

    public bool attacking;

    public GameObject slashVFX;

    public GameObject swordCollider;

    Coroutine attackRoutine;

    public Image healthBar;

    public WorldText popUpText;

    public bool dead = false;

    // Start is called before the first frame update
    public void Initialize(GameObject newPlayer)
    {
        player = newPlayer;
        level = PersistentData.instance.DungeonLevel;
        currentMovementSpeed = movementSpeed;
        rb = GetComponent<Rigidbody>();
        enemyAnimator = GetComponent<Animator>();
        renderers = GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials.Add(renderers[i].material);
        }
        movementVector = Vector3.zero;
        SetHealth();
    }

    // Calculate the health value based on the level
    void SetHealth()
    {
        // (10 + (playerLevel * playerLevel * 0.8f) * 10);
        health = (int)((int)10 + (level * level * 0.9f)*3);
    }

    // Update is called once per frame
    void Update()
    {
        if (dead)
        {
            return;
        }
        Move();
        Rotate();
        Attack();
    }

    private void Rotate()
    {
        if (canSeePlayer)
        {
            // Rotate towards player
            lookRotationQuaternion = Quaternion.LookRotation(player.transform.position - transform.position);
        }

        if (movementVector.magnitude > 0.1f)
        {
            model.transform.rotation = Quaternion.Lerp(model.transform.rotation, lookRotationQuaternion, rotationLerpSpeed * Time.deltaTime);
        }
    }

    private void Attack()
    {
        // If distance to player is lower than threshold; attack
        if (Vector3.Distance(transform.position, player.transform.position) < 2f)
        {
            if (attacking == false)
            {
                attacking = true;
                attackRoutine = StartCoroutine(AttackCooldownRoutine());
            }
        }
    }

    IEnumerator AttackCooldownRoutine()
    {
        yield return new WaitForSeconds(0.25f);
        enemyAnimator.SetTrigger("attack");

        Instantiate(slashVFX, transform.position, model.transform.rotation, transform);
        yield return new WaitForSeconds(0.5f);
        attacking = false;
    }

    private void Move()
    {
        if (canSeePlayer && attacking == false)
        {
            movementVector = model.transform.forward;
            movementVector = movementVector.normalized;
        }
        else
        {
            movementVector = Vector3.Lerp(movementVector, Vector3.zero, Time.deltaTime * 12f);
        }
        enemyAnimator.SetFloat("speed", movementVector.sqrMagnitude);
    }

    private void FixedUpdate()
    {
        if (dead)
        {
            return;
        }
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, (Vector3.up + player.transform.position) - (transform.position + Vector3.up), out hit, 20f, playerLayerMask))
        {
            if (hit.collider.gameObject == player)
            {
                canSeePlayer = true;
            }
            else
            {
                canSeePlayer = false;
            }
        }

        if (enemyAnimator.GetCurrentAnimatorStateInfo(0).IsName("PlayerAttack") == false)
        {
            rb.AddForce(movementVector * movementSpeed);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        // If the player hits the enemy
        if (collider.transform.gameObject.CompareTag("SwordCollision"))
        {
            rb.AddForce(-movementVector * 50f, ForceMode.Impulse);
            ulong playerDamage = PlayerStats.instance.PlayerDamage;
            health -= (int)playerDamage;
            healthBar.fillAmount = (float)health / (10 + (level * 5));
            // Show damage text
            Instantiate(popUpText, transform.position + Vector3.up * 2f, Quaternion.identity).SetText("-" + playerDamage.ToString(), Color.white);

            // Stop hitRoutine if it isn't already running
            if (hitRoutine != null)
            {
                StopCoroutine(hitRoutine);
            }
            hitRoutine = StartCoroutine(HitRoutine());
            
            if (health <= 0)
            {
                if (dead == false)
                {
                    health = 0;
                    healthBar.transform.parent.gameObject.SetActive(false);
                    DeathEffect();
                    // Show XP received
                    Instantiate(popUpText, transform.position + Vector3.up * 2f, Quaternion.identity).SetText("+" + (level * 5) + " XP", Color.blue, 2.5f);
                    dead = true;
                }
            }
        }
    }

    public int GetMyXP()
    {
        return 10 + (level * 5);
    }

    void DeathEffect()
    {
        enemyAnimator.enabled = false;
        swordCollider.SetActive(false);
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
        }
        PlayerStats.instance.AddXP(GetMyXP());
        PlayerStats.instance.SlaySkeleton();
        GameObject deadSkeletonObject = new GameObject();
        GetComponent<CapsuleCollider>().enabled = false;
        Destroy(rb);
        for (int i = 0; i < renderers.Length; i++)
        {
            MeshRenderer renderer = renderers[i];
            MeshCollider meshCollider = renderer.gameObject.AddComponent<MeshCollider>();
            meshCollider.convex = true;
            renderer.gameObject.AddComponent<Rigidbody>();
            renderer.transform.parent = deadSkeletonObject.transform;
            renderer.transform.gameObject.layer = LayerMask.NameToLayer("DeadEnemy");
        }
        GetComponent<SoundEffect>().PlaySound(0);
        StartCoroutine(DestroySelfAndDisposeBodyRoutine(deadSkeletonObject));
    }


    IEnumerator DestroySelfAndDisposeBodyRoutine(GameObject deadBody)
    {
        yield return new WaitForSeconds(2f);
        float timer = 0f;
        float duration = 3f;

        Transform[] allParts = deadBody.GetComponentsInChildren<Transform>();
        for (int i = 0; i < allParts.Length; i++)
        {
            var currentPart = allParts[i];
            Destroy(currentPart.GetComponent<Rigidbody>());
            Destroy(currentPart.GetComponent<MeshCollider>());
        }
        while (timer <= duration)
        {
            for (int i = 0; i < allParts.Length; i++)
            {
                var currentPart = allParts[i];

                // Do not include parent object
                if (currentPart != deadBody.transform)
                {
                    currentPart.localScale = Vector3.Lerp(currentPart.transform.localScale, Vector3.zero, partCurve.Evaluate(timer / duration));
                }
            }
            timer += Time.deltaTime;
            yield return null;
        }

        Destroy(deadBody);
        Destroy(gameObject);
    }

    IEnumerator HitRoutine()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            MeshRenderer renderer = renderers[i];
            renderer.material = hitMaterial;
        }
        GetComponent<SoundEffect>().PlaySound(1);
        yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < renderers.Length; i++)
        {
            MeshRenderer renderer = renderers[i];
            renderer.material = originalMaterials[i];
        }
    }
}
