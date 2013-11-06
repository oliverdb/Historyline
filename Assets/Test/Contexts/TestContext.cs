using UnityEngine;
using System.Collections;
using strange.extensions.context.impl;

public class TestContext : MVCSContext {

	 public TestContext()
        : base()
    {
    }

    public TestContext(MonoBehaviour view, bool autoStartup)
        : base(view, autoStartup)
    {
    }

    protected override void mapBindings()
    {		 
		mediationBinder.Bind<TestView>().To<TestMediator>();	
	}
}
