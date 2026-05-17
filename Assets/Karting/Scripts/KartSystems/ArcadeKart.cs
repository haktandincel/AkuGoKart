using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.VFX;
using Cinemachine;
using System.Collections;
using TMPro;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace KartGame.KartSystems
{
    public class ArcadeKart : MonoBehaviour
    {
        // ============================================================================
        // CONSTANTS - Sihirli Sayılar
        // ============================================================================
        private const float SPIN_SPEED_DEG_PER_SEC = 720f;
        private const float NITRO_DRAIN_SPEED = 0.2f;
        private const float SUSPENSION_COMPRESSION_VISUAL = 0.4f;
        private const float INPUT_NULL_THRESHOLD = 0.01f;
        private const float SPEED_NULL_THRESHOLD = 0.01f;
        private const float RAYCST_DISTANCE = 3.0f;
        private const float GROUND_RAYCAST_LAYERS = 1 << 9 | 1 << 10 | 1 << 11; // Ground(9) / Environment(10) / Track(11)
        private const float AIRBORNE_REORIENTATION_MULTIPLIER = 10.0f;
        private const float VELOCITY_STEERING_COEFFICIENT = 25f;
        private const float ANGULAR_VELOCITY_STEERING = 0.4f;
        private const float ANGULAR_VELOCITY_SMOOTH_SPEED = 20f;
        private const float AIRBORNE_ANGULAR_VELOCITY_DAMPEN = 0.98f;
        private const float DRIFT_GROUNDED_THRESHOLD = 0.1f;
        private const float HALF_GROUNDED_THRESHOLD = 0.7f;

        // ============================================================================
        // INNER CLASSES
        // ============================================================================
        [System.Serializable]
        public class StatPowerup
        {
            public ArcadeKart.Stats modifiers;
            public string PowerUpID;
            public float ElapsedTime;
            public float MaxTime;
        }

        // ============================================================================
        // UI & NITRO SYSTEM
        // ============================================================================
        private MyHorizontalProgressBar nitroUI;
        private float nitroUIValue = 0f;
        private float nitroDrainSpeed = NITRO_DRAIN_SPEED;

        public TextMeshProUGUI player1WinText;
        public TextMeshProUGUI player2WinText;
        
        // ============================================================================
        // BANANA PEEL SYSTEM
        // ============================================================================
        [Header("Banana Peel Settings")]
        [Tooltip("Fırlatılacak muz kabuğu prefab'ı (Rigidbody ve Collider içermeli).")]
        public GameObject bananaPrefab;
        [Tooltip("Muzun kartın neresinden çıkacağı.")]
        public Transform bananaSpawnPoint;

        // ============================================================================
        // SPIN SYSTEM (Muz Cezası)
        // ============================================================================
        public bool IsSpinning { get; private set; } = false;
        private float m_SpinElapsedTime = 0.0f;
        private float m_SpinDuration = 0.0f;

        [System.Serializable]
        public struct Stats
        {
            [Header("Movement Settings")]
            [Min(0.001f), Tooltip("Top speed attainable when moving forward.")]
            public float TopSpeed;

            [Tooltip("How quickly the kart reaches top speed.")]
            public float Acceleration;

            [Min(0.001f), Tooltip("Top speed attainable when moving backward.")]
            public float ReverseSpeed;

            [Tooltip("How quickly the kart reaches top speed, when moving backward.")]
            public float ReverseAcceleration;

            [Tooltip("How quickly the kart starts accelerating from 0. A higher number means it accelerates faster sooner.")]
            [Range(0.2f, 1)]
            public float AccelerationCurve;

            [Tooltip("How quickly the kart slows down when the brake is applied.")]
            public float Braking;

            [Tooltip("How quickly the kart will reach a full stop when no inputs are made.")]
            public float CoastingDrag;

            [Range(0.0f, 1.0f)]
            [Tooltip("The amount of side-to-side friction.")]
            public float Grip;

            [Tooltip("How tightly the kart can turn left or right.")]
            public float Steer;

            [Tooltip("Additional gravity for when the kart is in the air.")]
            public float AddedGravity;

            // allow for stat adding for powerups.
            public static Stats operator +(Stats a, Stats b)
            {
                return new Stats
                {
                    Acceleration        = a.Acceleration + b.Acceleration,
                    AccelerationCurve   = a.AccelerationCurve + b.AccelerationCurve,
                    Braking             = a.Braking + b.Braking,
                    CoastingDrag        = a.CoastingDrag + b.CoastingDrag,
                    AddedGravity        = a.AddedGravity + b.AddedGravity,
                    Grip                = a.Grip + b.Grip,
                    ReverseAcceleration = a.ReverseAcceleration + b.ReverseAcceleration,
                    ReverseSpeed        = a.ReverseSpeed + b.ReverseSpeed,
                    TopSpeed            = a.TopSpeed + b.TopSpeed,
                    Steer               = a.Steer + b.Steer,
                };
            }
        }
        public Rigidbody Rigidbody { get; private set; }
        public InputData Input     { get; private set; }
        public float AirPercent    { get; private set; }
        public float GroundPercent { get; private set; }
        public CinemachineVirtualCamera vCam;

        public GameObject nitroBar;

        private CinemachineTransposer transposer;


        public ArcadeKart.Stats baseStats = new ArcadeKart.Stats
        {
            TopSpeed            = 10f,
            Acceleration        = 5f,
            AccelerationCurve   = 4f,
            Braking             = 10f,
            ReverseAcceleration = 5f,
            ReverseSpeed        = 5f,
            Steer               = 5f,
            CoastingDrag        = 4f,
            Grip                = .95f,
            AddedGravity        = 1f,
        };

        [Header("Vehicle Visual")] 
        public List<GameObject> m_VisualWheels;

        [Header("Vehicle Physics")]
        [Tooltip("The transform that determines the position of the kart's mass.")]
        public Transform CenterOfMass;

        [Range(0.0f, 20.0f), Tooltip("Coefficient used to reorient the kart in the air. The higher the number, the faster the kart will readjust itself along the horizontal plane.")]
        public float AirborneReorientationCoefficient = 3.0f;

        [Header("Drifting")]
        [Range(0.01f, 1.0f), Tooltip("The grip value when drifting.")]
        public float DriftGrip = 0.4f;
        [Range(0.0f, 10.0f), Tooltip("Additional steer when the kart is drifting.")]
        public float DriftAdditionalSteer = 5.0f;
        [Range(1.0f, 30.0f), Tooltip("The higher the angle, the easier it is to regain full grip.")]
        public float MinAngleToFinishDrift = 10.0f;
        [Range(0.01f, 0.99f), Tooltip("Mininum speed percentage to switch back to full grip.")]
        public float MinSpeedPercentToFinishDrift = 0.5f;
        [Range(1.0f, 20.0f), Tooltip("The higher the value, the easier it is to control the drift steering.")]
        public float DriftControl = 10.0f;
        [Range(0.0f, 20.0f), Tooltip("The lower the value, the longer the drift will last without trying to control it by steering.")]
        public float DriftDampening = 10.0f;

        [Header("Nitro Boost")]
        [Range(0.0f, 10.0f), Tooltip("Speed multiplier when nitro is active.")]
        public float NitroSpeedMultiplier = 1.5f;
        [Range(0.0f, 10.0f), Tooltip("Acceleration multiplier when nitro is active.")]
        public float NitroAccelerationMultiplier = 2.0f;
        [Range(0.1f, 10.0f), Tooltip("How long the nitro boost lasts in seconds.")]
        public float NitroDuration = 3.0f;
        [Range(0.1f, 10.0f), Tooltip("Cooldown time before nitro can be used again.")]
        public float NitroCooldown = 2.0f;
        [Tooltip("VFX that spawns when nitro is activated.")]
        public GameObject NitroBoostVFX;
        [Range(0.0f, 2.0f), Tooltip("Visual trail effect for nitro boost.")]
        public ParticleSystem NitroTrailVFX;

        [Header("Nitro Camera Effects")]
        [Range(30f, 100f), Tooltip("Normal camera field of view.")]
        public float NormalFOV = 60f;
        [Range(30f, 100f), Tooltip("Camera field of view when nitro is active.")]
        public float NitroFOV = 75f;
        [Range(0.1f, 5f), Tooltip("Speed of FOV transition.")]
        public float FOVTransitionSpeed = 2f;
        [Range(0.1f, 2f), Tooltip("Duration of VFX fade-out effect when nitro ends.")]
        public float VFXFadeDuration = 0.5f;
        [Tooltip("Normal camera offset position.")]
        public Vector3 NormalCameraOffset = new Vector3(0, 0.6f, -4f);
        [Tooltip("Camera offset when nitro is active.")]
        public Vector3 NitroCameraOffset = new Vector3(0, 0.6f, -5.5f);
        [Range(0.1f, 5f), Tooltip("Speed of camera offset transition.")]
        public float OffsetTransitionSpeed = 2f;

        [Header("VFX")]
        [Tooltip("VFX that will be placed on the wheels when drifting.")]
        public ParticleSystem DriftSparkVFX;
        [Range(0.0f, 0.2f), Tooltip("Offset to displace the VFX to the side.")]
        public float DriftSparkHorizontalOffset = 0.1f;
        [Range(0.0f, 90.0f), Tooltip("Angle to rotate the VFX.")]
        public float DriftSparkRotation = 17.0f;
        [Tooltip("VFX that will be placed on the wheels when drifting.")]
        public GameObject DriftTrailPrefab;
        [Range(-0.1f, 0.1f), Tooltip("Vertical to move the trails up or down and ensure they are above the ground.")]
        public float DriftTrailVerticalOffset;
        [Tooltip("VFX that will spawn upon landing, after a jump.")]
        public GameObject JumpVFX;
        [Tooltip("VFX that is spawn on the nozzles of the kart.")]
        public GameObject NozzleVFX;
        [Tooltip("List of the kart's nozzles.")]
        public GameObject existingVFX;
        public List<Transform> Nozzles;

        [Header("Suspensions")]
        [Tooltip("The maximum extension possible between the kart's body and the wheels.")]
        [Range(0.0f, 1.0f)]
        public float SuspensionHeight = 0.2f;
        [Range(10.0f, 100000.0f), Tooltip("The higher the value, the stiffer the suspension will be.")]
        public float SuspensionSpring = 20000.0f;
        [Range(0.0f, 5000.0f), Tooltip("The higher the value, the faster the kart will stabilize itself.")]
        public float SuspensionDamp = 500.0f;
        [Tooltip("Vertical offset to adjust the position of the wheels relative to the kart's body.")]
        [Range(-1.0f, 1.0f)]
        public float WheelsPositionVerticalOffset = 0.0f;

        [Header("Spring Jump Settings")]
        [Tooltip("Zıplama kuvveti. Ne kadar yüksek olursa kart o kadar yukarı zıplar.")]
        public float jumpForce = 8f;

        [Header("Charge Jump Settings")]
        [Tooltip("Minimum (hiç şarj etmeden) zıplama kuvveti.")]
        public float minJumpForce = 0f;
        [Tooltip("Maksimum (tam şarjlı) zıplama kuvveti.")]
        public float maxJumpForce = 20f;
        [Tooltip("Şarjın maksimum seviyeye ulaşma hızı (Saniye).")]
        public float chargeSpeed = 2.5f;

        // Şarj durumunu takip eden dahili değişkenler
        private float m_CurrentJumpCharge = 0f;
        private bool m_IsChargingJump = false;
        
        // Zıplama girdisini takip etmek için
        private bool m_LastJumpInput = false;

        [Header("Physical Wheels")]
        [Tooltip("The physical representations of the Kart's wheels.")]
        public WheelCollider FrontLeftWheel;
        public WheelCollider FrontRightWheel;
        public WheelCollider RearLeftWheel;
        public WheelCollider RearRightWheel;

        [Tooltip("Which layers the wheels will detect.")]
        public LayerMask GroundLayers = Physics.DefaultRaycastLayers;

        // the input sources that can control the kart
        IInput[] m_Inputs;

        const float k_NullInput = 0.01f;
        const float k_NullSpeed = 0.01f;
        Vector3 m_VerticalReference = Vector3.up;

        // Drift params
        public bool WantsToDrift { get; private set; } = false;
        public bool IsDrifting { get; private set; } = false;
        float m_CurrentGrip = 1.0f;
        float m_DriftTurningPower = 0.0f;
        float m_PreviousGroundPercent = 1.0f;
        readonly List<(GameObject trailRoot, WheelCollider wheel, TrailRenderer trail)> m_DriftTrailInstances = new List<(GameObject, WheelCollider, TrailRenderer)>();
        readonly List<(WheelCollider wheel, float horizontalOffset, float rotation, ParticleSystem sparks)> m_DriftSparkInstances = new List<(WheelCollider, float, float, ParticleSystem)>();

        // Nitro params
        public bool IsNitroActive { get; private set; } = false;
        float m_NitroElapsedTime = 0.0f;
        float m_NitroCooldownElapsedTime = 0.0f;
        bool m_HasNitroVFX = false;
        bool m_IsDestroyingNitroVFX = false;

        // Banana Peel params
        bool m_LastBananaPeelInput = false;

        public Transform nitroSpawnpoint;

        // can the kart move?
        bool m_CanMove = true;
        List<StatPowerup> m_ActivePowerupList = new List<StatPowerup>();
        ArcadeKart.Stats m_FinalStats;
        GameObject NitroVFX;
        Quaternion m_LastValidRotation;
        Vector3 m_LastValidPosition;
        Vector3 m_LastCollisionNormal;
        bool m_HasCollision;
        bool m_InAir = false;

        public void AddPowerup(StatPowerup statPowerup) => m_ActivePowerupList.Add(statPowerup);
        public void SetCanMove(bool move) => m_CanMove = move;
        public float GetMaxSpeed() => Mathf.Max(m_FinalStats.TopSpeed, m_FinalStats.ReverseSpeed);
        public float GetNitroCharge() => Mathf.Max(0, 1.0f - (m_NitroCooldownElapsedTime / NitroCooldown));
        public bool CanUseNitro(){
             if (nitroBar.GetComponent<MyHorizontalProgressBar>().GetProgress() > 0.0f)
            {
                return true;
            }
                else return false;
                
             }

        public void ActivateNitro()
        {
            if (CanUseNitro())
            {
                // Eski VFX'i temizle
                if (NitroVFX != null)
                {
                    Destroy(NitroVFX);
                }
                
                IsNitroActive = true;
                m_NitroElapsedTime = 0.0f;
                m_NitroCooldownElapsedTime = 0.0f;
                m_IsDestroyingNitroVFX = false;
                
         
                    NitroVFX = Instantiate(NitroBoostVFX,nitroSpawnpoint);
                    NitroVFX.GetComponent<ParticleSystem>().Play();
                    m_HasNitroVFX = true;               
            }
        }

        private void ActivateDriftVFX(bool active)
        {
            foreach (var vfx in m_DriftSparkInstances)
            {
                if (active && vfx.wheel.GetGroundHit(out WheelHit hit))
                {
                    if (!vfx.sparks.isPlaying)
                        vfx.sparks.Play();
                }
                else
                {
                    if (vfx.sparks.isPlaying)
                        vfx.sparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                }
                    
            }

            foreach (var trail in m_DriftTrailInstances)
                trail.Item3.emitting = active && trail.wheel.GetGroundHit(out WheelHit hit);
        }

        private void UpdateDriftVFXOrientation()
        {
            foreach (var vfx in m_DriftSparkInstances)
            {
                vfx.sparks.transform.position = vfx.wheel.transform.position - (vfx.wheel.radius * Vector3.up) + (DriftTrailVerticalOffset * Vector3.up) + (transform.right * vfx.horizontalOffset);
                vfx.sparks.transform.rotation = transform.rotation * Quaternion.Euler(0.0f, 0.0f, vfx.rotation);
            }

            foreach (var trail in m_DriftTrailInstances)
            {
                trail.trailRoot.transform.position = trail.wheel.transform.position - (trail.wheel.radius * Vector3.up) + (DriftTrailVerticalOffset * Vector3.up);
                trail.trailRoot.transform.rotation = transform.rotation;
            }
        }

        void UpdateSuspensionParams(WheelCollider wheel)
        {
            wheel.suspensionDistance = SuspensionHeight;
            wheel.center = new Vector3(0.0f, WheelsPositionVerticalOffset, 0.0f);
            JointSpring spring = wheel.suspensionSpring;
            spring.spring = SuspensionSpring;
            spring.damper = SuspensionDamp;
            wheel.suspensionSpring = spring;
        }

        void Awake()
        {
            nitroUI = nitroBar.GetComponent<MyHorizontalProgressBar>();
            nitroUIValue = 0f;
            nitroUI.SetProgress(0f);
            Rigidbody = GetComponent<Rigidbody>();
            m_Inputs = GetComponents<IInput>();

            if (vCam == null) vCam = GameObject.Find("CinemachineVirtualCamera").GetComponent<CinemachineVirtualCamera>();
            
            // Get reference to the Transposer component
            transposer = vCam.GetCinemachineComponent<CinemachineTransposer>();

            UpdateSuspensionParams(FrontLeftWheel);
            UpdateSuspensionParams(FrontRightWheel);
            UpdateSuspensionParams(RearLeftWheel);
            UpdateSuspensionParams(RearRightWheel);

            m_CurrentGrip = baseStats.Grip;

            if (DriftSparkVFX != null)
            {
                AddSparkToWheel(RearLeftWheel, -DriftSparkHorizontalOffset, -DriftSparkRotation);
                AddSparkToWheel(RearRightWheel, DriftSparkHorizontalOffset, DriftSparkRotation);
            }

            if (DriftTrailPrefab != null)
            {
                AddTrailToWheel(RearLeftWheel);
                AddTrailToWheel(RearRightWheel);
            }

            if (NozzleVFX != null)
            {
                foreach (var nozzle in Nozzles)
                {
                    Instantiate(NozzleVFX, nozzle, false);
                }
            }

            // Initialize Nitro
            m_NitroElapsedTime = 0.0f;
            m_NitroCooldownElapsedTime = NitroCooldown; // Start with nitro available
            IsNitroActive = false;
            m_IsDestroyingNitroVFX = false;
        }

        void AddTrailToWheel(WheelCollider wheel)
        {
            GameObject trailRoot = Instantiate(DriftTrailPrefab, gameObject.transform, false);
            TrailRenderer trail = trailRoot.GetComponentInChildren<TrailRenderer>();
            trail.emitting = false;
            m_DriftTrailInstances.Add((trailRoot, wheel, trail));
        }

        void AddSparkToWheel(WheelCollider wheel, float horizontalOffset, float rotation)
        {
            GameObject vfx = Instantiate(DriftSparkVFX.gameObject, wheel.transform, false);
            ParticleSystem spark = vfx.GetComponent<ParticleSystem>();
            spark.Stop();
            m_DriftSparkInstances.Add((wheel, horizontalOffset, -rotation, spark));
        }

        void Update()
        {
            // Handle nitro camera effects only when nitro is actually active (duration-based)
            if (IsNitroActive && nitroBar.GetComponent<MyHorizontalProgressBar>().GetProgress() > 0.0f)
            {
                nitroUIValue -= Time.deltaTime * nitroDrainSpeed;
                    nitroUIValue = Mathf.Clamp01(nitroUIValue);

                    nitroUI.SetProgress(nitroUIValue);
                // FOV değerini yumuşakça artır
                vCam.m_Lens.FieldOfView = Mathf.Lerp(vCam.m_Lens.FieldOfView, NitroFOV, Time.deltaTime * FOVTransitionSpeed);
                
                // transposer.m_FollowOffset = Vector3.Lerp(transposer.m_FollowOffset, NitroCameraOffset, Time.deltaTime * OffsetTransitionSpeed);
            }
            else // Nitro süresi bittiğinde
            {
                // FOV değerini normale döndür
                vCam.m_Lens.FieldOfView = Mathf.Lerp(vCam.m_Lens.FieldOfView, NormalFOV, Time.deltaTime * FOVTransitionSpeed);
                
                // VFX'i yumuşak bir şekilde yok et
                if (NitroVFX != null && !m_IsDestroyingNitroVFX)
                {
                    m_IsDestroyingNitroVFX = true;
                    StartCoroutine(FadeOutAndDestroyVFX(NitroVFX, VFXFadeDuration));
                }
                // Offset değerini normale döndür
                // transposer.m_FollowOffset = Vector3.Lerp(transposer.m_FollowOffset, NormalCameraOffset, Time.deltaTime * OffsetTransitionSpeed);
            }
        }

        void FixedUpdate()
        {
            UpdateSuspensionParams(FrontLeftWheel);
            UpdateSuspensionParams(FrontRightWheel);
            UpdateSuspensionParams(RearLeftWheel);
            UpdateSuspensionParams(RearRightWheel);

            GatherInputs();

            // apply our powerups to create our finalStats
            TickPowerups();

            // apply our physics properties
            Rigidbody.centerOfMass = transform.InverseTransformPoint(CenterOfMass.position);

            int groundedCount = 0;
            if (FrontLeftWheel.isGrounded && FrontLeftWheel.GetGroundHit(out WheelHit hit))
                groundedCount++;
            if (FrontRightWheel.isGrounded && FrontRightWheel.GetGroundHit(out hit))
                groundedCount++;
            if (RearLeftWheel.isGrounded && RearLeftWheel.GetGroundHit(out hit))
                groundedCount++;
            if (RearRightWheel.isGrounded && RearRightWheel.GetGroundHit(out hit))
                groundedCount++;

            // calculate how grounded and airborne we are
            GroundPercent = (float) groundedCount / 4.0f;
            AirPercent = 1 - GroundPercent;

            // apply vehicle physics
            if (m_CanMove)
            {
                MoveVehicle(Input.Accelerate, Input.Brake, Input.TurnInput);
            }
            GroundAirbourne();

            m_PreviousGroundPercent = GroundPercent;

            UpdateDriftVFXOrientation();
        }

        void GatherInputs()
        {
            // Reset giriş verisi
            Input = new InputData();
            WantsToDrift = false;

            // Tüm giriş kaynağından input topla
            for (int i = 0; i < m_Inputs.Length; i++)
            {
                Input = m_Inputs[i].GenerateInput();
                WantsToDrift = Input.Brake && Vector3.Dot(Rigidbody.linearVelocity, transform.forward) > 0.0f;

                // Giriş işlemleri
                HandleNitroInput();
                HandleBananaPeelInput();
                HandleJumpInput();
            }
        }

        /// <summary>
        /// Nitro girdisini işler
        /// </summary>
        private void HandleNitroInput()
        {
            if (Input.Nitro)
            {
                ActivateNitro();
            }
        }

        /// <summary>
        /// Muz kabuğu girdisini işler (geçiş tabanlı - sadece tuşa basılışta)
        /// </summary>
        private void HandleBananaPeelInput()
        {
            if (Input.BananaPeel && !m_LastBananaPeelInput)
            {
                DropBananaPeel();
            }
            m_LastBananaPeelInput = Input.BananaPeel;
        }

        /// <summary>
        /// Zıplama girdisini işler (şarj tabanlı)
        /// Adımlar:
        /// 1. Tuşa ilk basılışında şarjlamayı başlat
        /// 2. Tuşa basılı tutulduğu sürece şarjı doldur
        /// 3. Tuş bırakıldığında zıplamayı tetikle
        /// </summary>
        private void HandleJumpInput()
        {
            // 1. Tuşa ilk basılma: Şarjlamaya başla
            if (Input.Jump && !m_LastJumpInput)
            {
                m_IsChargingJump = true;
                m_CurrentJumpCharge = 0f;
            }

            // 2. Tuşa basılı tutulduğu sürece: Şarjı doldur
            if (Input.Jump && m_IsChargingJump)
            {
                m_CurrentJumpCharge += Time.deltaTime * (1f / chargeSpeed);
                m_CurrentJumpCharge = Mathf.Clamp01(m_CurrentJumpCharge);

                // Görsel Detay: Şarj olurken kartı basık görünmesini sağla
                WheelsPositionVerticalOffset = Mathf.Lerp(0f, SUSPENSION_COMPRESSION_VISUAL, m_CurrentJumpCharge);
            }

            // 3. Tuş bırakıldığı an: Zıplamayı tetikle
            if (!Input.Jump && m_LastJumpInput && m_IsChargingJump)
            {
                ExecuteChargeJump();
            }

            // Sonraki frame'de geçişi detect etmek için kaydet
            m_LastJumpInput = Input.Jump;
        }

        void TickPowerups()
        {
            // remove all elapsed powerups
            m_ActivePowerupList.RemoveAll((p) => { return p.ElapsedTime > p.MaxTime; });

            // zero out powerups before we add them all up
            var powerups = new Stats();

            // add up all our powerups
            for (int i = 0; i < m_ActivePowerupList.Count; i++)
            {
                var p = m_ActivePowerupList[i];

                // add elapsed time
                p.ElapsedTime += Time.fixedDeltaTime;

                // add up the powerups
                powerups += p.modifiers;
            }

            // Handle Nitro Boost
            if (IsNitroActive)
            {
                m_NitroElapsedTime += Time.fixedDeltaTime;

                if (nitroBar.GetComponent<MyHorizontalProgressBar>().GetProgress() <= 0.0f)
                {
                    IsNitroActive = false;
                    m_NitroCooldownElapsedTime = 0.0f;
                    nitroUIValue = 0.0f; // UI'ı sıfırla
                    nitroUI.SetProgress(0.0f);

                    // Stop nitro VFX when duration ends
                    if (NitroTrailVFX != null && NitroTrailVFX.isPlaying)
                    {
                        NitroTrailVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    }
                    if (existingVFX != null)
                    {
                        existingVFX.SetActive(false);
                    }
                }
                else
                {
                    // Apply nitro boost to stats
                    powerups.TopSpeed += baseStats.TopSpeed * (NitroSpeedMultiplier - 1.0f);
                    powerups.Acceleration += baseStats.Acceleration * (NitroAccelerationMultiplier - 1.0f);
                }
            }
            else if (m_NitroCooldownElapsedTime < NitroCooldown)
            {
                m_NitroCooldownElapsedTime += Time.fixedDeltaTime;
            }

            // add powerups to our final stats
            m_FinalStats = baseStats + powerups;

            // clamp values in finalstats
            m_FinalStats.Grip = Mathf.Clamp(m_FinalStats.Grip, 0, 1);
        }

        void GroundAirbourne()
        {
            // while in the air, fall faster
            if (AirPercent >= 1)
            {
                Rigidbody.linearVelocity += Physics.gravity * Time.fixedDeltaTime * m_FinalStats.AddedGravity;
            }
        }

        public void Reset()
        {
            Vector3 euler = transform.rotation.eulerAngles;
            euler.x = euler.z = 0f;
            transform.rotation = Quaternion.Euler(euler);
        }

        public float LocalSpeed()
        {
            if (m_CanMove)
            {
                float dot = Vector3.Dot(transform.forward, Rigidbody.linearVelocity);
                if (Mathf.Abs(dot) > 0.1f)
                {
                    float speed = Rigidbody.linearVelocity.magnitude;
                    return dot < 0 ? -(speed / m_FinalStats.ReverseSpeed) : (speed / m_FinalStats.TopSpeed);
                }
                return 0f;
            }
            else
            {
                // use this value to play kart sound when it is waiting the race start countdown.
                return Input.Accelerate ? 1.0f : 0.0f;
            }
        }
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("BananaPeel"))
            {
                IsSpinning = true;
                m_SpinElapsedTime = 0.0f;
                m_SpinDuration = 1.0f; // Spin süresi (örneğin 2 saniye)
                ApplySpinMovement();
                Destroy(other.gameObject);
            }
            if (other.CompareTag("NOS") && !other.gameObject.GetComponent<Nitro1Time>().isCollected)
            {               
                if (nitroUIValue < 1.0f)
                {
                    nitroUIValue += 0.5f;
                }
                nitroUI.SetProgress(nitroUIValue); 
                other.gameObject.GetComponent<Nitro1Time>().isCollected = true;                                     
                Destroy(other.gameObject);
            }

            if (other.gameObject.name == "FinishTrigger")
            {
                if (this.gameObject.name == "Player1")
                {
                    player1WinText.gameObject.SetActive(true);
                    GameManager.Instance.isplayer1Won = true;
                }
                else if (this.gameObject.name == "Player2")
                {
                    player2WinText.gameObject.SetActive(true);
                    GameManager.Instance.isplayer1Won = false;
                }

                SceneManager.LoadScene("GirisScene");
            }
        }

        void OnCollisionEnter(Collision collision) => m_HasCollision = true;
        void OnCollisionExit(Collision collision) => m_HasCollision = false;

        void OnCollisionStay(Collision collision)
        {
            m_HasCollision = true;
            m_LastCollisionNormal = Vector3.zero;
            float dot = -1.0f;

            foreach (var contact in collision.contacts)
            {
                if (Vector3.Dot(contact.normal, Vector3.up) > dot)
                    m_LastCollisionNormal = contact.normal;
            }
        }

        void MoveVehicle(bool accelerate, bool brake, float turnInput)
        {
            if (IsSpinning)
            {
                ApplySpinMovement();
                return;
            }

            float accelInput = (accelerate ? 1.0f : 0.0f) - (brake ? 1.0f : 0.0f);

            // İvme eğrisi katsayısı
            const float ACCELERATION_CURVE_COEFFICIENT = 5f;
            Vector3 localVel = transform.InverseTransformVector(Rigidbody.linearVelocity);

            bool accelDirectionIsFwd = accelInput >= 0;
            bool localVelDirectionIsFwd = localVel.z >= 0;

            // Gidiş yönüne göre maksimum hız seçer
            float maxSpeed = localVelDirectionIsFwd ? m_FinalStats.TopSpeed : m_FinalStats.ReverseSpeed;
            float accelPower = accelDirectionIsFwd ? m_FinalStats.Acceleration : m_FinalStats.ReverseAcceleration;

            float currentSpeed = Rigidbody.linearVelocity.magnitude;
            float accelRampT = currentSpeed / maxSpeed;
            float multipliedAccelerationCurve = m_FinalStats.AccelerationCurve * ACCELERATION_CURVE_COEFFICIENT;
            float accelRamp = Mathf.Lerp(multipliedAccelerationCurve, 1, accelRampT * accelRampT);

            bool isBraking = (localVelDirectionIsFwd && brake) || (!localVelDirectionIsFwd && accelerate);

            // if we are braking (moving reverse to where we are going)
            // use the braking accleration instead
            float finalAccelPower = isBraking ? m_FinalStats.Braking : accelPower;

            float finalAcceleration = finalAccelPower * accelRamp;

            // apply inputs to forward/backward
            float turningPower = IsDrifting ? m_DriftTurningPower : turnInput * m_FinalStats.Steer;

            Quaternion turnAngle = Quaternion.AngleAxis(turningPower, transform.up);
            Vector3 fwd = turnAngle * transform.forward;
            Vector3 movement = fwd * accelInput * finalAcceleration * ((m_HasCollision || GroundPercent > 0.0f) ? 1.0f : 0.0f);

            // forward movement
            bool wasOverMaxSpeed = currentSpeed >= maxSpeed;

            // if over max speed, cannot accelerate faster.
            if (wasOverMaxSpeed && !isBraking) 
                movement *= 0.0f;

            Vector3 newVelocity = Rigidbody.linearVelocity + movement * Time.fixedDeltaTime;
            newVelocity.y = Rigidbody.linearVelocity.y;

            //  clamp max speed if we are on ground
            if (GroundPercent > 0.0f && !wasOverMaxSpeed)
            {
                newVelocity = Vector3.ClampMagnitude(newVelocity, maxSpeed);
            }

            // coasting is when we aren't touching accelerate
            if (Mathf.Abs(accelInput) < k_NullInput && GroundPercent > 0.0f)
            {
                newVelocity = Vector3.MoveTowards(newVelocity, new Vector3(0, Rigidbody.linearVelocity.y, 0), Time.fixedDeltaTime * m_FinalStats.CoastingDrag);
            }

            Rigidbody.linearVelocity = newVelocity;

            // Drift & Ground Physics
            if (GroundPercent > 0.0f)
            {
                if (m_InAir)
                {
                    m_InAir = false;
                    Instantiate(JumpVFX, transform.position, Quaternion.identity);
                }

                // Kart dönüşü (yaw) kontrol değişkenleri
                float angularVelocitySteering = ANGULAR_VELOCITY_STEERING;
                float angularVelocitySmoothSpeed = ANGULAR_VELOCITY_SMOOTH_SPEED;

                // Geri giderken vs ileriye giderken dönüş ters çevrilir
                if (!localVelDirectionIsFwd && !accelDirectionIsFwd) 
                    angularVelocitySteering *= -1.0f;

                var angularVel = Rigidbody.angularVelocity;

                // Y ekseni açısal hızını hedef yöne doğru hareket ettir
                angularVel.y = Mathf.MoveTowards(angularVel.y, turningPower * angularVelocitySteering, Time.fixedDeltaTime * angularVelocitySmoothSpeed);

                // Açısal hızı uygula
                Rigidbody.angularVelocity = angularVel;

                // Hız vektörünü de döndür - hızlı yön değişikliği sağla
                float velocitySteering = VELOCITY_STEERING_COEFFICIENT;

                // Kart yere indiğinde drift başlat (eğer yönü hızından farklıysa)
                if (GroundPercent >= 0.0f && m_PreviousGroundPercent < DRIFT_GROUNDED_THRESHOLD)
                {
                    Vector3 flattenVelocity = Vector3.ProjectOnPlane(Rigidbody.linearVelocity, m_VerticalReference).normalized;
                    if (Vector3.Dot(flattenVelocity, transform.forward * Mathf.Sign(accelInput)) < Mathf.Cos(MinAngleToFinishDrift * Mathf.Deg2Rad))
                    {
                        IsDrifting = true;
                        m_CurrentGrip = DriftGrip;
                        m_DriftTurningPower = 0.0f;
                    }
                }

                // Drift Management
                if (!IsDrifting)
                {
                    if ((WantsToDrift || isBraking) && currentSpeed > maxSpeed * MinSpeedPercentToFinishDrift)
                    {
                        IsDrifting = true;
                        m_DriftTurningPower = turningPower + (Mathf.Sign(turningPower) * DriftAdditionalSteer);
                        m_CurrentGrip = DriftGrip;

                        ActivateDriftVFX(true);
                    }
                }

                if (IsDrifting)
                {
                    float turnInputAbs = Mathf.Abs(turnInput);
                    if (turnInputAbs < k_NullInput)
                        m_DriftTurningPower = Mathf.MoveTowards(m_DriftTurningPower, 0.0f, Mathf.Clamp01(DriftDampening * Time.fixedDeltaTime));

                    // Update the turning power based on input
                    float driftMaxSteerValue = m_FinalStats.Steer + DriftAdditionalSteer;
                    m_DriftTurningPower = Mathf.Clamp(m_DriftTurningPower + (turnInput * Mathf.Clamp01(DriftControl * Time.fixedDeltaTime)), -driftMaxSteerValue, driftMaxSteerValue);

                    bool facingVelocity = Vector3.Dot(Rigidbody.linearVelocity.normalized, transform.forward * Mathf.Sign(accelInput)) > Mathf.Cos(MinAngleToFinishDrift * Mathf.Deg2Rad);

                    bool canEndDrift = true;
                    if (isBraking)
                        canEndDrift = false;
                    else if (!facingVelocity)
                        canEndDrift = false;
                    else if (turnInputAbs >= k_NullInput && currentSpeed > maxSpeed * MinSpeedPercentToFinishDrift)
                        canEndDrift = false;

                    if (canEndDrift || currentSpeed < k_NullSpeed)
                    {
                        // No Input, and car aligned with speed direction => Stop the drift
                        IsDrifting = false;
                        m_CurrentGrip = m_FinalStats.Grip;
                    }

                }

                // Hız vektörünü döndürüp steer değerine göre rotasyon yap
                Rigidbody.linearVelocity = Quaternion.AngleAxis(turningPower * Mathf.Sign(localVel.z) * velocitySteering * m_CurrentGrip * Time.fixedDeltaTime, transform.up) * Rigidbody.linearVelocity;
            }
            else
            {
                m_InAir = true;
            }

            // Yerden yüksekliği kontrol etmek için raycast
            bool validPosition = false;
            const float RAYCAST_VERTICAL_OFFSET = 0.1f;
            const int RAYCAST_LAYER_MASK = (1 << 9) | (1 << 10) | (1 << 11); // Ground(9) / Environment(10) / Track(11)
            
            if (Physics.Raycast(transform.position + (transform.up * RAYCAST_VERTICAL_OFFSET), -transform.up, out RaycastHit hit, RAYCST_DISTANCE, RAYCAST_LAYER_MASK))
            {
                Vector3 lerpVector = (m_HasCollision && m_LastCollisionNormal.y > hit.normal.y) ? m_LastCollisionNormal : hit.normal;
                float blendSpeed = (GroundPercent > 0.0f) ? AIRBORNE_REORIENTATION_MULTIPLIER : 1.0f;
                m_VerticalReference = Vector3.Slerp(m_VerticalReference, lerpVector, Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime * blendSpeed));
            }
            else
            {
                Vector3 lerpVector = (m_HasCollision && m_LastCollisionNormal.y > 0.0f) ? m_LastCollisionNormal : Vector3.up;
                m_VerticalReference = Vector3.Slerp(m_VerticalReference, lerpVector, Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime));
            }

            const float ORIENTATION_DOT_THRESHOLD = 0.9f;
            validPosition = GroundPercent > HALF_GROUNDED_THRESHOLD && !m_HasCollision && Vector3.Dot(m_VerticalReference, Vector3.up) > ORIENTATION_DOT_THRESHOLD;

            // Havada veya yarı yerde management
            if (GroundPercent < HALF_GROUNDED_THRESHOLD)
            {
                Rigidbody.angularVelocity = new Vector3(0.0f, Rigidbody.angularVelocity.y * AIRBORNE_ANGULAR_VELOCITY_DAMPEN, 0.0f);
                Vector3 finalOrientationDirection = Vector3.ProjectOnPlane(transform.forward, m_VerticalReference);
                finalOrientationDirection.Normalize();
                if (finalOrientationDirection.sqrMagnitude > 0.0f)
                {
                    Rigidbody.MoveRotation(Quaternion.Lerp(Rigidbody.rotation, Quaternion.LookRotation(finalOrientationDirection, m_VerticalReference), Mathf.Clamp01(AirborneReorientationCoefficient * Time.fixedDeltaTime)));
                }
            }
            else if (validPosition)
            {
                m_LastValidPosition = transform.position;
                m_LastValidRotation.eulerAngles = new Vector3(0.0f, transform.rotation.y, 0.0f);
            }

            ActivateDriftVFX(IsDrifting && GroundPercent > 0.0f);
        }

        IEnumerator FadeOutAndDestroyVFX(GameObject vfxObject, float fadeDuration)
        {
            if (vfxObject == null)
                yield break;

            ParticleSystem ps = vfxObject.GetComponent<ParticleSystem>();
            
            // Particle sisteminin yayınını durdur
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            // Fade süresi boyunca bekle
            yield return new WaitForSeconds(fadeDuration);

            // Sonra VFX'i yok et
            m_IsDestroyingNitroVFX = false;
            Destroy(vfxObject);
        }
        /// <summary>
        /// Kartın arkasından havaya doğru muz kabuğu fırlatır.
        /// </summary>
        public void DropBananaPeel()
        {
            if (bananaPrefab != null && bananaSpawnPoint != null)
            {
                // Spawn point'in 1 metre arkasındaki pozisyonu hesapla (-transform.forward)
                Vector3 spawnPosition = bananaSpawnPoint.position - (transform.forward * 1.0f);
                
                // Muzu anında o pozisyonda ve kartın o anki rotasyonuna paralel olarak doğur
                GameObject bananaInstance = Instantiate(bananaPrefab, spawnPosition, transform.rotation);
                
                // Eğer sahnede ölçeklenme sorunu oluyorsa boyutunu eşitle (isteğe bağlı)
                bananaInstance.transform.localScale = Vector3.one * 1f;
            }
        }

        /// <summary>
        /// Muza basıldığında çağrılan spin başlatıcı.
        /// </summary>
        public void SpinOut(float duration)
        {
            if (IsSpinning) return; 

            IsSpinning = true;
            m_SpinDuration = duration;
            m_SpinElapsedTime = 0.0f;

            // Cezalandırma: Nitro ve drift iptal
            IsNitroActive = false; 
            IsDrifting = false;
            ActivateDriftVFX(false);
        }

        /// <summary>
        /// Muza basan kartın yavaşlayıp fırfır dönmesini sağlar.
        /// </summary>
        private void ApplySpinMovement()
        {
            m_SpinElapsedTime += Time.fixedDeltaTime;

            if (m_SpinElapsedTime >= m_SpinDuration)
            {
                IsSpinning = false;
                return;
            }

            // Kartı kademeli olarak durdur (Sürtünmeyi artırdık)
            Vector3 newVelocity = Vector3.MoveTowards(Rigidbody.linearVelocity, new Vector3(0, Rigidbody.linearVelocity.y, 0), Time.fixedDeltaTime * m_FinalStats.CoastingDrag * 2.0f);
            Rigidbody.linearVelocity = newVelocity;

            // Kendi etrafında tatlı bir Mario Kart spini (360 derece)
            float rotationThisFrame = SPIN_SPEED_DEG_PER_SEC * Time.fixedDeltaTime;
            Quaternion deltaRotation = Quaternion.Euler(0f, rotationThisFrame, 0f);
            Rigidbody.MoveRotation(Rigidbody.rotation * deltaRotation);
        }
        /// <summary>
        /// Kart eğer yerdeyse dikey eksende yukarı doğru zıplamasını sağlar.
        /// </summary>
        /// <summary>
        /// Biriken şarj miktarına göre kartı yukarı doğru fırlatır.
        /// </summary>
        private void ExecuteChargeJump()
        {
            m_IsChargingJump = false;

            // Kart hala yerdeyse zıpla (Şarj ederken havaya uçtuysa zıplamasın)
            if (GroundPercent > 0.1f && !IsSpinning && m_CanMove)
            {
                // Min ve Max kuvvetler arasında şarj yüzdesine göre nihai kuvveti hesapla
                float finalJumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, m_CurrentJumpCharge);

                // Dikey hızı sıfırla ki stabil fırlasın
                Vector3 currentVel = Rigidbody.linearVelocity;
                currentVel.y = 0f;
                Rigidbody.linearVelocity = currentVel;

                // Yukarı doğru fırlat
                Rigidbody.AddForce(Vector3.up * finalJumpForce, ForceMode.Impulse);
                Rigidbody.angularVelocity = new Vector3(0f, Rigidbody.angularVelocity.y, 0f);
            }

            // Süspansiyon görselini eski haline (normal yüksekliğe) sıfırla
            WheelsPositionVerticalOffset = 0f;
            m_CurrentJumpCharge = 0f;
        }
    }  
    
}

