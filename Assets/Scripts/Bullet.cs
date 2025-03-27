using UnityEngine;

public class Bullet : MonoBehaviour
{
    
    void Update()
    {
        transform.position += transform.forward * 10 * Time.deltaTime; 
    }
}
