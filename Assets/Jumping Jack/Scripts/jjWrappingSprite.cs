using UnityEngine;
using System.Collections;

public class jjWrappingSprite : MonoBehaviour {

    public enum MovementDirection
    {
        Left, OneLevel, Right = Left
    }

    public void Init(MovementDirection dir = MovementDirection.OneLevel)
    {
        Sprite sprite = GetComponent<SpriteRenderer>() ? GetComponent<SpriteRenderer>().sprite : null;
        isSprite = sprite != null;

        float levelWidth = jjMain.level.right - jjMain.level.left;
        float floorSpacing = jjMain.level.spacing;

        // Create child objects that mirror this sprite
        if (transform.childCount == 0)
        {
            // Create and initialize object transforms
            int numShadows = dir == MovementDirection.OneLevel ? 2 : 4;
            for (int i = 0; i < numShadows; i++)
            {
                GameObject child = new GameObject();
                child.transform.parent = transform;
                child.transform.localScale = Vector3.one;
                child.transform.localRotation = Quaternion.identity;
                child.transform.position = transform.position
                    + levelWidth * (i % 2 * 2 - 1) * Vector3.right
                    - (i < 2 ? 1.0f : -7.0f) * floorSpacing * ((int)dir - 1) * (i % 2 * 2 - 1) * Vector3.up;
                switch(i)
                {
                    case 0: child.name = "Left Shadow"; break;
                    case 1: child.name = "Right Shadow"; break;
                    case 2: child.name = "Upper Shadow"; break;
                    case 3: child.name = "Lower Shadow"; break;
                }
            }

            // Init common graphics properties
            foreach (Transform t in transform)
            {
                GameObject child = t.gameObject;

                if (sprite)
                {
                    // Create sprite
                    child.AddComponent<SpriteRenderer>().sprite = sprite;
                }
                else
                {
                    // Crate mesh & texture
                    Mesh mesh = GetComponent<MeshFilter>().sharedMesh;

                    child.AddComponent<MeshFilter>().sharedMesh = mesh;
                    child.AddComponent<MeshCollider>().sharedMesh = mesh;

                    child.AddComponent<MeshRenderer>().material = GetComponent<MeshRenderer>().material;

                    child.AddComponent<jjPixelatedTexture>();
                }

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Sprite sprite = GetComponent<SpriteRenderer>() ? GetComponent<SpriteRenderer>().sprite : null;
        if (sprite)
        {
            foreach (Transform child in transform)
            {
                SpriteRenderer childSpriteRenderer = child.GetComponent<SpriteRenderer>();
                if (childSpriteRenderer)
                {
                    childSpriteRenderer.sprite = sprite;
                }
            }
        }
    }

    bool isSprite;
}
