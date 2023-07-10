using UnityEngine;

public class DashGemSpawn : MonoBehaviour
{
    [SerializeField] GameObject dashGem;
    [SerializeField] float respawnDelay;
    public float delay;
    GameObject thing;

    private void Awake()
    {
        delay = respawnDelay;
        thing = Instantiate(dashGem, transform.position, Quaternion.Euler (0, 0, 45), gameObject.transform);
    }

    private void FixedUpdate()
    {
        if (thing == null)
        {
            if (delay > 0)
            {
                delay -= Time.fixedDeltaTime;
            }
            else
            {
                thing = Instantiate(dashGem, transform.position, Quaternion.Euler(0, 0, 45), gameObject.transform);
                delay = respawnDelay;
            }
        }
    }
}
