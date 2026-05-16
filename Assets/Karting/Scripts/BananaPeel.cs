using UnityEngine;

namespace KartGame.KartSystems
{
    public class BananaPeel : MonoBehaviour
    {
        [Tooltip("Muz kabuğuna çarpan kartın spin atma süresi.")]
        public float spinDuration = 1.5f;

        private bool hasTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            // Eğer muz zaten birine çarptıysa işlem yapma
            if (hasTriggered) return;

            ArcadeKart kart = other.GetComponentInParent<ArcadeKart>();
            if (kart != null)
            {
                hasTriggered = true;
                
                // Kartı spin moduna sok
                kart.SpinOut(spinDuration);

                // Muz kabuğunu yok et
                Destroy(gameObject);
            }
        }
    }
}