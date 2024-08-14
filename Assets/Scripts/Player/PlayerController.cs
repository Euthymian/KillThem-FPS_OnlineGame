using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    [SerializeField] GameObject ui;
    [SerializeField] TMP_Text fpsText;
    [SerializeField] TMP_Text speedText;
    const float maxHealth = 100;
    float curreHealth = maxHealth;
    [SerializeField] Image healthBarImage;
    [SerializeField] UnityEvent HealthBarShrinkEvent;

    PlayerManager playerManager;

    [Header("Camera")]
    [SerializeField] GameObject cameraHolder;
    float verticalLookRotation;
    [SerializeField] float mouseSensetivity;

    [Header("Movement by Vector3.SmoothDamp")]
    [SerializeField] float sprintSpeed;
    [SerializeField] float walkSpeed;
    [SerializeField] float smoothTime;
    Vector3 smoothMoveVeclocity;
    Vector3 moveAmount;

    [Header("Movement")]
    [SerializeField] float moveSpeed;
    [SerializeField] float groundDrag;
    float horizontalInput, verticalInput;
    Vector3 moveDir;
    [HideInInspector] public bool needFreezeMove;

    [Header("SlopeMovement")]
    [SerializeField] LayerMask slopeLayerMask;
    [SerializeField] float maxSlopeAngle;
    RaycastHit slopeHit;
    bool exitingSlope;

    [Header("GrappleMovement")]
    [SerializeField] bool activeGrapple;
    Vector3 grappleVel;
    bool enableMovementOnNextTouch;

    [Header("Jump")]
    [SerializeField] float jumpForce;
    [SerializeField] float jumpCooldown;
    bool readyToJump = true;
    bool grounded;

    [Header("Gun")]
    [SerializeField] Item[] items;
    [SerializeField] GameObject[] itemsUI;
    [SerializeField] TMP_Text maxAmmoText;
    [SerializeField] TMP_Text currentAmmoText;
    int currentItemIndex = 0, previousItemIndex = -1;

    [SerializeField] Animator rifleAnim;
    bool onScope=false;

    [Header("Key Bindings")]
    KeyCode reloadKey = KeyCode.R;
    KeyCode jumpKey = KeyCode.Space;
    KeyCode sprintKey = KeyCode.LeftShift;

    Rigidbody rb;
    PhotonView pv;

    bool firstTimeCalled = true;
    bool equipRifleFirstTime = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pv = GetComponent<PhotonView>();
        playerManager = PhotonView.Find((int)pv.InstantiationData[0]).GetComponent<PlayerManager>();
    }

    private void Start()
    {
        if (pv.IsMine)
        {
            EquipItem(currentItemIndex);
            InvokeRepeating(nameof(GetFPS), 1, 0.1f);
            //InvokeRepeating(nameof(GetSpeed), 1, 0.1f);
        }
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
            Destroy(ui);
        }
    }

    void GetFPS()
    {
        string fps = ((int)(1 / Time.unscaledDeltaTime)).ToString();
        fpsText.text = $"FPS: {fps}";
    }

    void GetSpeed()
    {
        string speed = Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.z + rb.velocity.z).ToString();
        speedText.text = $"Speed: {speed}";
    }

    private void Update()
    {
        if (!pv.IsMine)
            return;

        Look();

        //Move();
        MyInput();
        ApplyGroundGrag();
        SpeedControl();
        UpdateSprint();

        Jump();
        FalTillDielCheck();

        SwitchGun();
        UseGun();
        Reload();
        ReloadUI();
        ChangeScopeStatus();
    }

    private void FixedUpdate()
    {
        if (!pv.IsMine)
            return;
        //rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
        MovePlayer();
    }

    #region Camera

    void Look()
    {
        Vector3 horizontalRotate = Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensetivity * Time.deltaTime;
        transform.Rotate(horizontalRotate);

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensetivity * Time.deltaTime;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    #endregion

    #region Move

    // Using this movement method by uncomment "rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);" in FixedUpdate
    // and Move() in Update
    void Move()
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVeclocity, smoothTime);
    }

    void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    void MovePlayer()
    {
        if (needFreezeMove || activeGrapple)
        {
            rb.useGravity = true;
            return;
        }

        moveDir = transform.forward * verticalInput + transform.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            // Add force to player so player no longer performs wierd boucing while move up slope
            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else
            rb.AddForce(moveDir.normalized * moveSpeed * 10, ForceMode.Force);

        // Turn off gravily while on slope to deny slowly sliding down
        rb.useGravity = !OnSlope();
    }

    void ApplyGroundGrag()
    {
        if (grounded && !activeGrapple)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    void SpeedControl()
    {
        if (activeGrapple) return;

        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    void UpdateSprint()
    {
        if (Input.GetKeyDown(sprintKey))
            moveSpeed *= 2;
        else if (Input.GetKeyUp(sprintKey))
            moveSpeed /= 2;
    }

    // Slope handler
    bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, transform.gameObject.GetComponent<CapsuleCollider>().height * 0.5f + 0.3f, slopeLayerMask))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
    }

    #endregion

    #region Grapple 

    public void JumpToPos(Vector3 targetPos, float trajectoryHeight)
    {
        activeGrapple = true;

        grappleVel = CalculateJumpVelocity(transform.position, targetPos, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestrictions), 2.5f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            GetComponent<Grappling>().StopGrapple();
        }
    }

    void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.velocity = grappleVel;
    }

    void ResetRestrictions()
    {
        activeGrapple = false;
    }

    Vector3 CalculateJumpVelocity(Vector3 startPos, Vector3 endPos, float trajactoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPos.y - startPos.y;
        Vector3 displacementXZ = new Vector3(endPos.x - startPos.x, 0, endPos.z - startPos.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajactoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajactoryHeight / gravity) + Mathf.Sqrt(2 * (displacementY - trajactoryHeight) / gravity));

        return velocityXZ + velocityY;
    }

    #endregion

    #region Jump and Fall

    void Jump()
    {
        if (Input.GetKey(jumpKey) && grounded && readyToJump)
        {
            exitingSlope = true;
            readyToJump = false;
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    void FalTillDielCheck()
    {
        if (transform.position.y < -10)
        {
            Die();
        }
    }

    public void SetGroundedState(bool _grounded)
    {
        grounded = _grounded;
    }

    #endregion

    #region Gun

    void EquipItem(int _index)
    {
        if (_index == previousItemIndex)
            return;

        currentItemIndex = _index;
        items[currentItemIndex].itemGameObject.SetActive(true);

        if (((Gun)items[currentItemIndex]).itemInfo.itemName == "Rifle" && equipRifleFirstTime && !pv.IsMine)
        {
            equipRifleFirstTime = false;
            Destroy(items[currentItemIndex].GetComponentInChildren<Camera>());
        }

        // Update GunUI
        if (pv.IsMine)
        {
            Image gunIcon = itemsUI[currentItemIndex].GetComponentInChildren<Image>();
            gunIcon.rectTransform.localScale = new Vector3(2, 2, 2);
            gunIcon.color = new Color(gunIcon.color.r, gunIcon.color.g, gunIcon.color.b, 1);
        }

        if (previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
            // Update GunUI
            if (pv.IsMine)
            {
                Image gunIcon = itemsUI[previousItemIndex].GetComponentInChildren<Image>();
                gunIcon.rectTransform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
                gunIcon.color = new Color(gunIcon.color.r, gunIcon.color.g, gunIcon.color.b, 0.4f);
            }
        }
        previousItemIndex = currentItemIndex;

        UpdateAmmoUI();

        if (pv.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("CurrentItemIndex", currentItemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("CurrentItemIndex") && !pv.IsMine && targetPlayer == pv.Owner)
        {
            EquipItem((int)changedProps["CurrentItemIndex"]);
        }
    }

    void SwitchGun()
    {
        // Switch by press number keys
        for (int i = 0; i < items.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);
                break;
            }
        }

        // Switch by scroll mouse
        if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
        {
            int afterScrollIndex = currentItemIndex + 1;

            if (afterScrollIndex >= items.Length)
                afterScrollIndex = 0;

            EquipItem(afterScrollIndex);
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") > 0)
        {
            int afterScrollIndex = currentItemIndex - 1;

            if (afterScrollIndex < 0)
                afterScrollIndex = items.Length - 1;

            EquipItem(afterScrollIndex);
        }
    }

    public void UpdateAmmoUI()
    {
        if (!pv.IsMine)
            return;

        maxAmmoText.text = ((GunInfo)items[currentItemIndex].itemInfo).ammoCapacity.ToString();
        currentAmmoText.text = ((Gun)items[currentItemIndex]).CurrentAmmos.ToString();
        // Need firstTimeCalled because when the UpdateAmmoUI was called 1st time, the gun hadnt been init yet, so the current ammo would be 0
        if (firstTimeCalled)
        {
            firstTimeCalled = false;
            currentAmmoText.text = ((GunInfo)items[currentItemIndex].itemInfo).ammoCapacity.ToString();
        }
    }

    void UseGun()
    {
        if (
            ((items[currentItemIndex].GetType() == typeof(SingleShotGun) || items[currentItemIndex].GetType() == typeof(ShotgunType)) && Input.GetMouseButtonDown(0)) ||
            (items[currentItemIndex].GetType() == typeof(MultipleShotGun) && Input.GetMouseButton(0))
            )
        {
            items[currentItemIndex].Use();
            UpdateAmmoUI();
        }
    }

    void Reload()
    {
        if (Input.GetKeyDown(reloadKey))
            ((Gun)items[currentItemIndex]).Reload();
    }

    void ReloadUI()
    {
        for (int i = 0; i < items.Length; i++)
        {
            Gun gun = (Gun)items[i];

            if (!gun.WasReloading && gun.IsReloading)
                gun.currentReloadTime = 0;

            if (gun.IsReloading)
            {
                gun.currentReloadTime += Time.deltaTime;
                itemsUI[i].GetComponentsInChildren<Image>()[1].fillAmount = gun.currentReloadTime / gun.GunInfor.reloadTime;

                if (gun.currentReloadTime / gun.GunInfor.reloadTime >= 1)
                {
                    itemsUI[i].GetComponentsInChildren<Image>()[1].fillAmount = 0;
                }
            }

            gun.WasReloading = gun.IsReloading;
        }
    }

    void ChangeScopeStatus()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (((Gun)items[currentItemIndex]).GunInfor.hasScope)
            {
                if (!onScope)
                {
                    onScope = true;
                    rifleAnim.SetTrigger("OnScope");
                }
                else
                {
                    onScope = false;
                    rifleAnim.SetTrigger("OffScope");
                }
            }
        }
    }

    #endregion

    #region TakeDamage and Die

    public void TakeDamage(float damage)
    {
        pv.RPC(nameof(RPC_TakeDamage), pv.Owner, damage);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage, PhotonMessageInfo info)
    {
        //Debug.Log($"Take {damage} damage");
        curreHealth -= damage;

        healthBarImage.fillAmount = curreHealth / maxHealth;
        HealthBarShrinkEvent.Invoke();

        if (curreHealth <= 0)
        {
            Die();
            PlayerManager.Find(info.Sender).GetKill();
        }
    }

    private void Die()
    {
        playerManager.Die();
    }

    #endregion

}