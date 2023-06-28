// Version 2023
//  (Updates: no getters in loop for efficiency)
using UnityEngine;
using System.IO;	// For file I/O
using System;		// For String formatting

public class SaveMeshToOBJ : MonoBehaviour {
	public string filename;
	
	public void SaveMesh() {
		var filter = GetComponent<MeshFilter>();
		if (filter!=null) {
			int index = 1;
			while (File.Exists(filename+index+".obj")) {
				index++;
			}
			string fullName = "Assets/" + filename + index + ".obj";

			SaveMeshToObj (GetComponent<MeshFilter> ().mesh, fullName, GetComponent<MeshRenderer>());
			Debug.Log ("Saved mesh to " + fullName);
		}
	}

	/// <summary>
	/// Saves the mesh as an OBJ file.
	/// </summary>
	/// <param name="mesh">Mesh.</param>
	/// <param name="filename">Filename. Does not automatically set the extension.</param>
	static public void SaveMeshToObj(Mesh mesh, string filename, MeshRenderer render) {
		StreamWriter writer = new StreamWriter (filename);

		//object name
		writer.WriteLine ("#Mesh\n");

        for (int g = 0; g < mesh.subMeshCount; g++)
        {
			writer.WriteLine ($"g mesh{g}\n");

			Mesh subMesh = new Mesh();
			subMesh.vertices = mesh.vertices;
			subMesh.uv = mesh.uv;
			subMesh.normals = mesh.normals;
			subMesh.triangles = mesh.GetTriangles(g);



			//vertices
			Vector3[] verts = subMesh.vertices;
			for (int i = 0; i < verts.Length; i++)
			{
				float x = -verts[i].x;
				float y = verts[i].y;
				float z = verts[i].z;
				writer.WriteLine(String.Format("v {0:F3} {1:F3} {2:F3}", x, y, z));
			}
			writer.WriteLine("");

			//uv-set
			Vector2[] uvs = subMesh.uv;
			for (int i = 0; i < uvs.Length; i++)
			{
				float u = uvs[i].x;
				float v = uvs[i].y;
				writer.WriteLine(String.Format("vt {0:F3} {1:F3}", u, v));
			}
			writer.WriteLine("");

			//normals
			Vector3[] normals = subMesh.normals;
			for (int i = 0; i < normals.Length; i++)
			{
				float x = normals[i].x;
				float y = normals[i].y;
				float z = normals[i].z;
				writer.WriteLine(String.Format("vn {0:F3} {1:F3} {2:F3}", x, y, z));
			}
			writer.WriteLine("");

			//triangles
			int[] tris = subMesh.triangles;
			for (int i = 0; i < tris.Length / 3; i++)
			{
				int v0 = tris[i * 3 + 0] + 1;
				int v1 = tris[i * 3 + 1] + 1;
				int v2 = tris[i * 3 + 2] + 1;
				writer.WriteLine(String.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", v2, v1, v0));
			}
			Material[] mats = render.materials; 


			writer.WriteLine(String.Format(($"usemtl {mats[g].name}\n")));

			for (int i = 0; i < tris.Length / 3; i++)
			{
				int v0 = tris[i * 3 + 0] + 1;
				int v1 = tris[i * 3 + 1] + 1;
				int v2 = tris[i * 3 + 2] + 1;
				writer.WriteLine(String.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}", v2, v1, v0));
			}


		}



		

		writer.Close ();
	}
}
