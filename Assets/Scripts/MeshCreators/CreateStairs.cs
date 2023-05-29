using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Handout {
	public class CreateStairs : MonoBehaviour {
		public int numberOfSteps = 10;
		// The dimensions of a single step of the staircase:
		public float width=3;
		public float height=1;
		public float depth=1;

		MeshBuilder builder;

		void Start () {
			builder = new MeshBuilder ();
			CreateShape ();
			GetComponent<MeshFilter> ().mesh = builder.CreateMesh (true);
		}

		/// <summary>
		/// Creates a stairway shape in [builder].
		/// </summary>
		void CreateShape() {
			builder.Clear ();

			/**
			// V1: single step, hard coded:
			// bottom:
			int v1 = builder.AddVertex (new Vector3 (2, 0, 0), new Vector2 (1, 0));	
			int v2 = builder.AddVertex (new Vector3 (-2, 0, 0), new Vector2 (0, 0));
			// top front:
			int v3 = builder.AddVertex (new Vector3 (2, 1, 0), new Vector2 (1, 0.5f));	
			int v4 = builder.AddVertex (new Vector3 (-2, 1, 0), new Vector2 (0, 0.5f));
			// top back:
			int v5 = builder.AddVertex (new Vector3 (2, 1, 1), new Vector2 (0, 1));	
			int v6 = builder.AddVertex (new Vector3 (-2, 1, 1), new Vector2 (1, 1));

			builder.AddTriangle (v1, v2, v3);
			builder.AddTriangle (v2, v3, v4);
			builder.AddTriangle (v3, v4, v5);
			builder.AddTriangle (v4, v6, v5);

			/**/
			// V2, with for loop:
			for (int i = 0; i < numberOfSteps; i++) {
				Vector3 offset = new Vector3 (0, height * i, depth * i); 

				// TODO 1: use the width and height parameters from the inspector to change the step width and height

				// TODO 4: Fix the uvs:
				// bottom:
				int v1 = builder.AddVertex (offset + new Vector3 (width, 0, 0), new Vector2 (1, 0));	
				int v2 = builder.AddVertex (offset + new Vector3 (-width, 0, 0), new Vector2 (0, 0));
				// top front:
				int v3 = builder.AddVertex (offset + new Vector3 (width, height, 0), new Vector2 (1, 0.5f));	
				int v4 = builder.AddVertex (offset + new Vector3 (-width, height, 0), new Vector2 (0, 0.5f));
				// top back:
				int v5 = builder.AddVertex (offset + new Vector3 (width, height, depth), new Vector2 (1, 0));	
				int v6 = builder.AddVertex (offset + new Vector3 (-width, height, depth), new Vector2 (0,0));

				// side right:
				int v7 = builder.AddVertex(offset + new Vector3(width, 0, 0), new Vector2(0, 0));
				int v8 = builder.AddVertex(offset + new Vector3(width, height, 0), new Vector2(0, 0.5f));
				int v9 = builder.AddVertex(offset + new Vector3(width, height, depth), new Vector2(0.5f, 0.5f));

				// side left: 
				int v10 = builder.AddVertex(offset + new Vector3(-width, 0, 0), new Vector2(0, 0));
				int v11 = builder.AddVertex(offset + new Vector3(-width, height, depth), new Vector2(0.5f, 0.5f));
				int v12 = builder.AddVertex(offset + new Vector3(-width, height, 0), new Vector2(0, 0.5f));

				// back back bottom: 
				int v13 = builder.AddVertex(offset + new Vector3(width, 0, 0), new Vector2(0, 0));
				int v14 = builder.AddVertex(offset + new Vector3(width, height, depth), new Vector2(0, 0.5f));
				int v15 = builder.AddVertex(offset + new Vector3(-width, 0, 0), new Vector2(1, 0));


				// back back top:
				int v16 = builder.AddVertex(offset + new Vector3(-width, 0, 0), new Vector2(1,0));
				int v17 = builder.AddVertex(offset + new Vector3(width, height, depth), new Vector2(0, 0.5f));
				int v18 = builder.AddVertex(offset + new Vector3(-width, height, depth), new Vector2(1, 0.5f));



				// TODO 2: Fix the winding order (everything clockwise):
				builder.AddTriangle (v1, v2, v3);
				builder.AddTriangle (v2, v4, v3);
				builder.AddTriangle (v3, v4, v5);
				builder.AddTriangle (v4, v6, v5);
				builder.AddTriangle (v7, v8, v9);
				builder.AddTriangle (v10, v11, v12);
				builder.AddTriangle (v13, v14, v15);
				builder.AddTriangle (v16, v17, v18);

				// TODO 3: make the mesh solid by adding left, right and back side.

				// TODO 5: Fix the normals by *not* reusing a single vertex in multiple triangles with different normals (solve it by creating more vertices at the same position)
			}
			/**/
		}
		
	}
}