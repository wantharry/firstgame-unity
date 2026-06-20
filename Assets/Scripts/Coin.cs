using UnityEngine;

// A spinning collectible. Its collider is a trigger, so when the
// player rolls into it, OnTriggerEnter fires, scores a point, and
// the coin removes itself.
public class Coin : MonoBehaviour
{
    [Tooltip("Degrees per second the coin spins.")]
    public float spinSpeed = 120f;

    void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.AddPoint();

            Destroy(gameObject);
        }
    }
}
