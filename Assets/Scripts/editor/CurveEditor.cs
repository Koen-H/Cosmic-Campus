using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Curve))]
public class CurveEditor : Editor
{
	private Curve curve;

	private void OnEnable()
	{
		curve = (Curve)target;
	}

	public override void OnInspectorGUI()
	{

		base.OnInspectorGUI();

		if (GUILayout.Button("Apply"))
		{
			curve.Apply();
		}
	}

	// This method is called by Unity whenever it renders the scene view.
	// We use it to draw gizmos, and deal with changes (dragging objects)
	void OnSceneGUI()
	{
		if (curve.points == null)
			return;


		DrawAndMoveCurve();

		// Add new points if needed:
		Event e = Event.current;
		if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Space)
		{
			AddPoint();
			Debug.Log("Space pressed - trying to add point to curve");
			e.Use(); // To prevent the event from being handled by other editor functionality
		}

		ShowAndMovePoints();
	}

	// Example: here's how to draw a handle:
	void DrawAndMoveCurve()
	{
		Transform handleTransform = curve.transform;
		Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ?
			handleTransform.rotation : Quaternion.identity;

		EditorGUI.BeginChangeCheck();
		Vector3 newPosition = Handles.PositionHandle(handleTransform.position, handleRotation);
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(curve.transform, "Move curve");
			EditorUtility.SetDirty(curve);
			curve.transform.position = newPosition;
		}
	}

	// Tries to add a point to the curve, where the mouse is in the scene view.
	// Returns true if a change was made.
	void AddPoint()
	{

		Undo.RecordObject(curve, "Added point");
		EditorUtility.SetDirty(curve);
		Transform handleTransform = curve.transform;

		Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
		{
			Debug.Log("Adding spline point at mouse position: " + hit.point);
			// TODO (1.2): Add this action to the undo list and mark the scene dirty
			curve.points.Add(handleTransform.InverseTransformPoint(hit.point));
		}
	}

	// Show points in scene view, and check if they're changed:
	void ShowAndMovePoints()
	{
		Transform handleTransform = curve.transform;


		Vector3 previousPoint = Vector3.zero;
		for (int i = 0; i < curve.points.Count; i++)
		{
			Vector3 currentPoint = curve.GetPoint(i);

			// TODO (1.2): Draw a line from previous point to current point, in white

			EditorGUI.BeginChangeCheck();

			currentPoint = Handles.PositionHandle(currentPoint, Quaternion.identity);


			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(curve, "Move curve");
				curve.points[i] = handleTransform.InverseTransformPoint(currentPoint);
				EditorUtility.SetDirty(curve);
			}
			if (i == curve.points.Count - 1)
			{
				previousPoint = currentPoint;
				return;
			}

			Vector3 nextPoint = handleTransform.TransformPoint(curve.points[i + 1]);

			Handles.DrawLine(nextPoint, currentPoint);

			previousPoint = currentPoint;

			// TODO (1.2): 
			// Draw position handles (see the above example code)
			// Record in the undo list and mark the scene dirty when the handle is moved.

		}
	}
}
