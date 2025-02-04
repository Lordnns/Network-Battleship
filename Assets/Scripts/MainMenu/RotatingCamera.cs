using UnityEngine;

public class RotatingCamera : MonoBehaviour
{
    [Range(0.0f, 1000.0f)]
    [SerializeField] private float radius;
    [Range(0.0f, 1000.0f)]
    [SerializeField] private float speed;

    private Vector3 pivot;

    private void Start()
    {
        pivot = transform.position + transform.forward * radius;
    }

    private void Update()
    {
        transform.RotateAround(pivot, Vector3.up, speed * Time.deltaTime);
        transform.LookAt(pivot);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * radius, radius);
    }
}
