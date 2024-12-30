using UnityEngine;

namespace MFlight.Demo
{
    [RequireComponent(typeof(Rigidbody))]
    public class Plane : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private MouseFlightController controller = null;
        [SerializeField] private GameObject rudalPrefab = null;
        [SerializeField] private Transform rudalSpawnPoint = null;

        [Header("Physics")]
        [Tooltip("Force to push plane forwards with")] public float thrust = 100f;
        [Tooltip("Pitch, Yaw, Roll")] public Vector3 turnTorque = new Vector3(90f, 25f, 45f);
        [Tooltip("Multiplier for all forces")] public float forceMult = 1000f;

        [Header("Rudal")]
        [Tooltip("Kecepatan Rudal")] public float kecepatanRudal = 100f;

        [Header("Autopilot")]
        [Tooltip("Sensitivity for autopilot flight.")] public float sensitivity = 5f;
        [Tooltip("Angle at which airplane banks fully into target.")] public float aggressiveTurnAngle = 10f;

        [Header("Turbo")]
        [Tooltip("Multiplier for turbo speed")] public float turboMultiplier = 2f;
        private bool turboActive = false;

        [Header("Input")]
        [SerializeField][Range(-1f, 1f)] private float pitch = 0f;
        [SerializeField][Range(-1f, 1f)] private float yaw = 0f;
        [SerializeField][Range(-1f, 1f)] private float roll = 0f;

        public float Pitch { set { pitch = Mathf.Clamp(value, -1f, 1f); } get { return pitch; } }
        public float Yaw { set { yaw = Mathf.Clamp(value, -1f, 1f); } get { return yaw; } }
        public float Roll { set { roll = Mathf.Clamp(value, -1f, 1f); } get { return roll; } }

        private Rigidbody rigid;

        private bool rollOverride = false;
        private bool pitchOverride = false;

        private void Awake()
        {
            rigid = GetComponent<Rigidbody>();

            if (controller == null)
                Debug.LogError(name + ": Plane - Missing reference to MouseFlightController!");

            if (rudalSpawnPoint == null)
                Debug.LogError(name + ": Plane - Missing missile spawn point!");
        }

        private void Update()
        {
            rollOverride = false;
            pitchOverride = false;

            float keyboardRoll = Input.GetAxis("Horizontal");
            if (Mathf.Abs(keyboardRoll) > .25f)
            {
                rollOverride = true;
            }

            float keyboardPitch = Input.GetAxis("Vertical");
            if (Mathf.Abs(keyboardPitch) > .25f)
            {
                pitchOverride = true;
                rollOverride = true;
            }

            float autoYaw = 0f;
            float autoPitch = 0f;
            float autoRoll = 0f;
            if (controller != null)
                RunAutopilot(controller.MouseAimPos, out autoYaw, out autoPitch, out autoRoll);

            yaw = autoYaw;
            pitch = (pitchOverride) ? keyboardPitch : autoPitch;
            roll = (rollOverride) ? keyboardRoll : autoRoll;

            // Aktifkan atau matikan turbo
            turboActive = Input.GetKey(KeyCode.LeftShift);

            handleInput();
        }

        private void handleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                tembakRudal();
            }
        }

        void tembakRudal()
        {
            Vector3 aimDirection = (controller.MouseAimPos - rudalSpawnPoint.position).normalized;

            if (aimDirection == Vector3.zero)
            {
                aimDirection = rudalSpawnPoint.forward;
                Debug.LogWarning("Aim direction is zero, defaulting to forward direction.");
            }

            Debug.Log($"MouseAimPos: {controller.MouseAimPos}, Aim Direction: {aimDirection}");
            Debug.DrawRay(rudalSpawnPoint.position, aimDirection * 10f, Color.red, 2f);

            GameObject missile = Instantiate(rudalPrefab, rudalSpawnPoint.position, Quaternion.identity);

            missile.transform.rotation = Quaternion.LookRotation(aimDirection);

            Rigidbody rb = missile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = missile.transform.forward * kecepatanRudal;
                rb.angularVelocity = Vector3.zero;
                rb.constraints = RigidbodyConstraints.FreezeRotation;
            }
            else
            {
                Debug.LogError("Missile prefab is missing a Rigidbody component!");
            }

            Destroy(missile, 5f);
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

        private void FixedUpdate()
        {
            float currentThrust = thrust * (turboActive ? turboMultiplier : 1f);
            rigid.AddRelativeForce(Vector3.forward * currentThrust * forceMult, ForceMode.Force);
            rigid.AddRelativeTorque(new Vector3(turnTorque.x * pitch,
                                                turnTorque.y * yaw,
                                                -turnTorque.z * roll) * forceMult,
                                    ForceMode.Force);
        }
    }
}
