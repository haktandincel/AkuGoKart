using UnityEngine;
using System.Collections; // Coroutine (bekleme süresi) için eklendi

public class NitroBottle : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public float floatSpeed = 2f;
    public float floatAmount = 0.25f;
    public float respawnTime = 2f; // 🕒 Yeniden doğma süresi

    private Vector3 startPos;
    private Renderer rend;
    private Material mat;
    private Collider col; // Çarpışmayı kontrol etmek için

    void Start()
    {
        startPos = transform.position;

        rend = transform.GetChild(0).GetComponent<Renderer>(); // İlk alt nesneyi al
        col = transform.GetChild(0).GetComponent<Collider>(); // Collider'ı tanımla

        if (rend != null)
        {
            mat = rend.material;
            mat.EnableKeyword("_EMISSION");
        }
    }

    void Update()
    {
        // Eğer obje görünmez ise (alınmışsa) animasyonları hesaplamaya gerek yok
        if (rend != null && !rend.enabled) return;

        // 🔄 Kendi etrafında dönme
        transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

        // 🌀 Hafif yukarı aşağı süzülme
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // ✨ Parlama efekti (nabız gibi)
        if (mat != null)
        {
            float emission = Mathf.PingPong(Time.time * 2f, 1f);
            Color baseColor = Color.cyan; // rengi buradan değiştir
            mat.SetColor("_EmissionColor", baseColor * emission * 2f);
        }
    }

    // 🏎️ Oyuncu nitroya çarptığında / içinden geçtiğinde
    private void OnTriggerEnter(Collider other)
    {
        // Çarpan objenin tag'i "Player" ise (Arabanın Tag'ini Unity'den Player yapmalısın)
        if (other.CompareTag("Player")) 
        {
            // BURAYA OYUNCUYA NİTRO VERME KODUNU EKLEYEBİLİRSİN
            // Örnek: other.GetComponent<CarController>().AddNitro(50);

            // Yeniden doğma işlemini başlat
            StartCoroutine(RespawnRoutine());
        }
    }


    public void temp()
    {
        StartCoroutine(RespawnRoutine());
    }
   // ⏳ 3 saniye bekleyip objeyi ve tüm alt objelerini (çocukları) geri getiren sistem
    private IEnumerator RespawnRoutine()
    {
        rend.enabled = false; // Görünmez yap
        col.enabled = false; // Çarpışmayı devre dışı bırak
        

        // 2. Belirlenen süre kadar bekle (3 saniye)
        yield return new WaitForSeconds(respawnTime);

        transform.GetChild(0).GetComponent<Nitro1Time>().isCollected = false; // Nitro'nun tekrar toplanabilir olmasını sağla
        rend.enabled = true; // Görünür yap
        col.enabled = true; // Çarpışmayı tekrar etkinleştir

        
    }
}