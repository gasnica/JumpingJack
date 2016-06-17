using UnityEngine;
using System.Collections;

public class jjPixelatedTexture : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Texture mainTexture = GetComponent<MeshRenderer>().material.mainTexture;
        mainTexture.filterMode = FilterMode.Point;
        mainTexture.wrapMode = TextureWrapMode.Repeat;
    }
}
