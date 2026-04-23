using UnityEngine;

public class NitroBottle : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public float floatSpeed = 2f;
    public float floatAmount = 0.25f;

    private Vector3 startPos;
    private Renderer rend;
    private Material mat;

    void Start()
    {
        startPos = transform.position;

        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            mat = rend.material;
            mat.EnableKeyword("_EMISSION");
        }
    }

    void Update()
    {
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
}