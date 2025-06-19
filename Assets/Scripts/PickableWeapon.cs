using System.Transactions;
using UnityEngine;

public class PickableWeapon : MonoBehaviour
{
    public GameObject weaponPrefab; // ����̃v���n�u
    public int durability = 1; // ����̑ϋv�l
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