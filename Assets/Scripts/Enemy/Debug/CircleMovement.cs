using UnityEngine;

public class CircleMovement : MonoBehaviour, IDamageable
{
    [SerializeField] Vector3 center;
    [SerializeField] float radius;
    [SerializeField] float speed;

    Vector3 axis = Vector3.up;
    float angle;

    public bool Damage(float damageValue, Vector3 hitPosition)
    {
        Debug.Log("player hit");
        return false;
    }

    public void Death()
    {

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
