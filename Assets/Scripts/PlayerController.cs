using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks, IDamageable
{
    [SerializeField] GameObject cameraHolder;
    [SerializeField] float mouseSensitivity, sprintSpeed, walkSpeed, jumpForce, smoothTime;
    [SerializeField] Gun[] guns;
    [SerializeField] TMP_Text healthbarText;
    [SerializeField] TMP_Text aim;
    [SerializeField] TMP_Text ammoInfo;
    [SerializeField] GameObject ui;
    [SerializeField] Image gunImage;
    int itemIndex;
    int previousItemIndex = -1;
    public static float verticalLookRotation;
    float showingHit = 0;
    bool grounded = true;
    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;
    Rigidbody rb;
    PhotonView PV;
    PlayerManager playerManager;
    const float maxHealth = 100f;
    float currentHealth = maxHealth;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>();
        aim.color = Color.white;
        aim.outlineColor = Color.black;
        aim.outlineWidth = 0.2f;
    }

    void Update()
    {
        if (!PV.IsMine)
            return;
        if (PauseMenu.isPaused)
        {
            aim.alpha = 0;
            return;
        }
        aim.alpha = 190;
        Look();
        Move();
        Jump();
        Equip();
        if(transform.position.y < -10f)
        {
            Die();
        }

        Gun s = guns[itemIndex];
        if (s.Use(PhotonNetwork.LocalPlayer.ActorNumber))
        {
            showingHit = 0.2f;
        }
        if (showingHit > 0)
        {
            aim.color = Color.red;
            aim.outlineColor = Color.red;
            showingHit -= Time.deltaTime;
        }
        else
        {
            aim.color = Color.white;
            aim.outlineColor = Color.black;
        }
        ammoInfo.text = s.currentLoadedAmmo.ToString();
        if (s.ammoInStock > 0)
        {
            ammoInfo.text += " | " + (s.ammoInStock).ToString();
        }
    }

    void Start()
    {
        if (PV.IsMine)
        {
            EquipItem(0);
            verticalLookRotation = 0f;
        }
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
            Destroy(ui);
        }
    }

    void Equip()
    {
        for (int i = 0; i < guns.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);
                break;
            }
        }
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            EquipItem((itemIndex + 1) % guns.Length);
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            EquipItem((itemIndex + guns.Length - 1) % guns.Length);
        }
    }

    void Move()
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddForce(transform.up * jumpForce);
        }
    }

    void Look()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);
        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    void EquipItem(int _itemIndex)
    {
        if (_itemIndex == previousItemIndex)
            return;
        itemIndex = _itemIndex;
        GunInfo g = (GunInfo)(guns[itemIndex]).itemInfo;
        if (gunImage != null) gunImage.sprite = g.gunIcon;
        if (ammoInfo != null) ammoInfo.rectTransform.offsetMax = new Vector2(-g.offset, 0);
        guns[itemIndex].itemGameObject.SetActive(true);
        if(previousItemIndex != -1)
        {
            guns[previousItemIndex].itemGameObject.SetActive(false);
        }

        previousItemIndex = itemIndex;
        if (PV.IsMine)
        {
            Hashtable hash = new Hashtable();
            hash.Add("itemIndex", itemIndex);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    public void SetGroundedState(bool _grounded)
    {
        grounded = _grounded;
    }
    void FixedUpdate()
    {
        if (!PV.IsMine)
            return;
        if (PauseMenu.isPaused)
            return;

        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("itemIndex") && !PV.IsMine && targetPlayer == PV.Owner)
        {
            EquipItem((int)changedProps["itemIndex"]);
        }
    }

    public void TakeDamage(float damage, int id)
    {
        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage, id);
    }

    [PunRPC]
    void RPC_TakeDamage(float damage, int id)
    {
        if (!PV.IsMine)
            return;
        //Debug.Log("Took damage: " + damage);
        currentHealth -= damage;
        //Debug.Log(currentHealth + " ");
        healthbarText.text = "H " + ((currentHealth > (int)currentHealth) ? ((int)currentHealth + 1).ToString() : (currentHealth).ToString());
        if(currentHealth <= 0)
        {
            Die();
            playerManager.kill(id);
        }
    }

    void Die()
    {
        playerManager.Die();
    }
}
