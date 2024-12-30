using UnityEngine;

namespace MFlight
{
    public class MouseFlightController : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] [Tooltip("Transform of the aircraft the rig follows and references")]
        private Transform aircraft = null;
        [SerializeField] [Tooltip("Transform of the object the mouse rotates to generate MouseAim position")]
        private Transform mouseAim = null;
        [SerializeField] [Tooltip("Transform of the object on the rig which the camera is attached to")]
        private Transform cameraRig = null;
        [SerializeField] [Tooltip("Transform of the camera itself")]
        private Transform cam = null;

        [Header("Options")]
        [SerializeField] [Tooltip("Follow aircraft using fixed update loop")]
        private bool useFixed = true;

        [SerializeField] [Tooltip("How quickly the camera tracks the mouse aim point.")]
        private float camSmoothSpeed = 5f;

        [SerializeField] [Tooltip("Mouse sensitivity for the mouse flight target")]
        private float mouseSensitivity = 3f;

        [SerializeField] [Tooltip("How far the boresight and mouse flight are from the aircraft")]
        private float aimDistance = 500f;

        [Space]
        [SerializeField] [Tooltip("How far the boresight and mouse flight are from the aircraft")]
        private bool showDebugInfo = false;

        private Vector3 frozenDirection = Vector3.forward;
        private bool isMouseAimFrozen = false;
        public Vector3 BoresightPos
        {
            get
            {
                return aircraft == null
                     ? transform.forward * aimDistance
                     : (aircraft.transform.forward * aimDistance) + aircraft.transform.position;
            }
        }

        public Vector3 MouseAimPos
        {
            get
            {
                if (mouseAim != null)
                {
                    return isMouseAimFrozen
                        ? GetFrozenMouseAimPos()
                        : mouseAim.position + (mouseAim.forward * aimDistance);
                }
                else
                {
                    return transform.forward * aimDistance;
                }
            }
        }

        private void Awake()
        {
            if (aircraft == null)
                Debug.LogError(name + "MouseFlightController - No aircraft transform assigned!");
            if (mouseAim == null)
                Debug.LogError(name + "MouseFlightController - No mouse aim transform assigned!");
            if (cameraRig == null)
                Debug.LogError(name + "MouseFlightController - No camera rig transform assigned!");
            if (cam == null)
                Debug.LogError(name + "MouseFlightController - No camera transform assigned!");

            transform.parent = null;
        }

        private void Update()
        {
            if (useFixed == false)
                UpdateCameraPos();

            RotateRig();
        }

        private void FixedUpdate()
        {
            if (useFixed == true)
                UpdateCameraPos();
        }

        private void RotateRig()
        {
            if (mouseAim == null || cam == null || cameraRig == null)
                return;

            if (Input.GetKeyDown(KeyCode.C))
            {
                isMouseAimFrozen = true;
                frozenDirection = mouseAim.forward;
            }
            else if  (Input.GetKeyUp(KeyCode.C))
            {
                isMouseAimFrozen = false;
                mouseAim.forward = frozenDirection;
            }

            // Mouse input.
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = -Input.GetAxis("Mouse Y") * mouseSensitivity;

            mouseAim.Rotate(cam.right, mouseY, Space.World);
            mouseAim.Rotate(cam.up, mouseX, Space.World);

            Vector3 upVec = (Mathf.Abs(mouseAim.forward.y) > 0.9f) ? cameraRig.up : Vector3.up;

            cameraRig.rotation = Damp(cameraRig.rotation,
                                      Quaternion.LookRotation(mouseAim.forward, upVec),
                                      camSmoothSpeed,
                                      Time.deltaTime);
        }

        private Vector3 GetFrozenMouseAimPos()
        {
            if (mouseAim != null)
                return mouseAim.position + (frozenDirection * aimDistance);
            else
                return transform.forward * aimDistance;
        }

        private void UpdateCameraPos()
        {
            if (aircraft != null)
            {
                transform.position = aircraft.position;
            }
        }

        // Thanks to Rory Driscoll
        // http://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp/
        /// <summary>
        /// Creates dampened motion between a and b that is framerate independent.
        /// </summary>
        /// <param name="a">Initial parameter</param>
        /// <param name="b">Target parameter</param>
        /// <param name="lambda">Smoothing factor</param>
        /// <param name="dt">Time since last damp call</param>
        /// <returns></returns>
        private Quaternion Damp(Quaternion a, Quaternion b, float lambda, float dt)
        {
            return Quaternion.Slerp(a, b, 1 - Mathf.Exp(-lambda * dt));
        }

        private void OnDrawGizmos()
        {
            if (showDebugInfo == true)
            {
                Color oldColor = Gizmos.color;

                if (aircraft != null)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireSphere(BoresightPos, 10f);
                }

                if (mouseAim != null)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireSphere(MouseAimPos, 10f);

                    Gizmos.color = Color.blue;
                    Gizmos.DrawRay(mouseAim.position, mouseAim.forward * 50f);
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(mouseAim.position, mouseAim.up * 50f);
                    Gizmos.color = Color.red;
                    Gizmos.DrawRay(mouseAim.position, mouseAim.right * 50f);
                }

                Gizmos.color = oldColor;
            }
        }
    }
}
