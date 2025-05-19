// En una carpeta "Editor"
using UnityEditor;
using UnityEngine;
using System.IO;

public class HierarchyComponentViewerSettingsProvider : SettingsProvider
{
    private SerializedObject _settingsSerializedObject;

    public HierarchyComponentViewerSettingsProvider(string path, SettingsScope scope = SettingsScope.Project)
        : base(path, scope) { }

    public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
    {
        HierarchyComponentViewerSettings settings = HierarchyComponentViewerSettings.Instance;
        _settingsSerializedObject = new SerializedObject(settings);
    }

    public override void OnGUI(string searchContext)
    {
        if (_settingsSerializedObject == null || _settingsSerializedObject.targetObject == null)
        {
            // Si por alguna razón se pierde la referencia (ej. el asset fue borrado y recreado)
            HierarchyComponentViewerSettings settings = HierarchyComponentViewerSettings.Instance;
            _settingsSerializedObject = new SerializedObject(settings);
        }

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(_settingsSerializedObject.FindProperty("maxComponentsPerPage"), new GUIContent("Max Components Per Page"));
        EditorGUILayout.PropertyField(_settingsSerializedObject.FindProperty("showComponentTypeName"), new GUIContent("Show Component Type Name"));
        EditorGUILayout.PropertyField(_settingsSerializedObject.FindProperty("disabledComponentAlpha"), new GUIContent("Disabled Component Alpha"));
        EditorGUILayout.PropertyField(_settingsSerializedObject.FindProperty("componentIconWidth"), new GUIContent("Component Icon Width"));
        EditorGUILayout.PropertyField(_settingsSerializedObject.FindProperty("componentIconSpacing"), new GUIContent("Component Icon Spacing"));

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tag and Layer Display", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_settingsSerializedObject.FindProperty("showGameObjectTag"), new GUIContent("Show GameObject Tag"));
        EditorGUILayout.PropertyField(_settingsSerializedObject.FindProperty("tagFieldWidth"), new GUIContent("Tag Field Width"));
        EditorGUILayout.PropertyField(_settingsSerializedObject.FindProperty("showGameObjectLayer"), new GUIContent("Show GameObject Layer"));
        EditorGUILayout.PropertyField(_settingsSerializedObject.FindProperty("layerFieldWidth"), new GUIContent("Layer Field Width"));

        if (EditorGUI.EndChangeCheck())
        {
            _settingsSerializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(HierarchyComponentViewerSettings.Instance);
            AssetDatabase.SaveAssets();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Reset to Defaults"))
        {
            if (EditorUtility.DisplayDialog("Reset Settings", "Are you sure you want to reset all settings to their default values?", "Yes", "No"))
            {
                // Obtenemos una nueva instancia con valores por defecto y copiamos sus valores
                HierarchyComponentViewerSettings defaults = ScriptableObject.CreateInstance<HierarchyComponentViewerSettings>();
                EditorUtility.CopySerialized(defaults, HierarchyComponentViewerSettings.Instance);
                Object.DestroyImmediate(defaults); // Limpiamos la instancia temporal

                _settingsSerializedObject.Update(); // Actualizamos el SerializedObject
                EditorUtility.SetDirty(HierarchyComponentViewerSettings.Instance);
                AssetDatabase.SaveAssets();
            }
        }
    }

    [SettingsProvider]
    public static SettingsProvider CreateHierarchyComponentViewerSettingsProvider()
    {
        if (IsSettingsAvailable())
        {
            var provider = new HierarchyComponentViewerSettingsProvider("Project/Hierarchy Component Viewer", SettingsScope.Project);
            // Cargar los settings al crear el provider para evitar que se muestren valores por defecto temporalmente
            provider.OnActivate(null, null);
            return provider;
        }
        // Si no hay settings, no mostrar la entrada en Project Settings (o manejarlo como se prefiera)
        // Opcionalmente, crear aquí el asset si no existe.
        // Forzamos la creación si no existe para que la entrada siempre esté disponible:
        HierarchyComponentViewerSettings.Instance.GetType(); // Accede a Instance para forzar la creación si es necesario
        return new HierarchyComponentViewerSettingsProvider("Project/Hierarchy Component Viewer", SettingsScope.Project);
    }

    private static bool IsSettingsAvailable()
    {
        return File.Exists(HierarchyComponentViewerSettings.SETTINGS_PATH);
    }
}