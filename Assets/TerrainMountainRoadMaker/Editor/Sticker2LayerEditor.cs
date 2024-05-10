using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Sticker2Layer
{
	[CustomEditor(typeof(Sticker2Layer))]
	public class Sticker2LayerEditor : Editor
	{
		#region CustomInspectorGUI
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			var t = (Sticker2Layer)target;

			Undo.RecordObject(t, "Sticker2Layer");

			if(GUILayout.Button("Go"))
				Going(t);

			if(t.undoList.Count>0)
			{
				EditorGUILayout.Space();
				EditorGUILayout.BeginHorizontal();
				{
					if(GUILayout.Button("Undo"))
					{
						t.terrain.terrainData.SetHeightsDelayLOD(0,0,t.undoList[t.undoList.Count-1]);
						t.undoList.RemoveAt(t.undoList.Count-1);
					}
					if(GUILayout.Button("Clear UndoLog"))
						t.undoList.Clear();
				}
				EditorGUILayout.EndHorizontal();
			}
		}
		#endregion

		#region Change heights process
		void Going(Sticker2Layer t)
		{
			var td = t.terrain.terrainData;
			var hr = td.heightmapResolution;
			var h = td.GetHeights(0,0,hr,hr);

			var p = t.transform.position;
			var fx = td.size.x / hr;
			var fz = td.size.z / hr;

			var mode = t.rayMode==RayMode.Up;
			var fy = mode?0:td.heightmapResolution;
				
			t.undoList.Add((float[,])h.Clone());

			for (var z=0;z<hr;z++)
			{
				for (var x=0;x<hr;x++)
				{
					var pp = new Vector3(x*fx,fy,z*fz);
					var ray = new Ray(p+pp,(mode?Vector3.up:Vector3.down));
					RaycastHit hit;

					if(Physics.Raycast(ray,out hit,td.heightmapResolution,t.layer))
					{
						var y = hit.point.y - p.y - t.margin;
						h[z,x] = (y / td.size.y);
					}
				}
			}

			td.SetHeightsDelayLOD(0,0,h);
#if UNITY_2020_3_OR_NEWER
			t.terrain.terrainData.SyncHeightmap();
#else
			t.terrain.ApplyDelayedHeightmapModification();
#endif
		}
		#endregion
	}
}