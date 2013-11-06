using UnityEngine;
using System.Collections;
using strange.extensions.context.impl;

public class TestContextView : ContextView {

	
	void Awake()
		{
			//Instantiate the context, passing it this instance and a 'true' for autoStartup.
			//You might pass 'false' if you needed to await some kind of asynchronous bootstrapping
			//before launching the app. In that case, you would have to call Context's Launch()
			//method manually.
			context = new TestContext(this, true);
			context.Start ();
		}

	
	
}
