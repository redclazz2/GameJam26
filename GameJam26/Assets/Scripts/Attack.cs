using UnityEngine;

public class Attack : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Attack START on " + gameObject.name);
        Destroy(gameObject, 0.2f);
    }

    void OnDestroy()
    {
        Debug.Log("Attack DESTROYED: " + gameObject.name);
    }
}
