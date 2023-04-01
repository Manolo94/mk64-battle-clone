using UnityEngine;
using UnityEngine.Events;

public class GreenShell : MonoBehaviour
{
    public float initialVelocity = 0.0f;
    public float distanceToFloor = 0.2f;
    public float shellGravity = 1f; // Fake gravity on shells

    public UnityEvent<GameObject> OnProjectileDestroyed;

    private Rigidbody shellBody;
    private Vector3 shellVelocity = Vector3.zero;
    private Vector3 velocityFromGravity = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        shellBody = GetComponent<Rigidbody>();
        shellVelocity = initialVelocity * transform.forward;
    }

    private void OnDestroy()
    {
        if (OnProjectileDestroyed != null) OnProjectileDestroyed.Invoke(gameObject);
    }

    private void FixedUpdate()
    {
        // Update velocity
        shellBody.velocity = shellVelocity + velocityFromGravity;

        // Hover over the floor
        RaycastHit ray;

        if (Physics.Raycast(transform.position, -transform.up, out ray, distanceToFloor))
        {
            shellBody.position = ray.point + distanceToFloor * transform.up;
            Debug.DrawRay(ray.point, shellVelocity, Color.cyan);

            velocityFromGravity = Vector3.zero;
        }
        else
        {
            velocityFromGravity -= Vector3.up * shellGravity * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            // Shoot it up a bit
            var hitVelocity = shellVelocity.normalized + Vector3.up;
            hitVelocity *= shellVelocity.magnitude;

            PlayerDamage playerDamage = other.GetComponent<PlayerDamage>();
            if(playerDamage != null) playerDamage.PlayerShotByShell(hitVelocity, transform.position);

            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Destroy each other if it collides with a projectile
        if(collision.gameObject.CompareTag("Projectile"))
        {
            Destroy(gameObject);
            Destroy(collision.gameObject);

            return;
        }

        // Use shellVelocity and not shellBody.velocity. It seems that unity modifies the
        //  shellBody.velocity prematurely trying to resolve the collision

        // reflect velocity
        // r = d - 2(d.n)n  => r - new reflected vector, d - original vector, n - normal of the surface
        Vector3 d = shellVelocity;
        Vector3 n = collision.contacts[0].normal;

        n.y = 0; d.y = 0; // Remove vertical component, to avoid flying in the air when going up ramps

        // normalize so that the magnitude of the reflected velocity is always the same
        // don't normalize the normal (n), that way hits that are mostly vertical are mostly ignored
        d.Normalize();

        // Negate the dot product, since the resulting velocity should negate the incoming one
        // perfect collision for green shells, multiply by shellVelocity, since d and n were normalized
        Vector3 r = n * 2 * -Vector3.Dot(d, n) * shellVelocity.magnitude;

        Debug.DrawRay(collision.contacts[0].point, n, Color.red, 10.0f);

        shellVelocity += r;

        Debug.DrawRay(collision.contacts[0].point, shellVelocity, Color.black, 3.0f);
    }
}
