using UnityEngine;

public class LockZRotationSprite : MonoBehaviour
{
    void LateUpdate()
    {
        Vector3 rotation = transform.eulerAngles;

        transform.rotation = Quaternion.Euler(rotation.x, rotation.y, 0f);
    }
}
