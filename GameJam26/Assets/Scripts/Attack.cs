using UnityEngine;

public class Attack : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 0.2f);
    }

    void OnDestroy()
    {
       
    }
}
