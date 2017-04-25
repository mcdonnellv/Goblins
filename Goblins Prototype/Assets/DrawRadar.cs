using UnityEngine;
using System.Collections;

public class DrawRadar : MonoBehaviour {
	public float ThetaScale = 0.01f;
	public float radius = 3f;
	public float n = 1f;
	public float width = 0.02f;
	public Material mat;
	private int Size;
	private LineRenderer LineDrawer;
	private float Theta = 0f;

	void Start () {       
		LineDrawer = GetComponent<LineRenderer>();
	}

	void Update () {
		if(mat != null)
			LineDrawer.material = mat;
		LineDrawer.SetWidth(width, width); //thickness of line
		Theta = 0f;
		Size = (int)((1f / ThetaScale) + n);
		LineDrawer.SetVertexCount(Size); 
		for(int i = 0; i < Size; i++){          
			Theta += (2.0f * Mathf.PI * ThetaScale);         
			float x = radius * Mathf.Cos(Theta);
			float y = radius * Mathf.Sin(Theta);    

			x += gameObject.transform.position.x;
			y += gameObject.transform.position.y;
			Vector3 pos = new Vector3(x, y, 0);

			LineDrawer.SetPosition(i, pos);
		}
	}
}