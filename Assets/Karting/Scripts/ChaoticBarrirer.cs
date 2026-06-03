using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class UnstoppableBarrier : MonoBehaviour
{
    private Rigidbody rb;
    
    [Header("Barrier Settings")]
    public float speed = 2f;         // Açılıp kapanma hızı
    public float openAngle = 90f;    // Bariyerin kalkacağı açı (Derece)
    public Vector3 rotationAxis = Vector3.forward; // Z ekseni etrafında döneceğini varsayarsak
    
    private Quaternion startRotation;
    private Quaternion targetRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // İşin sırrı burada: Kinematic obje iter ama itilemez!
        rb.isKinematic = true; 
        
        // Hızlı hareketlerde bariyerin diğer objelerin içinden geçmesini önler
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative; 
        
        // Görüntüdeki titremeyi engellemek için
        rb.interpolation = RigidbodyInterpolation.Interpolate; 
        
        startRotation = transform.rotation;
        targetRotation = startRotation * Quaternion.Euler(rotationAxis * openAngle);
    }

    void FixedUpdate()
    {
        // Sinüs dalgası kullanarak 0 ile 1 arasında pürüzsüz (ease-in/ease-out) bir değer üretiyoruz.
        // Bu sayede bariyer uç noktalara geldiğinde aniden sekmez, motor gibi yavaşlayıp geri döner.
        float wave = (Mathf.Sin(Time.time * speed) + 1f) / 2f; 
        
        // Başlangıç ve bitiş açıları arasında wave değerine göre gidip gelme
        Quaternion currentRot = Quaternion.Slerp(startRotation, targetRotation, wave);
        
        // Fiziği uyararak döndürüyoruz. Araba araya girerse fiziksell olarak ezilir veya fırlatılır!
        rb.MoveRotation(currentRot); 
    }
}