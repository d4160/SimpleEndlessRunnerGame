// En una carpeta "Editor"
using UnityEngine;

// Le damos un nombre de menú para crearlo fácilmente si no existe
[CreateAssetMenu(fileName = "HierarchyComponentViewerSettings", menuName = "Editor/Hierarchy Component Viewer Settings")]
public class HierarchyComponentViewerSettings : ScriptableObject
{
    [Tooltip("Número máximo de componentes a mostrar por página (excluyendo Transform/RectTransform).")]
    [Range(1, 20)]
    public int maxComponentsPerPage = 5;

    [Tooltip("Mostrar el nombre del tipo de componente junto al icono.")]
    public bool showComponentTypeName = true;

    [Tooltip("Transparencia para los componentes desactivados (0 = invisible, 1 = opaco).")]
    [Range(0f, 1f)]
    public float disabledComponentAlpha = 0.5f;

    [Tooltip("Ancho de cada icono de componente en la jerarquía.")]
    public float componentIconWidth = 20f;

    [Tooltip("Espacio entre iconos de componente.")]
    public float componentIconSpacing = 2f;

    [Header("Tag and Layer Display")]
    [Tooltip("Mostrar el Tag del GameObject en la jerarquía.")]
    public bool showGameObjectTag = true;
    [Tooltip("Ancho del campo Tag en la jerarquía.")]
    public float tagFieldWidth = 70f;

    [Tooltip("Mostrar la Layer del GameObject en la jerarquía.")]
    public bool showGameObjectLayer = true;
    [Tooltip("Ancho del campo Layer en la jerarquía.")]
    public float layerFieldWidth = 100f;

    // --- Constantes para facilitar el acceso ---
    public const string SETTINGS_PATH = "Assets/Editor/HierarchyComponentViewerSettings.asset";

    private static HierarchyComponentViewerSettings _instance;
    public static HierarchyComponentViewerSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = UnityEditor.AssetDatabase.LoadAssetAtPath<HierarchyComponentViewerSettings>(SETTINGS_PATH);
                if (_instance == null)
                {
                    _instance = ScriptableObject.CreateInstance<HierarchyComponentViewerSettings>();
                    // Asegurarse de que la carpeta Editor exista
                    if (!UnityEditor.AssetDatabase.IsValidFolder("Assets/Editor"))
                    {
                        UnityEditor.AssetDatabase.CreateFolder("Assets", "Editor");
                    }
                    UnityEditor.AssetDatabase.CreateAsset(_instance, SETTINGS_PATH);
                    UnityEditor.AssetDatabase.SaveAssets();
                    UnityEditor.AssetDatabase.Refresh();
                    Debug.Log("HierarchyComponentViewerSettings.asset creado en Assets/Editor/");
                }
            }
            return _instance;
        }
    }
}