using UnityEngine;

public class MetaballMover : MonoBehaviour
{
    // Límites de la habitación
    public Vector3 roomMin = new Vector3(-4.5f, 0.5f, -4.5f);
    public Vector3 roomMax = new Vector3(4.5f,  4.5f,  4.5f);
    public float speed = 1.5f;

    private Vector3 _velocity;

    void Start()
    {
        // Velocidad aleatoria
        _velocity = Random.insideUnitSphere.normalized * speed;

        // Asegurar que no sea velocidad cero
        if (_velocity == Vector3.zero)
            _velocity = Vector3.one.normalized * speed;
    }

    void Update()
    {
        Vector3 pos = transform.position;
        pos += _velocity * Time.deltaTime;

        // Rebote en cada eje
        if (pos.x < roomMin.x || pos.x > roomMax.x)
        {
            _velocity.x *= -1;
            pos.x = Mathf.Clamp(pos.x, roomMin.x, roomMax.x);
        }
        if (pos.y < roomMin.y || pos.y > roomMax.y)
        {
            _velocity.y *= -1;
            pos.y = Mathf.Clamp(pos.y, roomMin.y, roomMax.y);
        }
        if (pos.z < roomMin.z || pos.z > roomMax.z)
        {
            _velocity.z *= -1;
            pos.z = Mathf.Clamp(pos.z, roomMin.z, roomMax.z);
        }

        transform.position = pos;
    }
}