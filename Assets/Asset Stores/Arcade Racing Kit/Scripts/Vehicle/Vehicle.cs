using Ilumisoft.ArcardeRacingKit.Input;
using UnityEngine;

namespace Ilumisoft.ArcardeRacingKit
{
    public class Vehicle : MonoBehaviour
    {
        [SerializeField]
        VehicleStats stats = new VehicleStats();

        [SerializeField]
        VehiclePhysics physics = new VehiclePhysics();

        [SerializeField]
        VehicleGroundDetection groundDetection = new VehicleGroundDetection();

        /// <summary>
        /// Reference to the vehicle manager, used to register the vehicle when created and allowing other 
        /// systems to access it
        /// </summary>
        VehicleManager vehicleManager;

        /// <summary>
        /// The input manager collects the input from all input sources, which are on the vehicle
        /// </summary>
        VehicleInputManager inputManager = new VehicleInputManager();

        /// <summary>
        /// Gets the stts of the vehicle after all powerUps have been applied to its base stats
        /// </summary>
        public VehicleStats FinalStats => stats;

        /// <summary>
        /// Gets the rigidbody of the vehicle
        /// </summary>
        public Rigidbody Rigidbody { get; private set; }

        /// <summary>
        /// Gets the input applied to the vehicle
        /// </summary>
        public Vector2 Input => inputManager.Input;

        /// <summary>
        /// Returns whether the vehicle is grounded or not
        /// </summary>
        public bool IsGrounded => groundDetection.IsGrounded;

        /// <summary>
        /// Returns whether the vehicle is allowed to move or not
        /// </summary>
        public bool CanMove { get; set; } = true;

        /// <summary>
        /// The amount of the vehicles velocity going forward
        /// </summary>
        public float ForwardSpeed => Vector3.Dot(Rigidbody.velocity, transform.forward);

        /// <summary>
        /// Returns the forward speed relative to the vehicles max speed. The returned value is in range [-1, 1] 
        /// </summary>
        public float NormalizedForwardSpeed
        {
            get => (Mathf.Abs(ForwardSpeed) > 0.1f) ? ForwardSpeed / FinalStats.MaxSpeed : 0.0f;
        }

        void Awake()
        {
            vehicleManager = FindObjectOfType<VehicleManager>();

            Rigidbody = GetComponent<Rigidbody>();

            inputManager.Initialize(this);
            groundDetection.Initialize(this);

            RegisterVehicle();
        }

        private void OnDestroy()
        {
            // Deregister the vehicle. 
            // This should not be done if the vehicle manager does not exist anymore (e.g. the game is closed or the scene unloaded)
            if (vehicleManager != null)
            {
                vehicleManager.Deregister(this);
            }
        }

        /// <summary>
        /// Registers the vehicle in the vehicle manager. This allows other systems to access it in a very performant way.
        /// </summary>
        void RegisterVehicle()
        {
            if (vehicleManager != null)
            {
                vehicleManager.Register(this);
            }
            else
            {
                Debug.Log("Could not register vehicle, because no Vehicle Manager has been added to the scene.", this);
            }
        }

        /// <summary>
        /// Apply lateral friction, steering and acceleration
        /// </summary>
        protected virtual void FixedUpdate()
        {
            CollectInput();

            PerformGroundCheck();

            ApplyCenterOfMass();

            ApplyGravity();

            ApplyLateralFriction();

            ApplySteering();

            ApplyAcceleration();
        }

        /// <summary>
        /// Collect the input from all input sources
        /// </summary>
        protected virtual void CollectInput()
        {
            inputManager.CollectInput();
        }

        /// <summary>
        /// Checks whether the vehicle is grounded or not
        /// </summary>
        protected virtual void PerformGroundCheck()
        {
            groundDetection.CheckGround();
        }

        /// <summary>
        /// Applies the custom center of mass to the rigidbody
        /// </summary>
        protected virtual void ApplyCenterOfMass()
        {
            if (physics.CenterOfMass != null)
            {
                Rigidbody.centerOfMass = Rigidbody.transform.InverseTransformPoint(physics.CenterOfMass.position);
            }
        }

        /// <summary>
        /// Applies gravity to the vehicle
        /// </summary>
        protected virtual void ApplyGravity()
        {
            float factor = groundDetection.IsGrounded ? physics.Gravity : physics.FallGravity;

            Rigidbody.AddForce(-factor * Vector3.up, ForceMode.Acceleration);
        }

        /// <summary>
        /// Applies side friction to the vehicle. 
        /// The amount of added force defines how much the vehicle drifts while turning.
        /// </summary>
        protected virtual void ApplyLateralFriction()
        {
            if (IsGrounded)
            {
                // Calculate how much the vehicle is moving left or right
                float lateralSpeed = Vector3.Dot(Rigidbody.velocity, transform.right);

                //Calculate the desired amount of friction to apply to the side of the vehicle.
                Vector3 lateralFriction = -transform.right * (lateralSpeed / Time.fixedDeltaTime) * FinalStats.Grip;

                Rigidbody.AddForce(lateralFriction, ForceMode.Acceleration);
            }
        }
      
        /// <summary>
        /// Applies the steering to the vehicle by adding rotation force
        /// </summary>
        protected virtual void ApplySteering()
        {
            if (IsGrounded && CanMove)
            {
                float steeringPower = Input.x * FinalStats.SteeringPower;

                // The vehicle should only rotate if it is actually moving forward or backwards, therefore we
                // will multiply it by the forward speed and a coefficient
                float speedFactor = ForwardSpeed * 0.075f;

                steeringPower = Mathf.Clamp(steeringPower * speedFactor, -FinalStats.SteeringPower, FinalStats.SteeringPower);

                float rotationTorque = steeringPower - Rigidbody.angularVelocity.y;

                Rigidbody.AddRelativeTorque(0f, rotationTorque, 0f, ForceMode.VelocityChange);
            }
        }

        /// <summary>
        /// Applies the movement acceleration to the vehicle
        /// </summary>
        protected virtual void ApplyAcceleration()
        {
            if (IsGrounded && CanMove)
            {
                float drag = FinalStats.Acceleration / FinalStats.MaxSpeed;

                var force = FinalStats.Acceleration * Input.y - drag * Mathf.Clamp(ForwardSpeed, 0f, FinalStats.MaxSpeed);

                Rigidbody.AddForce(transform.forward * force, ForceMode.Acceleration);
            }
        }

        /// <summary>
        /// Add all required components automatically, when creating a new vehicle
        /// </summary>
        public void Reset()
        {
            // Automatically add and setup a rigidbody if none exists
            if (GetComponent<Rigidbody>() == null)
            {
                var rb = gameObject.AddComponent<Rigidbody>();

                rb.interpolation = RigidbodyInterpolation.None;
                rb.useGravity = false;
                rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                rb.mass = 2500;
                rb.angularDrag = 3;
                rb.drag = 0.05f;
            }

            // Automatically add a player input if no input source exists
            if(GetComponent<VehicleInputSource>() == null)
            {
                gameObject.AddComponent<VehiclePlayerInput>();
            }
        }

        /// <summary>
        /// Draw additional debug info in the scene view when the vehicle is selected
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            groundDetection.OnDrawGizmosSelected(this);
        }
    }
}
