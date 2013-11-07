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
		
		
		//Command binding
		commandBinder.Bind(TestEvent.TEST_VIEW_INIT_EVENT).To<TestCommand>();
		
		//commandBinder.Bind(ContextEvent.START).To<StartCommand>().Once();
	}
}
