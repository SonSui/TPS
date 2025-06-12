using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f;             // 移動速度
    public float jumpHeight = 2f;            // ジャンプの高さ（未使用）
    public float gravity = -9.81f;           // 重力加速度
    public float rotationSpeed = 360f;       // 回転速度（度／秒）

    [Header("接地判定")]
    public Transform groundCheck;            // 地面判定のためのTransform
    public float groundDistance = 0.4f;      // 接地チェックの半径
    public LayerMask groundMask;             // 地面として判定されるレイヤー

    [Header("武器関連")]
    public GameObject weaponBox;             // 武器の親オブジェクト
    public List<GameObject> weapons;         // 所持武器のリスト
    private int currentWeaponIndex = 0;      // 現在の武器インデックス

    [Header("プレイヤーモデル")]
    public GameObject playerModel;           // プレイヤーモデルのGameObject
    public Animator playerAnimator;       // プレイヤーのアニメーションコンポーネント


    private CharacterController controller;  // CharacterControllerの参照
    private Vector3 velocity;                // 垂直方向の速度
    private bool isGrounded;                 // 接地しているかどうか
    private Transform mainCam;               // メインカメラのTransform

    // プレイヤーの状態
    enum PlayerState
    {
        Idle,       // 待機
        Walking,    // 歩き
        Running,    // 走り
        Jumping,    // ジャンプ中
        Falling     // 落下中
    };
    PlayerState currentState = PlayerState.Idle;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCam = Camera.main.transform; // メインカメラを取得
    }

    void Update()
    {
        // 地面に接しているか判定
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            // 地面に押し付けるようにY速度をリセット
            velocity.y = -2f;
        }

        // 入力取得（方向キー/WASD）
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(inputX, 0f, inputZ).normalized;

        // カメラの向きを基準に移動方向を計算
        Vector3 camForward = mainCam.forward;
        Vector3 camRight = mainCam.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // カメラ基準の移動方向ベクトル
        Vector3 moveDir = camRight * inputX + camForward * inputZ;
        moveDir.Normalize();

        // 移動処理
        if (moveDir.magnitude >= 0.1f)
        {
            // 現在の向きから目標方向へスムーズに回転
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // キャラクターを移動させる
            controller.Move(moveDir * moveSpeed * Time.deltaTime);
            currentState = PlayerState.Walking;
            playerAnimator.SetBool("Move", true);
        }
        else
        {
            currentState = PlayerState.Idle;
            playerAnimator.SetBool("Move", false);
        }

        if(Input.GetButtonDown("Fire1"))
        {
            // 攻撃アニメーションを再生
            playerAnimator.SetTrigger("Attack");
        }
        if (Input.GetButtonDown("Fire2"))
        {
            int weaponCount = weapons.Count;
            currentWeaponIndex = (currentWeaponIndex + 1) % weaponCount; // 武器を切り替え
            if (weaponBox.transform.childCount > 0)
            {
                Transform oldWeapon = weaponBox.transform.GetChild(0);
                if (oldWeapon != null)
                {
                    Destroy(oldWeapon.gameObject); // 古い武器を削除
                }
            }
            if (currentWeaponIndex < weaponCount)
            {
                GameObject newWeapon = Instantiate(weapons[currentWeaponIndex], weaponBox.transform);
                newWeapon.transform.localPosition = Vector3.zero; // 武器の位置をリセット
                newWeapon.transform.localRotation = Quaternion.identity; // 武器の回転をリセット
            }
        }

        // 重力処理
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}