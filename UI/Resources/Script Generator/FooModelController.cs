using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(FooModel))]
public class FooModelController : MonoBehaviour
{
   [SerializeField] private FooModel fooModel;
   // TODO :: Add your Event here
   
   private void OnEnable()
   {
        StartCoroutine(Init());    
   }        
                
   private void OnDisable()
   { 
        StopAllCoroutines();
   }
                
   // Note :: There is a UnityEngine's bug of rootVisualElement initialization.
   //        So, I used Coroutine to wait for rootVisualElement initialization.
   private IEnumerator Init()
   {
        while(fooModel is null)
            yield return null;

        while(fooModel.uiDocument.rootVisualElement is null || !fooModel.InitSuccess)
            yield return null;

        // element's events
		fooModel.btn.clicked += OnBtnClicked;

		fooModel.toggle.RegisterValueChangedCallback(OnToggleValueChanged);

		fooModel.listView.itemsAdded += OnListViewItemsAdded;
        fooModel.listView.itemsRemoved += OnListViewItemsRemoved;
        fooModel.listView.itemsSourceChanged += OnListViewitemsSourceChanged;
        fooModel.listView.itemsChosen += OnListViewItemsChosen;
        fooModel.listView.makeItem += OnListViewMakeItem;
        fooModel.listView.bindItem += OnListViewBindItem;
        fooModel.listView.destroyItem += OnListViewDestroyItem;
        fooModel.listView.unbindItem += OnListViewUnbindItem;
   }
                
    // TODO :: Implement your event callback here 
	private void OnBtnClicked() 
    {
        throw new System.NotImplementedException();    
    }
	private void OnToggleValueChanged(ChangeEvent<bool> evt)
    {
        throw new System.NotImplementedException();    
    }
	private void OnListViewItemsAdded(IEnumerable<int> obj)
    {
        throw new System.NotImplementedException();    
    }

    private void OnListViewItemsRemoved(IEnumerable<int> obj)
    {
        throw new System.NotImplementedException();    
    }

    private void OnListViewitemsSourceChanged()
    {
        throw new System.NotImplementedException();    
    }
                          
    private void OnListViewItemsChosen(IEnumerable<object> obj)
    {
        throw new System.NotImplementedException();    
    }    
                          
    private VisualElement OnListViewMakeItem()
    {
        throw new System.NotImplementedException();    
    }    
                          
    private void OnListViewBindItem(VisualElement visualElement, int index)
    {
        throw new System.NotImplementedException();    
    }
                          
    private void OnListViewDestroyItem(VisualElement visualElement)
    {
        throw new System.NotImplementedException();    
    }
                          
    private void OnListViewUnbindItem(VisualElement visualElement, int index)
    {
        throw new System.NotImplementedException();    
    }
}
