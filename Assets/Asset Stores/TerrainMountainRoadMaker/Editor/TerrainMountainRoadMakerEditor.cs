using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.Linq;


namespace TerrainMountainRoadMaker
{
	[CustomEditor(typeof(TerrainMountainRoadMaker))]
	public class TerrainMountainRoadMakerEditor : Editor
	{
		#region variables
		int bezierMode = 0;
		Vector3[] bezierPoints = new Vector3[4];

		readonly Color C_pos = new Color(1f,1f,0f,0.4f);
		readonly Color C_tan = new Color(0f,1f,0f,0.4f);
		readonly Color C_line = new Color(1f,1f,0f,1f);
		readonly Color C_bezier = new Color(1f,0.5f,0f,0.4f);
		readonly Color C_selectedBezier = new Color(1f,0f,0f,1f);

		ReorderableList list;

		List<Vector3[]> pointsForHeight = new List<Vector3[]>();
		#endregion


		#region Initialize & Validate
		void OnEnable()
		{
			// set background of label.
			var t = (TerrainMountainRoadMaker)target;
			t.style.normal.background = Texture2D.whiteTexture;

			// set reorderable list.
			list = new ReorderableList(
				serializedObject,
				serializedObject.FindProperty("Points"),
				true,true,false,true
			);

			// layout list.
			list.drawElementCallback = (rect,index,active,focus) => {
				// draw text field.
				rect.width*=0.5f;
				var info = list.serializedProperty.GetArrayElementAtIndex(index);
				var prop = info.FindPropertyRelative("label");
				prop.stringValue = EditorGUI.TextField(rect,prop.stringValue);
			};
		}
		#endregion


