using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Random = UnityEngine.Random;

public class PlayerController2 : NetworkBehaviour
{
    public NetworkVariable<NetworkString> playerName = new NetworkVariable<NetworkString>("",NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public string PlayerName => playerName.Value;

    private NetManager netManager;
    
    [SerializeField] Transform cam;
    [SerializeField] Transform leftFoot;
    [SerializeField] Transform rightFoot;
    [SerializeField] ProceduralLegsController proceduralLegs;
    [SerializeField] Rigidbody headRb;
    [SerializeField] float feetGroundCheckDist;
    [SerializeField] ConfigurableJoint hipsCj;
    [SerializeField] Rigidbody hipsRb;

    LayerMask groundMask;

    [SerializeField] float moveSpeed;
    [SerializeField] float rotationForce;
    [SerializeField] float balanceForce;

    [SerializeField] private int maxJumps = 2;
    [SerializeField] private float jumpCoolDown = 5f;
    private int currentJumps = 0;
    private float lastJumpTime = 0f;
    [SerializeField] float jumpForce;
    [SerializeField] float speedMultiplier = 1.5f;
    [SerializeField] float maxVelocityChange;

    public Transform torso;
    public float mouseX;
    public float mouseY;
    public float rotationSpeed = 5f;
    private float yaw = 0f;
    private float pitch = 0f;

    bool isGrounded;
    bool isDead = false;
    
    [SerializeField] private HealthBar healthBar;
    private NetworkVariable<float> currentHealth = new NetworkVariable<float>(100f); 
    private float predictedHealth;
    
    float horizontal, vertical;
    public int score = 0;
    
    [SerializeField] ConfigurableJoint[] cjs;
    JointDrive[] jds;
    JointDrive inAirDrive;
    JointDrive hipsInAirDrive;

    [SerializeField] float airSpring;

    
   
   
    public override void OnNetworkSpawn()
    {
        
        if (IsOwner)
        {
            playerName.Value = NameInputHandler.PlayerName;  // Ensure this is the correct way to get the player's name
        }

        Debug.Log(NameInputHandler.PlayerName);
       
    
        if (!IsOwner)
        {
            enabled = false;
            return;
        }
        
        // Find the NetManager instance
        netManager = FindObjectOfType<NetManager>();
        
        // Set random spawn point
        Transform spawnPoint = SpawnManager.Instance.spawnPoints[Random.Range(0, SpawnManager.Instance.spawnPoints.Count)];
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        InitializeComponents();
    }

    


    private void Start()
    {
        if (IsOwner)
        {
          //  playerName.Value = $"Player {OwnerClientId}";
            InitializeComponents();
            healthBar = GetComponentInChildren<HealthBar>();
        }
    }

    void InitializeComponents()
    {
        jds = new JointDrive[cjs.Length];

        inAirDrive.maximumForce = Mathf.Infinity;
        inAirDrive.positionSpring = airSpring;

        hipsInAirDrive.maximumForce = Mathf.Infinity;
        hipsInAirDrive.positionSpring = 0;

        if (hipsRb == null)
        {
            hipsRb = GetComponent<Rigidbody>();
        }

        if (hipsCj == null)
        {
            hipsCj = GetComponent<ConfigurableJoint>();
        }

        for (int i = 0; i < cjs.Length; i++)
        {
            jds[i] = cjs[i].angularXDrive;
        }

        groundMask = LayerMask.GetMask("Ground");

        currentHealth.OnValueChanged += OnHealthChanged;
        playerName.OnValueChanged += OnPlayerNameChanged;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetHealthServerRpc(float health)
    {
        currentHealth.Value = health;
    }
    void OnHealthChanged(float oldValue, float newValue)
    {
        if (IsOwner && healthBar != null)
        {
            healthBar.UpdateHealthBar(newValue);
        }
    }

    void OnPlayerNameChanged(NetworkString oldName, NetworkString newName)
    {
        // Handle any UI updates or other logic when the player name changes
    }

    void SetPlayerInputs()
    {
        // Get mouse input
        mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
    }

    void Update()
    {
        if (IsOwner && healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth.Value);
        }
        if (Input.GetKeyDown(KeyCode.V))
            StabilizeBody();
        if (!isDead && IsOwner)
        {
            proceduralLegs.GroundHomeParent();
            CheckGrounded();
            SetPlayerInputs();

            DoubleJump();
            if (isGrounded)
            {
                currentJumps = 0;
            }
        }
    }

    void FixedUpdate()
    {
        if (isGrounded && !isDead && IsOwner)
        {
            StabilizeBody();
            Move();
        }
    }

    void Move()
    {
        // Update yaw and pitch
        yaw += mouseX * rotationSpeed;
        pitch -= mouseY * rotationSpeed;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        // Rotate torso based on mouse input
        torso.localRotation = Quaternion.Euler(pitch, yaw, 0f);

        // Calculate movement direction based on camera's forward direction
        Vector3 moveDirection = new Vector3(horizontal, 0f, vertical).normalized;
        Vector3 rotatedMoveDirection = cam.TransformDirection(moveDirection);
        rotatedMoveDirection.y = 0f; // Keep movement strictly horizontal

        // If there's any movement input
        if (moveDirection.magnitude > 0.1f)
        {
            // Calculate target rotation
            Quaternion targetRotation = Quaternion.LookRotation(rotatedMoveDirection);

            // Rotate the character smoothly towards the target rotation
            hipsRb.MoveRotation(Quaternion.RotateTowards(hipsRb.rotation, targetRotation,
                rotationSpeed * Time.fixedDeltaTime));

            // Calculate and apply movement force
            Vector3 targetVelocity = rotatedMoveDirection * moveSpeed;
            Vector3 velocityChange = targetVelocity - hipsRb.velocity;
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0; // Prevent vertical velocity change

            hipsRb.AddForce(velocityChange, ForceMode.VelocityChange);
        }

        // Apply balance forces
        headRb.AddForce(Vector3.up * balanceForce);
        hipsRb.AddForce(Vector3.down * balanceForce);

        // Adjust speed if running
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftShift))
        {
            Vector3 runVelocity = rotatedMoveDirection * (moveSpeed * speedMultiplier);
            Vector3 runVelocityChange = runVelocity - hipsRb.velocity;
            runVelocityChange.x = Mathf.Clamp(runVelocityChange.x, -maxVelocityChange, maxVelocityChange);
            runVelocityChange.z = Mathf.Clamp(runVelocityChange.z, -maxVelocityChange, maxVelocityChange);
            runVelocityChange.y = 0;

            hipsRb.AddForce(runVelocityChange, ForceMode.VelocityChange);
        }
    }

    void StabilizeBody()
    {
        headRb.AddForce(Vector3.up * balanceForce);
        hipsRb.AddForce(Vector3.down * balanceForce);
    }

    void CheckGrounded()
    {
        bool leftCheck = false;
        bool rightCheck = false;
        RaycastHit hit;

        if (Physics.Raycast(leftFoot.position, Vector3.down, out hit, feetGroundCheckDist, groundMask))
            leftCheck = true;

        if (Physics.Raycast(rightFoot.position, Vector3.down, out hit, feetGroundCheckDist, groundMask))
            rightCheck = true;

        if ((rightCheck || leftCheck) && !isGrounded)
        {
            SetDrives();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ApplyDamageServerRpc(ulong killerId, float damage)
    {
        currentHealth.Value -= damage;
        Debug.Log($"[Server] Damage applied: {damage}, Current health: {currentHealth.Value}");

        if (currentHealth.Value <= 0 && !isDead)
        {
            DieServerRpc(killerId, true);
        }
    }

    public void ApplyDamage(ulong attackerId, float damage)
    {
        if (!IsServer)
        {
            ApplyDamageServerRpc(attackerId, damage);
            return;
        }
        currentHealth.Value -= damage;
       
        Debug.Log($"Damage applied: {damage}, Current health: {currentHealth.Value}");

        if (currentHealth.Value <= 0 && !isDead)
        {
            DieServerRpc(attackerId, true);
            
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void DieServerRpc(ulong killerId, bool respawn)
    {
        DieClientRpc(respawn:true);
        isDead = true;
        foreach (ConfigurableJoint cj in cjs)
        {
            cj.angularXDrive = inAirDrive;
            cj.angularYZDrive = inAirDrive;
        }

        hipsCj.angularYZDrive = hipsInAirDrive;
        hipsCj.angularXDrive = hipsInAirDrive;
        isGrounded = false;
        proceduralLegs.DisableIk();

        // Update kill feed and respawn if needed
        UpdateKillFeedServerRpc(killerId ,OwnerClientId);
        if (respawn)
        {
            StartCoroutine(RespawnCoroutine(5f)); // Respawn after 5 seconds
        }
    }

    [ClientRpc]
    private void DieClientRpc(bool respawn)
    {
        if (respawn)
        {
            isDead = true;
            foreach (ConfigurableJoint cj in cjs)
            {
                cj.angularXDrive = inAirDrive;
                cj.angularYZDrive = inAirDrive;
            }

            hipsCj.angularYZDrive = hipsInAirDrive;
            hipsCj.angularXDrive = hipsInAirDrive;
            isGrounded = false;
            proceduralLegs.DisableIk();

            StartCoroutine(RespawnCoroutine(5f)); // Start the respawn coroutine
        }
    }

    private IEnumerator RespawnCoroutine(float respawnTime)
    {
        yield return new WaitForSeconds(respawnTime);
        Transform spawnPoint = SpawnManager.Instance.GetRandomSpawnPoint();
       
        if (spawnPoint)
        {
            Debug.Log(spawnPoint.position);
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
            
            // Reset player state here (health, position, etc.)
            SetHealthServerRpc(100f);
            isDead = false;
            isGrounded = true;
            proceduralLegs.EnableIk();
            SetDrives();
            if (healthBar != null)
                healthBar.UpdateHealthBar(currentHealth.Value);
        }
    }

   

    [ServerRpc(RequireOwnership = false)]
    public void UpdateKillFeedServerRpc(ulong killerId, ulong victimId)
    {
        if (netManager == null)
        {
            netManager = FindObjectOfType<NetManager>();
        }

        string killerName = netManager.GetPlayerName(killerId);
        string victimName = netManager.GetPlayerName(victimId);

        Debug.Log($"Killer: {killerName}, Victim: {victimName}");
        UpdateKillFeedClientRpc(killerName, victimName);
    }

    [ClientRpc]
    private void UpdateKillFeedClientRpc(string killerName, string victimName)
    {
        Debug.Log($"ClientRPC received: {killerName} killed {victimName}");
        KillFeedManager.Instance.AddKillFeedItem(killerName, victimName);
    }

 //   private string GetPlayerName(ulong playerId)
 //   {
 //       if (NetworkManager.Singleton.ConnectedClients.TryGetValue(playerId, out var client))
 //       {
 //           var playerController = client.PlayerObject.GetComponent<PlayerController2>();
 //           return playerController != null ? playerController.PlayerName : "Unknown";
 //       }
 //       return "Unknown";
 //   }

    public void IncreaseScore()
    {
        score += 1;
        UpdateScoreClientRpc(score);
    }

    [ClientRpc]
    public void UpdateScoreClientRpc(int newScore)
    {
        score = newScore;
        // Update the score UI here
    }

    void DoubleJump()
    {
        if (!isDead && Time.time - lastJumpTime >= jumpCoolDown && currentJumps < maxJumps &&
            Input.GetKeyDown(KeyCode.Space))
        {
            hipsRb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
            hipsRb.AddTorque(new Vector3(750, 0));

            currentJumps++;
            lastJumpTime = Time.time;
            if (currentJumps >= maxJumps)
            {
                StartCoroutine(JumpCoolDownRoutine());
            }
        }

        IEnumerator JumpCoolDownRoutine()
        {
            yield return new WaitForSeconds(jumpCoolDown);
            currentJumps = 0;
        }
    }

    void SetDrives()
    {
        if (IsOwner)
        {
            
        
            for (int i = 0; i < cjs.Length; i++)
            {
                cjs[i].angularXDrive = jds[i];
                cjs[i].angularYZDrive = jds[i];
            }
            proceduralLegs.EnableIk();
            isGrounded = true;
        }
    }
}
