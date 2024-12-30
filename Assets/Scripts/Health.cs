using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHP = 1000; // HP maksimal
    private int currentHP;

    private void Start()
    {
        currentHP = maxHP;
    }

    /// <summary>
    /// Mengurangi HP objek
    /// </summary>
    /// <param name="damage">Jumlah damage yang diterima</param>
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"{gameObject.name} HP: {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Fungsi yang dipanggil ketika HP mencapai 0
    /// </summary>
    private void Die()
    {
        Debug.Log($"{gameObject.name} has been destroyed!");
        Destroy(gameObject); // Menghilangkan objek dari scene
    }

    /// <summary>
    /// Mendapatkan HP saat ini
    /// </summary>
    public int GetCurrentHP()
    {
        return currentHP;
    }
}
