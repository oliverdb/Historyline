using UnityEngine;
using System.Collections;
using strange.extensions.mediation.impl;
public class TestView : View {
	
	
    internal void init()
    {
		
	}
	
	void OnGUI() {
		GUI.Label(new Rect(200,200,200,200),"Hello view");
	}
}
