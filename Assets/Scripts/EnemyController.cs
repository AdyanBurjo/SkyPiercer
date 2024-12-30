using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Components")]
    public Transform playerTransform;
    private Rigidbody enemyRigidbody;

    [Header("Physics")]
    [Tooltip("Force to push enemy forwards with")] 
    public float thrust = 50f; // Dikurangi dari 100f menjadi 50f
    [Tooltip("Pitch, Yaw, Roll")] 
    public Vector3 turnTorque = new Vector3(90f, 25f, 45f);
    [Tooltip("Multiplier for all forces")] 
    public float forceMult = 500f; // Dikurangi dari 1000f menjadi 500f

    [Header("Autopilot")]
    [Tooltip("Sensitivity for autopilot flight.")] 
    public float sensitivity = 5f;
    [Tooltip("Angle at which enemy banks fully into target.")] 
    public float aggressiveTurnAngle = 10f;

    private float yaw;
    private float pitch;
    private float roll;
    public int damage = 200;

    private void Awake()
    {
        enemyRigidbody = GetComponent<Rigidbody>();
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        if (enemyRigidbody == null)
            Debug.LogError("EnemyController requires a Rigidbody component.");
    }

    private void FixedUpdate()
    {
        if (playerTransform == null) return;

        Vector3 targetPosition = playerTransform.position;
        RunAutopilot(targetPosition, out yaw, out pitch, out roll);

        enemyRigidbody.AddRelativeForce(Vector3.forward * thrust * forceMult, ForceMode.Force);
        enemyRigidbody.AddRelativeTorque(new Vector3(turnTorque.x * pitch,
                                                     turnTorque.y * yaw,
                                                     -turnTorque.z * roll) * forceMult,
                                         ForceMode.Force);
    }

    private void RunAutopilot(Vector3 flyTarget, out float yaw, out float pitch, out float roll)
    {
        var localFlyTarget = transform.InverseTransformPoint(flyTarget).normalized * sensitivity;
        var angleOffTarget = Vector3.Angle(transform.forward, flyTarget - transform.position);

        yaw = Mathf.Clamp(localFlyTarget.x, -1f, 1f);
        pitch = -Mathf.Clamp(localFlyTarget.y, -1f, 1f);

        var agressiveRoll = Mathf.Clamp(localFlyTarget.x, -1f, 1f);

        var wingsLevelRoll = transform.right.y;

        var wingsLevelInfluence = Mathf.InverseLerp(0f, aggressiveTurnAngle, angleOffTarget);
        roll = Mathf.Lerp(wingsLevelRoll, agressiveRoll, wingsLevelInfluence);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Periksa apakah objek yang ditabrak adalah player
        if (collision.gameObject.CompareTag("Player"))
        {
            Health playerHealth = collision.gameObject.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage); // Kurangi HP player
            }
        }
    }
}
