using System.Collections.Generic;
using Unity.Entities.Serialization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class AdditionalTools : EditorWindow
{
    private List<UntypedWeakReferenceId> IDsList = new();

    private Button _createWeakRefIdButton;

    [MenuItem("Tools/Additional tools")]
    public static void Open() => GetWindow<AdditionalTools>().titleContent = new GUIContent("Additional tools");

    void CreateGUI()
    {
        var root = rootVisualElement;
        root.style.paddingLeft = 10;
        root.style.paddingRight = 10;

        root.Add(new Label("Create Weak Reference Id"));
        _createWeakRefIdButton = root.CreateChild<Button>();
        _createWeakRefIdButton.text = "Create Id";

        _createWeakRefIdButton.RegisterCallback<ClickEvent>(GenerateAndSaveWeakRefID);
    }

    private void GenerateAndSaveWeakRefID(ClickEvent evt)
    {
        var IDs = WeakObjectReferenceIDsLog.LogWeakObjectReferenceIDs();
        IDsList.AddRange(IDs);
    }
}

public static class WeakObjectReferenceIDsLog
{
    public static List<UntypedWeakReferenceId> LogWeakObjectReferenceIDs()
    {
        Object[] selectedObjects = Selection.GetFiltered<Object>(SelectionMode.Assets);
        List<UntypedWeakReferenceId> IDs = new();
        
        for (int i = 0; i < selectedObjects.Length; i++)
        {
            var id = UntypedWeakReferenceId.CreateFromObjectInstance(selectedObjects[i]);
            Debug.Log($"Object name {selectedObjects[i].name} : {id}");
            IDs.Add(id);
        }

        return IDs;
    }
}