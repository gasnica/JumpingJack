using UnityEngine;
using System.Collections;

public class jjGap : MonoBehaviour {

    public void Init(bool moveRight)
    {
        jjLevel level = jjMain.level;

        Vector3 scale = transform.localScale;
        scale.x = level.spacing / 10.0f;
        scale.y = scale.z = level.lineWidth / 10.0f;
        transform.localScale = scale;

        Vector3 position = Vector3.zero;
        position.y = level.bottom;// + 0.5f * level.lineWidth;
        transform.position = position;

        GetComponent<jjWrappingSprite>().Init(moveRight ? jjWrappingSprite.MovementDirection.Right : jjWrappingSprite.MovementDirection.Left);
    }
}
