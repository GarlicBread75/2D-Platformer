using UnityEngine;

public class RandomSprite : MonoBehaviour
{
    public Sprite[] sprites;

    private void Awake()
    {
        gameObject.GetComponent<SpriteRenderer>().sprite = sprites[Random.Range(0, sprites.Length)];
    }
}
