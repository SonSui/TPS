using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;  
    public Vector2 rotationSpeed = new Vector2(400f, 300f); // カメラ回転速度
    public Vector2 pitchClamp = new Vector2(-30f, 60f);    // 垂直角度の制限

    public float defaultDistance = 10f; // カメラ距離
    public float minDistance = 1f;     // 最小距離
    public float smoothSpeed = 10f;    // カメラの追従スムーズさ
    public LayerMask collisionMask;    // カメラがぶつかる対象のレイヤー

    private float yaw;    // 水平角度
    private float pitch;  // 垂直角度
    private Vector3 currentVelocity;   // スムーズ移動用の速度ベクトル
    private Transform cam;             // 実際のカメラ

    void Start()
    {
        cam = Camera.main.transform;

        // 初期角度設定
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;

        // マウスを画面に固定
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        // マウスでカメラ回転
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        yaw += mouseX * rotationSpeed.x * Time.deltaTime;    // 水平
        pitch -= mouseY * rotationSpeed.y * Time.deltaTime;  // 垂直
        pitch = Mathf.Clamp(pitch, pitchClamp.x, pitchClamp.y); // 角度の制限

        
        transform.position = target.position;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f); // カメラ方向を設定

        
        Vector3 desiredCameraPos = transform.position - transform.forward * defaultDistance;

        // 障害物チェック
        RaycastHit hit;
        float actualDistance = defaultDistance;
        if (Physics.Raycast(target.position, -transform.forward, out hit, defaultDistance, collisionMask))
        {
            // 障害物がある場合は、距離を縮める
            actualDistance = Mathf.Clamp(hit.distance-0.25f, minDistance, defaultDistance);
        }

        // 実際のカメラ位置を更新
        Vector3 finalPos = transform.position - transform.forward * actualDistance;
        cam.position = Vector3.SmoothDamp(cam.position, finalPos, ref currentVelocity, 1f / smoothSpeed);

        
        cam.LookAt(target.position + Vector3.up * 0.5f);
    }
}