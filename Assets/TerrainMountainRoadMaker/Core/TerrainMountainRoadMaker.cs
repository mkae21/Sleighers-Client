using UnityEngine;
using System;
using System.Collections.Generic;


namespace TerrainMountainRoadMaker
{
	[Serializable]
	public struct BezierInfo
	{
		public string label;
		public Vector3 startPoint;
		public Vector3 startTangent;
		public Vector3 endPoint;
		public Vector3 endTangent;

		public BezierInfo(string Label,Vector3 StartPoint,Vector3 EndPoint,Vector3 StartTangent,Vector3 EndTangent)
		{
			label = Label;
			startPoint = StartPoint;
			startTangent = StartTangent;
			endPoint = EndPoint;
			endTangent = EndTangent;
		}
	}

	public class TerrainMountainRoadMaker : MonoBehaviour
	{
		#region variables
		[Header("setting")]
		public int roadWidth = 5;
		public int roadResolution = 250;
		public float roundingCorner = 1f;

		[Header("visibility")]
		public bool showLabel = true;
		public bool showArrows = true;
		public bool showSpheres = true;
		public bool showLines = true;

		[Header("bezier info")]
		public bool directInput;
		public List<BezierInfo> Points = new List<BezierInfo>();

		[Header("Undo & Redo")]
		public List<float[,]> hightUndo = new List<float[,]>();
		public List<float[,,]> alphaUndo = new List<float[,,]>();

		[Header("Painting")]
		public int splatIndex = -1;

		[Header("hidden variables")]
		[HideInInspector] public GUIStyle style = new GUIStyle();
		[HideInInspector] public Terrain _terrain;
		[HideInInspector] public TerrainCollider _collider;
		#endregion


		public Terrain terrain
		{
			get
			{
				if(_terrain==null)
					_terrain = GetComponent<Terrain>();
				return _terrain;
			}
		}
		new public TerrainCollider collider
		{
			get
			{
				if(_collider==null)
					_collider = GetComponent<TerrainCollider>();
				return _collider;
			}
		}
	}
}
