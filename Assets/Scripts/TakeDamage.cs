using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    public interface TakeDamage
    {
        void TakeDamage(int damage);
    }
}
