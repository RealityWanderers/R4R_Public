using UnityEngine;
using DG.Tweening; 

public class RamenBox : MonoBehaviour
{
    public float despawnDelay = 3f;
    public ParticleSystem despawnParticle; 
    private bool despawnStarted; 
    private Rigidbody rb;
    private bool homing = false;
    private Transform homingTarget;
    private float homingStrength;
    private System.Action onArrivalCallback;
    private bool isThrown; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Launch(Vector3 start, Vector3 initialVelocity, Transform target = null, float homingPower = 0f, System.Action onArrival = null)
    {
        transform.position = start;
        homingTarget = target;
        homingStrength = homingPower;
        onArrivalCallback = onArrival;
        homing = target != null;
        isThrown = true; 

        rb.isKinematic = false;
        rb.linearVelocity = initialVelocity;
    }

    private void FixedUpdate()
    {
        if (homing && homingTarget != null && homingTarget.gameObject.active)
        {
            Vector3 toTarget = (homingTarget.position - transform.position).normalized;
            Vector3 newVelocity = Vector3.Lerp(rb.linearVelocity, toTarget * rb.linearVelocity.magnitude, homingStrength * Time.fixedDeltaTime);
            rb.linearVelocity = newVelocity;
        }
        else if (!despawnStarted && isThrown) //If a box is thrown, but no longer homing as the original target is completed and gone. 
        {
            homing = false;
            DespawnAfterDelay(despawnDelay); 
        }

        if (!despawnStarted && isThrown) //Failsafe If a box is thrown, but perhaps still homing while stuck on the ground or something.
        {
            DespawnAfterDelay(despawnDelay * 3); 
        }
    }

    private Sequence sequence_Despawn; 
    private void DespawnAfterDelay(float delay)
    {
        despawnStarted = true; 

        sequence_Despawn?.Kill();
        sequence_Despawn = DOTween.Sequence();
        sequence_Despawn
            .AppendInterval(delay - 0.4f)
            .Append(transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InExpo));

        Destroy(gameObject, delay); //Keeping the destroy seperate from the tween here on purpose to ensure it always works even if there is something with the tween.
    }

    private void OnTriggerEnter(Collider other)
    {
        if (homing && homingTarget != null && other.transform == homingTarget)
        {
            onArrivalCallback?.Invoke();
            Destroy(gameObject); // Or return to pool!
        }
    }
}