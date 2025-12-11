using Sirenix.OdinInspector;
using UnityEngine;

public class Enemy : ResettableObject
{
    [Header("Settings")]
    public int startHealth = 3; 
    public float respawnTime = 5f;
    [Header("Data")]
    [ReadOnly] public bool isDead = false;  
    [ReadOnly] public int currentHealth = 3;
    [Header("Refs")]
    private Renderer enemyRenderer;
    private Collider enemyCollider; 

    public void Start()
    {
        currentHealth = startHealth;
        enemyRenderer = GetComponentInChildren<Renderer>();
        enemyCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        // Example: When health reaches 0, disable the enemy
        if (currentHealth <= 0 && !isDead)
        {
            //DisableObject();  // Disable the enemy (simulating death)
            isDead = true;     // Mark as dead
            Invoke(nameof(RespawnEnemy), respawnTime);  // Call RespawnEnemy after a delay
            ToggleState(false); 
        }
    }

    private void RespawnEnemy()
    {
        // Reset the object (i.e., bring it back to life)
        //ResetObject();
        isDead = false;  // Mark the enemy as alive again
        currentHealth = startHealth;      // Reset health or any other attributes
        ToggleState(true);
    }

    private void ToggleState(bool isAlive)
    {
        if (enemyRenderer != null) { enemyRenderer.enabled = isAlive; }
        if (enemyCollider != null) { enemyCollider.enabled = isAlive; }
    }

    [Button]
    // Example of a method that could handle damage
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
    }
}


