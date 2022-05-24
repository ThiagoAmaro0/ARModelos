using UnityEngine;
using System.Collections;
using System.IO;

public class Example3_ExternalFile : MonoBehaviour {

	public string objFileName = "Car_obj.txt";
	public Material standardMaterial;
	public Material transparentMaterial;
	
	private string loadingText = "";

	IEnumerator Start () {
		loadingText = "Loading...";
		yield return null;
		yield return null;
		
		objFileName = Path.Combine(Application.persistentDataPath,"Desk.obj");
		
		ObjReader.use.ConvertFile (objFileName, false, standardMaterial, transparentMaterial);
		
		loadingText = "";
	}
	
	void OnGUI () {
		GUILayout.BeginArea (new Rect(10, 10, 400, 400));
		GUILayout.Label (loadingText);
		GUILayout.EndArea();
	}
}