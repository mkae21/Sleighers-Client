using UnityEngine;
using System;
using System.Collections.Generic;


namespace Sticker2Layer
{
	public enum RayMode
	{
		Up,
		Down
	}

	public class Sticker2Layer : MonoBehaviour
	{
		#region variables
		[Header("setting")]
		public RayMode rayMode = RayMode.Up;
		public float margin = 0.1f;
		public LayerMask layer;

		[Header("Undo & Redo")]
		public List<float[,]> undoList = new List<float[,]>();

		[Header("hidden variables")]
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
