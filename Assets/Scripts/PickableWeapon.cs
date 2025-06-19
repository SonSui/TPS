using System.Transactions;
using UnityEngine;

public class PickableWeapon : MonoBehaviour
{
    public GameObject weaponPrefab; // 武器のプレハブ
    public int durability = 1; // 武器の耐久値
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.PickUpWeapon(weaponPrefab, durability);
                Destroy(gameObject);
            }
        }
    }
}