using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FooModel : MonoBehaviour
{
    public UIDocument uiDocument;
    private VisualElement rootVisualElement;

    // TODO :: Add your Event here
    public VisualElement root;
	public Button btn;
	public Toggle toggle;
	public ListView listView;
	public readonly string rootName = "root";
	public readonly string btnName = "btn";
	public readonly string toggleName = "toggle";
	public readonly string listViewName = "listView";
	
	private bool _initSuccess = false;
	public bool InitSuccess => _initSuccess;

    private void OnEnable()
    {
        StartCoroutine(Init());    
    }        
                
    private void OnDisable()
    { 
        StopAllCoroutines();
    }
                
    // Note :: There is a UnityEngine's bug of rootVisualElement initialization.
    // So, I used Coroutine to wait for rootVisualElement initialization.
    private IEnumerator Init()
    {
        while(uiDocument is null)
            yield return null;

        while(uiDocument.rootVisualElement is null)
            yield return null;
                    
        rootVisualElement = uiDocument.rootVisualElement;

        // element's Queries
        root = rootVisualElement.Q<VisualElement>(rootName);
		btn = rootVisualElement.Q<Button>(btnName);
		toggle = rootVisualElement.Q<Toggle>(toggleName);
		listView = rootVisualElement.Q<ListView>(listViewName);

		_initSuccess = true;
    }
}
