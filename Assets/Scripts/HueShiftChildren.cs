using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HueShiftChildren : MonoBehaviour {


	void Start () {
		var children = GetComponentsInChildren<Renderer>();
		// For hue 0 == 1 so we don't want get to 1,
		// Instead we want hue[children.Length-1] = children.Length-1/children.Length 
		float shift = 1.0f / children.Length;

		/* Old code that yields terrible results
//		for (int i = 0; i < children.Length; i++) {
//			children[i].material.color = (children[i].material.color + Color.HSVToRGB(shift*i, 1, 1)) / 2;
//		}
		*/

		// For each child in the parent create a new texture with a unique hue.
		for (int i = 0; i < children.Length; i++) {
			//Get the original texture
			Texture2D texture = (children [i].material.mainTexture as Texture2D);
			//Create a new texture because we don't want to overwrite the original one
			Texture2D newTexture = new Texture2D(texture.width, texture.height);
			//Get a pixel array for both the old and the new texture
			Color[] pixels = texture.GetPixels();
			Color[] newPixels = newTexture.GetPixels ();
			//Set the new pixels to a new hue but with the old saturation and value
			for (int ii = 0; ii < pixels.Length; ii++) {
				float h, s, v;
				Color.RGBToHSV (pixels[ii], out h, out s, out v);
				newPixels [ii] = Color.HSVToRGB(shift*i, s, v);
			}
			//Put the new pixel array back into the new texture and apply changes.
			newTexture.SetPixels (newPixels);
			newTexture.Apply ();
			children[i].material.mainTexture = newTexture;
		}
	}
}
