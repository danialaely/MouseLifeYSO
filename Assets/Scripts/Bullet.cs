using UnityEngine;

public class Bullet : MonoBehaviour
{
    
    void Update()
    {
        transform.position += transform.forward * 10 * Time.deltaTime; 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "CatNew")
        {
            Debug.Log("Collided with cat");
        }
        else 
        {
            Destroy(this.gameObject);
        }
    }
}
