using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class WeaponData
{
    public GameObject prefab;
    public int durability;

    public WeaponData(GameObject prefab, int durability)
    {
        this.prefab = prefab;
        this.durability = durability;
    }
}

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("�ړ��ݒ�")]
    public float moveSpeed = 5f;             // �ړ����x
    public float jumpHeight = 2f;            // �W�����v�̍����i���g�p�j
    public float gravity = -9.81f;           // �d�͉����x
    public float rotationSpeed = 360f;       // ��]���x�i�x�^�b�j

    [Header("�ڒn����")]
    public Transform groundCheck;            // �n�ʔ���̂��߂�Transform
    public float groundDistance = 0.4f;      // �ڒn�`�F�b�N�̔��a
    public LayerMask groundMask;             // �n�ʂƂ��Ĕ��肳��郌�C���[

    [Header("����֘A")]
    public GameObject weaponBox;             // ����̐e�I�u�W�F�N�g
    public List<WeaponData> weapons;         // ��������̃��X�g
    private int currentWeaponIndex = -1;      // ���݂̕���C���f�b�N�X

    [Header("�v���C���[���f��")]
    public GameObject playerModel;           // �v���C���[���f����GameObject
    public Animator playerAnimator;       // �v���C���[�̃A�j���[�V�����R���|�[�l���g


    private CharacterController controller;  // CharacterController�̎Q��
    private Vector3 velocity;                // ���������̑��x
    private bool isGrounded;                 // �ڒn���Ă��邩�ǂ���
    private Transform mainCam;               // ���C���J������Transform

    // �v���C���[�̏��
    enum PlayerState
    {
        Idle,       // �ҋ@
        Walking,    // ����
        Running,    // ����
        Jumping,    // �W�����v��
        Falling     // ������
    };
    PlayerState currentState = PlayerState.Idle;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCam = Camera.main.transform; // ���C���J�������擾
        SwitchWeapon(); // ���������ݒ�
    }

    void Update()
    {
        // �n�ʂɐڂ��Ă��邩����
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
        {
            // �n�ʂɉ����t����悤��Y���x�����Z�b�g
            velocity.y = -2f;
        }

        // ���͎擾�i�����L�[/WASD�j
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(inputX, 0f, inputZ).normalized;

        // �J�����̌�������Ɉړ��������v�Z
        Vector3 camForward = mainCam.forward;
        Vector3 camRight = mainCam.right;
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // �J������̈ړ������x�N�g��
        Vector3 moveDir = camRight * inputX + camForward * inputZ;
        moveDir.Normalize();

        // �ړ�����
        if (moveDir.magnitude >= 0.1f)
        {
            // ���݂̌�������ڕW�����փX���[�Y�ɉ�]
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // �L�����N�^�[���ړ�������
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
            // �U���A�j���[�V�������Đ�
            playerAnimator.SetTrigger("Attack");
            if (currentWeaponIndex >= 0 && currentWeaponIndex < weapons.Count)
            {
                WeaponData currentWeapon = weapons[currentWeaponIndex];
                currentWeapon.durability--;

                Debug.Log("Attack with: " + currentWeapon.prefab.name + " Durability now: " + currentWeapon.durability);

                if (currentWeapon.durability <= 0)
                {
                    Debug.Log("Weapon broke: " + currentWeapon.prefab.name);

                    if (weaponBox.transform.childCount > 0)
                    {
                        Destroy(weaponBox.transform.GetChild(0).gameObject);
                    }

                    weapons.RemoveAt(currentWeaponIndex);

                    if (weapons.Count > 0)
                    {
                        SwitchWeapon(); // �؂�ւ���
                    }
                    else
                    {
                        currentWeaponIndex = -1; // ����Ȃ����
                    }
                }
            }
        }
        if (Input.GetButtonDown("Fire2"))
        {
            SwitchWeapon(); // �����؂�ւ�
        }

        // �d�͏���
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    void SwitchWeapon()
    {
        int weaponCount = weapons.Count;
        if (weaponCount == 0)
        {
            Debug.LogWarning("No weapons available to switch.");
            return;
        }
        currentWeaponIndex = (currentWeaponIndex + 1) % weaponCount; // �����؂�ւ�
        if (weaponBox.transform.childCount > 0)
        {
            Transform oldWeapon = weaponBox.transform.GetChild(0);
            if (oldWeapon != null)
            {
                Destroy(oldWeapon.gameObject); // �Â�������폜
            }
        }
        if (currentWeaponIndex < weaponCount)
        {
            GameObject newWeapon = Instantiate(weapons[currentWeaponIndex].prefab, weaponBox.transform);
            newWeapon.transform.localPosition = Vector3.zero; // ����̈ʒu�����Z�b�g
            newWeapon.transform.localRotation = Quaternion.identity; // ����̉�]�����Z�b�g
        }
    }
    public void PickUpWeapon(GameObject prefab,int durability)
    {
        if (prefab!= null)
        {
            WeaponData weaponData = new WeaponData(prefab, durability);
            weapons.Add(weaponData);
            Debug.Log("PickUp:" + weaponData.prefab.name+ " Durability:" + weaponData.durability);
        }
    }
}