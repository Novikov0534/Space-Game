using UnityEngine;

public class MoveSpot : MonoBehaviour
{
    private Transform _originalParent;
    private Vector3 _originalLocalPosition;
    private Vector3 _originalLocalRotation;

    void Awake()
    {
        //Сохраняем исходные параметры при старте
        _originalParent = transform.parent;
        _originalLocalPosition = transform.localPosition;
        _originalLocalRotation = transform.localEulerAngles;
    }

    private void Update()
    {
        if (_originalParent == null)
        {
            Destroy(gameObject);
        }
        
    }

    public void DetachFromEnemy()
    {
        if (transform.parent != null)
        {
            transform.SetParent(null, worldPositionStays: true);
        }
    }

    public void ReattachToEnemy()
    {
        if (_originalParent != null)
        {
            transform.SetParent(_originalParent);
            transform.localPosition = _originalLocalPosition;
            transform.localEulerAngles = _originalLocalRotation;
        }
    }
}