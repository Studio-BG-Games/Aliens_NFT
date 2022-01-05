using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace HexasphereGrid {


	public partial class Hexasphere : MonoBehaviour {

		Color[] heights, waters;
		int heightMapWidth, heightMapHeight;
		Color[] gradientColors;
		int rampWidth;


		void LoadRampColors (Texture2D rampColors) {
			if (rampColors == null) {
				if (defaultRampTexture == null) {
					defaultRampTexture = Resources.Load<Texture2D> ("Textures/HexasphereDefaultRampTex");
				}
				if (defaultRampTexture != null) {
					gradientColors = defaultRampTexture.GetPixels ();
					rampWidth = defaultRampTexture.width;
				}
			} else {
				gradientColors = rampColors.GetPixels ();
				rampWidth = rampColors.width;
			}
		}


        /// <summary>
        /// Converts latitude/longitude/altitude to sphere coordinates.
        /// </summary>
        Vector3 GetSpherePointFromLatLon(float lat, float lon) {
            float phi = lat * 0.0174532924f;
            float theta = (lon + 90.0f) * 0.0174532924f;
            float cosPhi = Mathf.Cos(phi);
            float x = cosPhi * Mathf.Cos(theta) * 0.5f;
            float y = Mathf.Sin(phi) * 0.5f;
            float z = cosPhi * Mathf.Sin(theta) * 0.5f;
            return new Vector3(x, y, z);
        }


    }


}