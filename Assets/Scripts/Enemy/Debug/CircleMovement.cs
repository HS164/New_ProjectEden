using UnityEngine;

public class CircleMovement : MonoBehaviour, IDamageable
{
    [SerializeField] Vector3 center;
    [SerializeField] float radius;
    [SerializeField] float speed;

    Vector3 axis = Vector3.up;
    float angle;

    public void Damage(float damageValue)
    {
        Debug.Log("player hit");
    }

    private void FixedUpdate()
    {
        angle += speed * Time.deltaTime;

        Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radius;

        Quaternion rotation = Quaternion.FromToRotation(Vector3.up, axis.normalized);
        Vector3 rotatedOffset = rotation * offset;

        transform.position = center + rotatedOffset;
    }
}