		#region CustomInspectorGUI
		public override void OnInspectorGUI()
		{
			var t = (TerrainMountainRoadMaker)target;

			Undo.RecordObject(t,"TerrainMountainRoadMaker");

			EditorGUILayout.LabelField("Parameter",(GUIStyle)"BoldLabel");
			t.roadWidth = EditorGUILayout.IntField("Road Width",t.roadWidth);
			t.roadResolution = EditorGUILayout.IntField("Road Resolution",t.roadResolution);
			t.roundingCorner = EditorGUILayout.FloatField("Rounding Corner",t.roundingCorner);
			t.roundingCorner = Mathf.Clamp01(t.roundingCorner);

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Visibility",(GUIStyle)"BoldLabel");
			t.showLabel = EditorGUILayout.Toggle("Show Label Always",t.showLabel);
			t.showArrows = EditorGUILayout.Toggle("Show Arrows Always",t.showArrows);
			t.showSpheres = EditorGUILayout.Toggle("Show Spheres Always",t.showSpheres);
			t.showLines = EditorGUILayout.Toggle("Show Lines Always",t.showLines);

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("BezierLines",(GUIStyle)"BoldLabel");
			t.directInput = EditorGUILayout.Toggle("Direct Input",t.directInput);

			if(t.directInput)
			{
				for (int i=0;i<t.Points.Count;i++)
				{
					var p = t.Points[i];
					p.label = EditorGUILayout.TextField(p.label);
					p.startPoint = EditorGUILayout.Vector3Field("startPoint",p.startPoint);
					p.endPoint = EditorGUILayout.Vector3Field("endPoint",p.endPoint);
					p.startTangent = EditorGUILayout.Vector3Field("startTangent",p.startTangent);
					p.endTangent = EditorGUILayout.Vector3Field("endTangent",p.endTangent);
					t.Points[i] = p;
				}
			}
			else
			{
				if(bezierMode==0)
				{
					if(GUILayout.Button("AddBezierLine"))
					{
						bezierPoints = new Vector3[4];
						bezierMode++;
					}
				}
				else
				{
					GUI.color = Color.gray;
					if(GUILayout.Button("Cancel"))
						bezierMode = 0;
				}

				serializedObject.Update();
				list.DoLayoutList();
				serializedObject.ApplyModifiedProperties();
			}

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Painting",(GUIStyle)"BoldLabel");

#if UNITY_2020_3_OR_NEWER
			var tLayers = t.terrain.terrainData.terrainLayers;
			if (tLayers.Length>0)
			{
				if(t.splatIndex>-1)
					GUILayout.Box(new GUIContent(tLayers[t.splatIndex].diffuseTexture),GUILayout.Width(48),GUILayout.Height(48));
				else
					GUILayout.Box("Not\nPaint",GUILayout.Width(48),GUILayout.Height(48));

				EditorGUILayout.BeginHorizontal();
				{
					if(GUILayout.Button("Not",GUILayout.Width(32),GUILayout.Height(32)))
						t.splatIndex = -1;

					for(var i=0;i< tLayers.Length;i++)
					{
						if(GUILayout.Button(tLayers[i].diffuseTexture, GUILayout.Width(32),GUILayout.Height(32)))
							t.splatIndex = i;
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				EditorGUILayout.LabelField("No texture is found");
			}
#else
			var splats = t.terrain.terrainData.splatPrototypes;
			if(splats.Length>0)
			{
				if(t.splatIndex>-1)
					GUILayout.Box(new GUIContent(splats[t.splatIndex].texture),GUILayout.Width(48),GUILayout.Height(48));
				else
					GUILayout.Box("Not\nPaint",GUILayout.Width(48),GUILayout.Height(48));

				EditorGUILayout.BeginHorizontal();
				{
					if(GUILayout.Button("Not",GUILayout.Width(32),GUILayout.Height(32)))
						t.splatIndex = -1;

					for(var i=0;i<splats.Length;i++)
					{
						if(GUILayout.Button(splats[i].texture,GUILayout.Width(32),GUILayout.Height(32)))
							t.splatIndex = i;
					}
				}
				EditorGUILayout.EndHorizontal();
			}
			else
			{
				EditorGUILayout.LabelField("No texture is found");
			}
#endif


			if (bezierMode==0)
			{
				if(GUILayout.Button("do making"))
				{
					if(EditorUtility.DisplayDialog(
						"Terrain Road Maker",
						"Would you want to make roads?",
						"ok",
						"cancel"
					))
					{
						DoMaking();
					}
				}
			}
			if(t.hightUndo.Count>0)
			{
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				{
					if(GUILayout.Button("Undo"))
					{
						t.terrain.terrainData.SetHeightsDelayLOD(0,0,t.hightUndo[t.hightUndo.Count-1]);
						t.hightUndo.RemoveAt(t.hightUndo.Count-1);

						t.terrain.terrainData.SetAlphamaps(0,0,t.alphaUndo[t.alphaUndo.Count-1]);
						t.alphaUndo.RemoveAt(t.alphaUndo.Count-1);
					}
					if(GUILayout.Button("Clear UndoLog"))
					{
						t.hightUndo.Clear();
						t.alphaUndo.Clear();
					}
				}
				EditorGUILayout.EndHorizontal();
			}
		}
		#endregion


#region Customize SceneView & Create Handles
		void OnSceneGUI ()
		{
			var t = (TerrainMountainRoadMaker)target;
			var lp = t.transform.position;

			Undo.RecordObject(t,"RoadMakerSceneGUI");

			if(bezierMode!=0)
			{
				var mp = Event.current.mousePosition;
				Ray ray = HandleUtility.GUIPointToWorldRay(mp);
				RaycastHit hit;
				if (t.collider.Raycast(ray, out hit, float.MaxValue))
				{
					var pos = t.transform.InverseTransformPoint(hit.point);

					// for draw preview.
					if (bezierMode == 1)
						Handles.color = C_pos;
					else if (bezierMode == 2)
					{
						Handles.color = C_pos;
						bezierPoints[1] = bezierPoints[2] = bezierPoints[3] = pos;
					}
					else if (bezierMode == 3)
					{
						Handles.color = C_tan;
						bezierPoints[2] = bezierPoints[3] = pos;
					}
					else if (bezierMode == 4)
					{
						Handles.color = C_tan;
						bezierPoints[3] = pos;
					}

					// draw clickable button.
#if UNITY_2020_3_OR_NEWER
					if (Handles.Button(pos + lp, Quaternion.identity, 1f, 3f, (i, p, r, s, e) => Handles.SphereHandleCap(i, p, r, s, e)))
#else
					if (Handles.Button(pos + lp, Quaternion.identity, 1f, 3f, (i, p, r, s) => Handles.SphereCap(i, p, r, s)))
#endif
					{
						bezierPoints[bezierMode - 1] = pos;

						bezierMode++;
						if (bezierMode == 5)
						{
							bezierMode = 0;
							t.Points.Add(new BezierInfo(
								"Line " + (t.Points.Count + 1),
								bezierPoints[0],
								bezierPoints[1],
								bezierPoints[2],
								bezierPoints[3]
							));
							OnEnable();
							list.index = list.count - 1;
						}
					}
				}

				// draw bezier line preview.
				if(bezierMode>1)
				{
					Handles.DrawBezier(
						bezierPoints[0]+lp,
						bezierPoints[1]+lp,
						bezierPoints[2]+lp,
						bezierPoints[3]+lp,
						Color.red,
						Texture2D.whiteTexture,
						2f
					);
					if(bezierMode>2)
					{
						Handles.color = C_line;
						Handles.DrawLine(bezierPoints[0]+lp,bezierPoints[2]+lp);
						Handles.DrawLine(bezierPoints[1]+lp,bezierPoints[3]+lp);
					}
				}
			}

			for (int i=0;i<t.Points.Count;i++)
			{
				var p = t.Points[i];
				var q = Quaternion.identity;

				// draw sphere & line.
				if(list.index==i || t.showSpheres)
				{
#if UNITY_2020_3_OR_NEWER
					Handles.color = C_pos;
					Handles.SphereHandleCap(0, p.startPoint + lp, q, 3f, EventType.Repaint);
					Handles.SphereHandleCap(0, p.endPoint + lp, q, 3f, EventType.Repaint);
					Handles.color = C_tan;
					Handles.SphereHandleCap(0, p.startTangent + lp, q, 3f, EventType.Repaint);
					Handles.SphereHandleCap(0, p.endTangent + lp, q, 3f, EventType.Repaint);
#else
					Handles.color = C_pos;
					Handles.SphereCap(0, p.startPoint + lp, q, 3f);
					Handles.SphereCap(0, p.endPoint + lp, q, 3f);
					Handles.color = C_tan;
					Handles.SphereCap(0, p.startTangent + lp, q, 3f);
					Handles.SphereCap(0, p.endTangent + lp, q, 3f);
#endif
				}
				if(list.index==i || t.showLines)
				{
					Handles.color = C_line;
					Handles.DrawLine(p.startPoint + lp, p.startTangent + lp);
					Handles.DrawLine(p.endPoint + lp, p.endTangent + lp);

					// draw bezier line.
					Handles.DrawBezier(
						p.startPoint+lp,
						p.endPoint+lp,
						p.startTangent+lp,
						p.endTangent+lp,
						(i==list.index?C_selectedBezier:C_bezier),
						Texture2D.whiteTexture,
						2f
					);
				}

				// draw label.
				if(list.index==i || t.showLabel)
				{
					var labelPos = (p.startPoint+p.endPoint+p.startTangent+p.endTangent)/4f+lp;
					Handles.Label(labelPos,p.label,t.style);
				}

				// set movable arrows.
				if(list.index==i || t.showArrows)
				{
					p.startPoint = Handles.PositionHandle(p.startPoint+lp,q)-lp;
					p.endPoint = Handles.PositionHandle(p.endPoint+lp,q)-lp;
					p.startTangent = Handles.PositionHandle(p.startTangent+lp,q)-lp;
					p.endTangent = Handles.PositionHandle(p.endTangent+lp,q)-lp;
					t.Points[i] = p;
				}
			}
			HandleUtility.Repaint();
		}
#endregion


#region Change heights process
		void DoMaking()
		{
			var t = (TerrainMountainRoadMaker)target;
			var td = t.terrain.terrainData;
			var hr = td.heightmapResolution;
			var h = td.GetHeights(0,0,hr,hr);
			var h_c = (float[,])h.Clone();

			var lp = t.transform.position;

			// for painting.
			var ar = td.alphamapResolution;
			var dr = (float)hr/(float)ar;
			var a = td.GetAlphamaps(0,0,ar,ar);
			var a_c = (float[,,])a.Clone();

			t.hightUndo.Add(h_c);
			t.alphaUndo.Add(a_c);

			pointsForHeight.Clear();
			foreach (var p in t.Points)
			{
				pointsForHeight.Add(Handles.MakeBezierPoints(
					p.startPoint+lp,
					p.endPoint+lp,
					p.startTangent+lp,
					p.endTangent+lp,
					t.roadResolution
				));
			}

			foreach (var pfhs in pointsForHeight)
			{
				foreach (var pfh in pfhs)
				{
					var p = t.transform.InverseTransformPoint(pfh);
					var hx = Mathf.RoundToInt(p.x/td.size.x*hr);
					var hz = Mathf.RoundToInt(p.z/td.size.z*hr);

					var hy = Mathf.Clamp01(p.y/td.size.y);

					var hv2 = new Vector2(hx,hz);
					var round = t.roadWidth*(2-t.roundingCorner);

					for(var z=hz-t.roadWidth;z<hz+t.roadWidth;z++)
					{
						for(var x=hx-t.roadWidth;x<hx+t.roadWidth;x++)
						{
							var dist = Vector2.Distance(new Vector2(x,z),hv2);

							if(x<0 || x>=hr || z<0 || z>=hr)
								continue;
							if(dist>round)
								continue;

							h[z,x] = hy;

							//painting.
							if(t.splatIndex>-1)
							{
								var ax = Mathf.RoundToInt(x*dr);
								var az = Mathf.RoundToInt(z*dr);

								for(var ii=0;ii<a.GetLength(2);ii++)
									a[az,ax,ii] = 0f;
								a[az,ax,t.splatIndex] = 1f;
							}
						}
					}

				}
			}

			td.SetHeightsDelayLOD(0,0,h);
#if UNITY_2020_3_OR_NEWER
			t.terrain.terrainData.SyncHeightmap();
#else
			t.terrain.ApplyDelayedHeightmapModification();
#endif
			td.SetAlphamaps(0,0,a);
		}
#endregion
	}
}