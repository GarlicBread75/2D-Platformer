using UnityEngine;

public class RainbowRot : MonoBehaviour
{
    float hue;
    public float colourModifier;
    float rot;
    public float rotationModifier;

    public bool canColour;
    public bool canRot;

    private void Awake()
    {
        hue = Random.Range(0f, 1f);
        rot = Random.Range(0f, 360f);
    }

    void Update()
    {
        if (canColour)
        {
            if (hue > 1f)
            {
                hue = 0;
            }
            else
            {
                hue += Time.deltaTime / colourModifier;
            }

            gameObject.GetComponent<SpriteRenderer>().color = Color.HSVToRGB(hue, 1f, 1f);
        }

        if (canRot)
        {
            if (rot > 1440)
            {
                rot = 0;
            }
            else
            {
                rot += Time.deltaTime / rotationModifier;
            }

            transform.rotation = Quaternion.Euler(0, 0, rot);
        }
    }
}
