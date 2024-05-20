using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[ExecuteInEditMode]
public class MiniMapController : MonoBehaviour {
#region PublicVariables
	[HideInInspector] public Transform shapeColliderGO;
	public RenderTexture renderTex;
	public Material mapMaterial;
	[HideInInspector] public List<MiniMapEntity> miniMapEntities;
	public GameObject iconPref;
	[HideInInspector] Camera mapCamera;

	[Tooltip("The target which the minimap will be following")]
	public Transform target;
	//UI related variables
	[Tooltip("Set which layers to show in the minimap")]
	public LayerMask minimapLayers;
	[Tooltip("Set this true, if you want minimap border as background of minimap")]
	public bool showBackground;
	[Tooltip("The mask to change the shape of minimap")]
	public Sprite miniMapMask;
	[Range(0,1)] public float miniMapOpacity = 1f;
	[Tooltip("border graphics of the minimap")]
	public Vector3 miniMapScale = new Vector3(1,1,1);

	//Render camera related variables
	[Tooltip("Camera offset from the target")]
	public Vector3 cameraOffset = new Vector3(0f, 7.5f, 0f);
	[Tooltip("Camera's orthographic size")]
	public float camSize = 15;
	[Tooltip("Camera's far clip")]
	public float camFarClip = 1000;
	[Tooltip("Adjust the rotation according to your scene")]
	public Vector3 rotationOfCam = new Vector3(90,0,0);
	[Tooltip("If true the camera rotates according to the target")]
	public bool rotateWithTarget = true;
	[HideInInspector] public Dictionary<GameObject, GameObject> ownerIconMap = new Dictionary<GameObject, GameObject>();
#endregion

#region PrivateVariables
	private GameObject miniMapPanel;
	private Image mapPanelMask;
	private Image mapPanel;
	private Color mapColor;
	private RectTransform mapPanelRect;
	private RectTransform mapPanelMaskRect;
	private Vector2 res;
	private Image miniMapPanelImage;
#endregion

	//Initialize everything here
	public void OnEnable(){
		ownerIconMap.Clear ();
		GameObject maskPanelGO = transform.GetComponentInChildren<Mask> ().gameObject;
		mapPanelMask = maskPanelGO.GetComponent<Image> ();
		miniMapPanel = maskPanelGO.transform.GetChild (0).gameObject;
		mapPanel = miniMapPanel.GetComponent<Image> ();
		mapColor = mapPanel.color;
		if (mapCamera == null)
			mapCamera = transform.GetComponentInChildren<Camera>();
		mapCamera.cullingMask = minimapLayers;

		mapPanelMaskRect = maskPanelGO.GetComponent<RectTransform> ();
		mapPanelRect = miniMapPanel.GetComponent<RectTransform> ();
		mapPanelRect.anchoredPosition = mapPanelMaskRect.anchoredPosition;
		res = new Vector2(Screen.width,Screen.height);

		miniMapPanelImage = miniMapPanel.GetComponent<Image> ();
		miniMapPanelImage.enabled = !showBackground;
		SetupRenderTexture();
	}
	//Release the unmanaged objects
	private void OnDisable()
	{
		if (renderTex != null) 
			if (!renderTex.IsCreated ())
				renderTex.Release ();
	}

	//Release the unmanaged objects
	void OnDestroy()
	{
		if (renderTex != null) 
			if (!renderTex.IsCreated()) 
				renderTex.Release();
	}

	//As this script is ExecuteInEditMode, this function will be called when something in scene changes
	public void LateUpdate(){
		//Set minimap images and colors
		mapPanelMask.sprite = miniMapMask;
		mapColor.a = miniMapOpacity;
		mapPanel.color = mapColor;

		//Set minimappanel size and position, so it updates with size and resolution changes
		mapPanelMaskRect.sizeDelta = new Vector2(Mathf.RoundToInt(mapPanelMaskRect.sizeDelta.x),Mathf.RoundToInt(mapPanelMaskRect.sizeDelta.y));
		mapPanelRect.position = mapPanelMaskRect.position;
		mapPanelRect.sizeDelta = mapPanelMaskRect.sizeDelta;
		miniMapPanelImage.enabled = !showBackground;

		if (Screen.width != res.x || Screen.height != res.y) 
		{
			//Set the render texture
			SetupRenderTexture ();
			res.x = Screen.width;
			res.y = Screen.height;
		}
		//Set the camera
		SetCam ();
	}
	void SetupRenderTexture(){
		//Release the old texture, otherwise memory leak happens
		//This line shows as error log in Unity versions < 5.4, which is a Unity bug. But harmless.
		if(renderTex.IsCreated()) 
			renderTex.Release ();
		//Setup render texture and resize it.
		//New render texture was created, as premade render texture's size can't be changed
		renderTex = new RenderTexture ((int)mapPanelRect.sizeDelta.x, (int)mapPanelRect.sizeDelta.y, 24);
		//Create only creates new render texture in memory, if it is not already created
		renderTex.Create ();

		mapMaterial.mainTexture = renderTex;
		mapCamera.targetTexture = renderTex;

		//Cheat to refresh the minimap panel texture;
		mapPanelMaskRect.gameObject.SetActive (false);
		mapPanelMaskRect.gameObject.SetActive (true);
	}

	void SetCam(){
		mapCamera.orthographicSize = camSize;
		mapCamera.farClipPlane = camFarClip;
		if (target)
		{
			mapCamera.transform.eulerAngles = rotationOfCam;

			if (rotateWithTarget) 
				mapCamera.transform.eulerAngles = target.eulerAngles + rotationOfCam;
			mapCamera.transform.position = target.position + cameraOffset;
		}
	}

	//Register's minimap objects here
	public MapObject RegisterMapObject(GameObject owner, MiniMapEntity mme){
		GameObject curMGO = Instantiate (iconPref);
		MapObject curMO = curMGO.AddComponent<MapObject> ();
		curMO.SetMiniMapEntityValues (this,mme,owner,mapCamera,miniMapPanel);

		if(!ownerIconMap.ContainsKey(owner))
			ownerIconMap.Add(owner, curMGO);
		else 
       		ownerIconMap[owner] = curMGO; // 현재 인스턴스로 업데이트	
		return owner.GetComponent<MapObject>();
	}

	//Unregister's minimap objects here
	public void UnregisterMapObject(MapObject mmo, GameObject owner)
	{
		if (ownerIconMap.ContainsKey (owner)) 
		{
			Destroy (ownerIconMap [owner]);
			ownerIconMap.Remove (owner);
		}
		Destroy (mmo);
	}
	public void SetTarget(Transform _target)
	{
		target = _target;
	}
}
