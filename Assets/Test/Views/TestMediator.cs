using UnityEngine;
using System.Collections;
using strange.extensions.mediation.impl;

public class TestMediator : EventMediator {

	[Inject]
    public TestView view { get; set; }
	
	 public override void OnRegister(){
		
		view.init();
	 }
	
	public override void OnRemove(){
		
		
	}

}
