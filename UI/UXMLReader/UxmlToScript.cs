using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class UxmlToScript : MonoBehaviour
{
    [SerializeField] private VisualTreeAsset uxml;
    [SerializeField] private string uxmlPath;
    [SerializeField] private PanelSettings panelSettings;
    [SerializeField] private float sortOrder;

    private StringBuilder _classFrameBuilder = new StringBuilder(1000);
    private StringBuilder _classFieldBuilder = new StringBuilder(1000);
    
    private string _ignorePrefixName = "___";

    [SerializeField] private string modelScriptName;
    [SerializeField] private string controllerScriptName;
    [SerializeField] private string savePath;
    [SerializeField] private Transform instanceParent;
    [SerializeField] private UIAnimator animatorPrefab;

    private List<(string type, string name)> _elementInfos = new List<(string type, string name)>(128);
   
    public void CreateModel()
    {
        if (uxml is null)
        {
            Debug.Log($"There is no uxml.");
            return;
        }
        GetVisualElementNames();

        CreateModelMonoScript(modelScriptName);
    }
    public void CreateModelCtrl()
    {
        if (uxml is null)
        {
            Debug.Log($"There is no uxml.");
            return;
        }
        GetVisualElementNames();

        CreateCtrlMonoScript(controllerScriptName, modelScriptName);
    }

    // can use runtime when initializing.
    public void InstantiateModelWithController()
    {
        Type modelType = Type.GetType(modelScriptName);
        Type modelControllerType = Type.GetType(controllerScriptName);

        if (modelType is null)
        {
            Debug.Log($"{modelScriptName} is not create yet.");
            return;
        }
        if (modelControllerType is null) 
        {
            Debug.Log($"{controllerScriptName} is not create yet.");
            return;
        }

        // Create Instance
        GameObject instance = new GameObject($"{modelType} UI Instance");

        // Add Components
        Component docuemnt = instance.AddComponent(typeof(UIDocument));
        Component model = instance.AddComponent(modelType);
        Component controller = instance.AddComponent(modelControllerType);
        
        // inject UI Document ref
        FieldInfo modelUIDocumentField = instance.GetComponent(modelType).GetType().GetField("uiDocument");
        modelUIDocumentField.SetValue(model, docuemnt );

        // inject model type ref
        Span<char> modelName = new Span<char>(modelType.ToString().ToCharArray());
        modelName[0] = char.ToLower(modelName[0]);
        string fieldName = modelName.ToString();
        FieldInfo controllerModelField = instance.GetComponent(modelControllerType).GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
        controllerModelField?.SetValue(controller, model );
        
        // Set UIDocument
        if (uxml is null)
        {
            Debug.Log($"if assign uxml , automatically inject to UIDocument of instance's Component.");
        }

        UIDocument docRef = (UIDocument)docuemnt;
        docRef.visualTreeAsset = uxml;
        if (panelSettings is null)
        {
            Debug.Log($"if assign PanelSettings , automatically inject to UIDocument of instance's Component.");
        }
        docRef.panelSettings = panelSettings;
        docRef.sortingOrder = sortOrder;
        
        // Set Parent
        if(instanceParent is not null)
            instance.transform.SetParent(instanceParent);

        if (animatorPrefab is null)
        {
            Debug.Log($"if assign _animatorPrefab , automatically inject to UIDocument of instance's Component.");
            return;
        }
        UIAnimator instancedUIAnimator = Instantiate(animatorPrefab, null, true);
        instancedUIAnimator.name = "UI Animator";
        DestroyImmediate(instancedUIAnimator.GetComponent(typeof(UIDocument)));
        instancedUIAnimator.transform.SetParent(instance.transform);
        instancedUIAnimator.dataController.UIDocument = docRef;
    }

    public void GetUxmlPath()
    {
#if UNITY_EDITOR
        if(uxml)
            uxmlPath = AssetDatabase.GetAssetPath(uxml);
#endif
    }
    private void GetVisualElementNames()
    {
        _elementInfos.Clear();
        XDocument document = XDocument.Load(uxmlPath);
        IEnumerable<(string type, string name)> elements = document.Elements().Descendants().Select(i => (i.Name.LocalName, i.Attribute("name")?.Value));

        foreach ((string type, string name) element in elements)
        {
            // ignore empty name element
            if (string.IsNullOrEmpty(element.name)) continue;
            // ignore case
            if (element.name.Contains(_ignorePrefixName)) continue;
            
            _elementInfos.Add(element);
        }
    }
    private void CreateModelMonoScript(string monoName)
    {
        string filePath = $"{savePath}/{monoName}.cs";

        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);

        using FileStream fs = new FileStream(filePath, FileMode.Truncate,FileAccess.ReadWrite, FileShare.ReadWrite);
        using StreamWriter writer = new StreamWriter(fs);
        string code = GenerateModelClass(monoName);
        writer.Write(code);
        writer.Close();
#if UNITY_EDITOR
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        Object result = AssetDatabase.LoadAssetAtPath<Object>(filePath);
        EditorGUIUtility.PingObject(result);
#endif
    }
    private string GenerateModelClass(string monoName)
    {
        _classFrameBuilder.Clear();
        
        _classFrameBuilder.Append(
            $@"using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class {monoName} : MonoBehaviour
{{
    public UIDocument uiDocument;
    private VisualElement rootVisualElement;

    // TODO :: Add your Event here
    {GenerateField(_elementInfos)}
	private bool _initSuccess = false;
	public bool InitSuccess => _initSuccess;

    private void OnEnable()
    {{
        StartCoroutine(Init());    
    }}        
                
    private void OnDisable()
    {{ 
        StopAllCoroutines();
    }}
                
    // Note :: There is a UnityEngine's bug of rootVisualElement initialization.
    // So, I used Coroutine to wait for rootVisualElement initialization.
    private IEnumerator Init()
    {{
        while(uiDocument is null)
            yield return null;

        while(uiDocument.rootVisualElement is null)
            yield return null;
                    
        rootVisualElement = uiDocument.rootVisualElement;

        // element's Queries
        {(GenerateInitModelQuery())}

		_initSuccess = true;
    }}
}}
");
        return _classFrameBuilder.ToString();
    }
    private void CreateCtrlMonoScript(string monoName,string modelMonoName)
    {
        string filePath =  $"{savePath}/{monoName}.cs";

        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);
        
        using FileStream fs = new FileStream(filePath, FileMode.Truncate,FileAccess.ReadWrite, FileShare.ReadWrite);
        using StreamWriter writer = new StreamWriter(fs);
        string code = GenerateCtrlClass(monoName, modelMonoName);
        writer.Write(code);
        writer.Close();

#if UNITY_EDITOR
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        Object result = AssetDatabase.LoadAssetAtPath<Object>(filePath);
        EditorGUIUtility.PingObject(result);
#endif
    }
    private string GenerateCtrlClass(string monoName, string modelMonoName)
    {
        _classFrameBuilder.Clear();
        Span<char> newName = new Span<char>(modelMonoName.ToCharArray());
        newName[0] = char.ToLower(newName[0]);
        string modelMonoVariableName = newName.ToString();
        _classFrameBuilder.Append(
            $@"using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof({modelMonoName}))]
public class {monoName} : MonoBehaviour
{{
   [SerializeField] private {modelMonoName} {modelMonoVariableName};
   // TODO :: Add your Event here
   
   private void OnEnable()
   {{
        StartCoroutine(Init());    
   }}        
                
   private void OnDisable()
   {{ 
        StopAllCoroutines();
   }}
                
   // Note :: There is a UnityEngine's bug of rootVisualElement initialization.
   //        So, I used Coroutine to wait for rootVisualElement initialization.
   private IEnumerator Init()
   {{
        while({modelMonoVariableName} is null)
            yield return null;

        while({modelMonoVariableName}.uiDocument.rootVisualElement is null || !{modelMonoVariableName}.InitSuccess)
            yield return null;

        // element's events
{(GenerateCtrlRegisterCallbacks(modelMonoVariableName))}
   }}
                
    // TODO :: Implement your event callback here 
{(GenerateCtrlCallbackMethods())}
}}
");
        return _classFrameBuilder.ToString();
    }

    private string GenerateField(List<(string type, string name)> elementInfos)
    {
        _classFieldBuilder.Clear();
        
        // visualElement field
        for (int i = 0; i < elementInfos.Count; i++)
        {
            (string type, string name) currentElement = elementInfos[i];
            _classFieldBuilder.Append($"public {currentElement.type} {currentElement.name};\n\t");
        }
        // visualElement's name field
        for (int i = 0; i < elementInfos.Count; i++)
        {
            (string type, string name) currentElement = elementInfos[i];
            _classFieldBuilder.Append($"public readonly string {currentElement.name}Name = \"{currentElement.name}\";\n\t");
        }

        return _classFieldBuilder.ToString();
    }

    private string GenerateInitModelQuery()
    {
        _classFieldBuilder.Clear();
        
        // visualElement Query
        for (int i = 0; i < _elementInfos.Count; i++)
        {
            (string type, string name) currentElement = _elementInfos[i];
            _classFieldBuilder.Append(i == _elementInfos.Count - 1
                ? $"{currentElement.name} = rootVisualElement.Q<{currentElement.type}>({currentElement.name}Name);"
                : $"{currentElement.name} = rootVisualElement.Q<{currentElement.type}>({currentElement.name}Name);\n\t\t");
        }

        return _classFieldBuilder.ToString();
    }
    private string GenerateCtrlRegisterCallbacks(string modelMonoVariableName)
    {
        _classFieldBuilder.Clear();
        
        // visualElement Query
        for (int i = 0; i < _elementInfos.Count; i++)
        {
            (string type, string name) currentElement = _elementInfos[i];
            string snippet =
                GenerateCallbackByVisualElementType(modelMonoVariableName, currentElement.type, currentElement.name);
            if(string.IsNullOrEmpty(snippet)) continue;
            if(i == _elementInfos.Count - 1)
                _classFieldBuilder.Append($"\t\t{snippet}");
            else
                _classFieldBuilder.AppendLine($"\t\t{snippet}\n");
        }

        return _classFieldBuilder.ToString();
    }
    private string GenerateCtrlCallbackMethods()
    {
        _classFieldBuilder.Clear();
        
        // visualElement Query
        for (int i = 0; i < _elementInfos.Count; i++)
        {
            (string type, string name) currentElement = _elementInfos[i];
            string snippet = GenerateCallbackMethod(currentElement.type, currentElement.name);
            if(string.IsNullOrEmpty(snippet)) continue;
            if(i == 0 || i ==_elementInfos.Count - 1)
                _classFieldBuilder.Append($"\t{snippet}");
            else
                _classFieldBuilder.AppendLine($"\t{snippet}");
        }

        return _classFieldBuilder.ToString();
    }

    #region Register
    private string GenerateCallbackByVisualElementType(string modelMonoVariableName, string type, string name)
    {
        Span<char> replaceFirstCharacterString = new Span<char>(name.ToCharArray());
        replaceFirstCharacterString[0] = char.ToUpper(replaceFirstCharacterString[0]);
        string newName = replaceFirstCharacterString.ToString();
        switch (type)
        {
            default:
                return string.Empty;
            case nameof(Button):
                return $@"{modelMonoVariableName}.{name}.clicked += On{newName}Clicked;";
            case nameof(ScrollView):
                return $@"{modelMonoVariableName}.{name}.verticalScroller.valueChanged += On{newName}VerticalValueChanged;
        {modelMonoVariableName}.{name}.horizontalScroller.valueChanged += On{newName}HorizontalValueChanged;";
            case nameof(ListView):
                return $@"{modelMonoVariableName}.{name}.itemsAdded += On{newName}ItemsAdded;
        {modelMonoVariableName}.{name}.itemsRemoved += On{newName}ItemsRemoved;
        {modelMonoVariableName}.{name}.itemsSourceChanged += On{newName}itemsSourceChanged;
        {modelMonoVariableName}.{name}.itemsChosen += On{newName}ItemsChosen;
        {modelMonoVariableName}.{name}.makeItem += On{newName}MakeItem;
        {modelMonoVariableName}.{name}.bindItem += On{newName}BindItem;
        {modelMonoVariableName}.{name}.destroyItem += On{newName}DestroyItem;
        {modelMonoVariableName}.{name}.unbindItem += On{newName}UnbindItem;";
            case nameof(TreeView):
                return $@"{modelMonoVariableName}.{name}.itemsSourceChanged += On{newName}itemsSourceChanged;
        {modelMonoVariableName}.{name}.itemsChosen += On{newName}ItemsChosen;
        {modelMonoVariableName}.{name}.makeItem += On{newName}MakeItem;
        {modelMonoVariableName}.{name}.bindItem += On{newName}BindItem;
        {modelMonoVariableName}.{name}.destroyItem += On{newName}DestroyItem;
        {modelMonoVariableName}.{name}.unbindItem += On{newName}UnbindItem;";
           case nameof(Scroller):
                return $@"{modelMonoVariableName}.{name}.valueChanged += On{newName}ValueChanged;";
            case nameof(Toggle):
            case nameof(TextField):
            case nameof(Foldout):
            case nameof(Slider):
            case nameof(SliderInt):
            case nameof(MinMaxSlider):
            case nameof(ProgressBar):
            case nameof(DropdownField):
            case nameof(EnumField):
            case nameof(RadioButton):
            case nameof(RadioButtonGroup):
            case nameof(IntegerField):
            case nameof(FloatField):
            case nameof(LongField):
            case nameof(DoubleField):
            case nameof(Hash128Field):
            case nameof(Vector2Field):
            case nameof(Vector3Field):
            case nameof(Vector4Field):
            case nameof(Vector2IntField):
            case nameof(Vector3IntField):
                return $@"{modelMonoVariableName}.{name}.RegisterValueChangedCallback(On{newName}ValueChanged);";
            // editor-Only
#if UNITY_EDITOR
            case nameof(IMGUIContainer):
            case nameof(UnityEditor.UIElements.ColorField):
            case nameof(UnityEditor.UIElements.Toolbar):
            case nameof(UnityEditor.UIElements.ToolbarMenu):
            case nameof(UnityEditor.UIElements.ToolbarSpacer):
            case nameof(UnityEditor.UIElements.ToolbarBreadcrumbs):
            case nameof(UnityEditor.UIElements.ToolbarSearchField):
                return string.Empty;
            case nameof(UnityEditor.UIElements.CurveField):
            case nameof(UnityEditor.UIElements.GradientField):
            case nameof(UnityEditor.UIElements.TagField):
            case nameof(UnityEditor.UIElements.MaskField):
            case nameof(UnityEditor.UIElements.LayerField):
            case nameof(UnityEditor.UIElements.LayerMaskField):
            case nameof(UnityEditor.UIElements.EnumFlagsField):
            case nameof(UnityEditor.UIElements.ToolbarToggle):
            case nameof(UnityEditor.UIElements.ToolbarPopupSearchField):
            case nameof(UnityEditor.UIElements.ObjectField):
            case nameof(UnityEditor.UIElements.PropertyField):
                return $@"{modelMonoVariableName}.{name}.RegisterValueChangedCallback(On{newName}ValueChanged);
";
#endif
        }
    }
    #endregion

    #region CallbackMethods
    private string GenerateCallbackMethod(string type, string name)
    {
        Span<char> replaceFirstCharacterString = new Span<char>(name.ToCharArray());
        replaceFirstCharacterString[0] = char.ToUpper(replaceFirstCharacterString[0]);
        string newName = replaceFirstCharacterString.ToString();
        
        switch (type)
        {
            default:
                return string.Empty;
            case nameof(Button):
                return $@"private void On{newName}Clicked() 
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(ScrollView):
                return $@"private void On{newName}VerticalValueChanged(float changedValue)
    {{
        throw new System.NotImplementedException();    
    }}
                          
    private void On{newName}HorizontalValueChanged(float changedValue)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(ListView):
                return $@"private void On{newName}ItemsAdded(IEnumerable<int> obj)
    {{
        throw new System.NotImplementedException();    
    }}

    private void On{newName}ItemsRemoved(IEnumerable<int> obj)
    {{
        throw new System.NotImplementedException();    
    }}

    private void On{newName}itemsSourceChanged()
    {{
        throw new System.NotImplementedException();    
    }}
                          
    private void On{newName}ItemsChosen(IEnumerable<object> obj)
    {{
        throw new System.NotImplementedException();    
    }}    
                          
    private VisualElement On{newName}MakeItem()
    {{
        throw new System.NotImplementedException();    
    }}    
                          
    private void On{newName}BindItem(VisualElement visualElement, int index)
    {{
        throw new System.NotImplementedException();    
    }}
                          
    private void On{newName}DestroyItem(VisualElement visualElement)
    {{
        throw new System.NotImplementedException();    
    }}
                          
    private void On{newName}UnbindItem(VisualElement visualElement, int index)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(TreeView):
                return $@"private void On{newName}itemsSourceChanged()
    {{
        throw new System.NotImplementedException();    
    }}
                          
    private void On{newName}ItemsChosen(IEnumerable<object> obj)
    {{
        throw new System.NotImplementedException();    
    }}

    private VisualElement On{newName}MakeItem()
    {{
        throw new System.NotImplementedException();    
    }}

    private void On{newName}BindItem(VisualElement visualElement, int index)
    {{
        throw new System.NotImplementedException();    
    }}

    private void On{newName}DestroyItem(VisualElement visualElement)
    {{
        throw new System.NotImplementedException();    
    }}

    private void On{newName}UnbindItem(VisualElement visualElement, int index)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(ProgressBar):
            case nameof(Slider):
           case nameof(Scroller):
            case nameof(FloatField):
                return $@"private void On{newName}ValueChanged(ChangeEvent<float> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(RadioButton):
            case nameof(Toggle):
            case nameof(Foldout):
                return $@"private void On{newName}ValueChanged(ChangeEvent<bool> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(TextField):
                return $@"private void On{newName}ValueChanged(ChangeEvent<string> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(IntegerField):
            case nameof(DropdownField):
            case nameof(SliderInt):
                return $@"private void On{newName}ValueChanged(ChangeEvent<int> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(Vector2Field):
            case nameof(MinMaxSlider):
                return $@"private void On{newName}ValueChanged(ChangeEvent<Vector2> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(EnumField):
            case nameof(RadioButtonGroup):
                return $@"private void On{newName}ValueChanged(ChangeEvent<Enum> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(LongField):
                return $@"private void On{newName}ValueChanged(ChangeEvent<long> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(DoubleField):
                return $@"private void On{newName}ValueChanged(ChangeEvent<double> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(Hash128Field):
                return $@"private void On{newName}ValueChanged(ChangeEvent<Hash128> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(Vector3Field):
                return $@"private void On{newName}ValueChanged(ChangeEvent<Vector3> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(Vector4Field):
                return $@"private void On{newName}ValueChanged(ChangeEvent<Vector4> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(Vector2IntField):
                return $@"private void On{newName}ValueChanged(ChangeEvent<Vector2Int> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(Vector3IntField):
                return $@"private void On{newName}ValueChanged(ChangeEvent<Vector3Int> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
            // editor-Only
#if UNITY_EDITOR
            case nameof(IMGUIContainer):
            case nameof(UnityEditor.UIElements.ColorField):
            case nameof(UnityEditor.UIElements.Toolbar):
            case nameof(UnityEditor.UIElements.ToolbarMenu):
            case nameof(UnityEditor.UIElements.ToolbarSpacer):
            case nameof(UnityEditor.UIElements.ToolbarBreadcrumbs):
            case nameof(UnityEditor.UIElements.ToolbarSearchField):
                return string.Empty;
            case nameof(UnityEditor.UIElements.CurveField):
                return $@"private void On{newName}ValueChanged(ChangeEvent<AnimationCurve> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(UnityEditor.UIElements.GradientField):
                return $@"private void On{newName}ValueChanged(ChangeEvent<Gradient> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(UnityEditor.UIElements.TagField):
            case nameof(UnityEditor.UIElements.ToolbarPopupSearchField):
                return $@"private void On{newName}ValueChanged(ChangeEvent<string> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(UnityEditor.UIElements.MaskField):
            case nameof(UnityEditor.UIElements.LayerField):
                return $@"private void On{newName}ValueChanged(ChangeEvent<int> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(UnityEditor.UIElements.LayerMaskField):
                return $@"private void On{newName}ValueChanged(ChangeEvent<LayerMask> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(UnityEditor.UIElements.EnumFlagsField):
                return $@"   private void On{newName}ValueChanged(ChangeEvent<Enum> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(UnityEditor.UIElements.ToolbarToggle):
                return $@"private void On{newName}ValueChanged(ChangeEvent<bool> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
            case nameof(UnityEditor.UIElements.ObjectField):
            case nameof(UnityEditor.UIElements.PropertyField):
                return $@"private void On{newName}ValueChanged(ChangeEvent<UnityEngine.Object> evt)
    {{
        throw new System.NotImplementedException();    
    }}";
#endif
        }
    }
    #endregion
}
