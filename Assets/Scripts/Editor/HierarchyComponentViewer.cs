// En una carpeta "Editor"
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// Estructura para almacenar datos de componentes copiados con EditorJsonUtility
public class CopiedComponentData
{
    public string JsonData;
    public System.Type ComponentType;
    public string OriginalName; // Para el tooltip o la interfaz
}

[InitializeOnLoad]
public static class HierarchyComponentViewer
{
    private static HierarchyComponentViewerSettings _settings;
    private static Dictionary<int, int> _gameObjectPageStates = new Dictionary<int, int>();
    private static Dictionary<System.Type, Texture> _iconCache = new Dictionary<System.Type, Texture>();

    // Usaremos nuestra propia estructura para el buffer de copiado
    private static CopiedComponentData _copiedComponentJsonBuffer = null;

    private const float MIN_SPACE_AFTER_NAME_DEFAULT = 20f;

    static HierarchyComponentViewer()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemGUI;
        EnsureSettingsExist();
    }

    private static void EnsureSettingsExist()
    {
        // ... (igual que antes, usa tu HierarchyComponentViewerSettings.Instance) ...
        if (_settings == null)
        {
            _settings = HierarchyComponentViewerSettings.Instance;
            if (_settings == null)
            {
                Debug.LogError("HierarchyComponentViewerSettings no se pudieron cargar.");
            }
        }
    }

    private static void OnHierarchyWindowItemGUI(int instanceID, Rect selectionRect)
    {
        GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (go == null) return;

        EnsureSettingsExist();
        if (_settings == null) return;

        Rect availableRect = selectionRect;
        availableRect.y += (selectionRect.height - EditorGUIUtility.singleLineHeight) / 2;
        availableRect.height = EditorGUIUtility.singleLineHeight;

        float currentX = selectionRect.xMax - 5f;

        if (_settings.showGameObjectLayer && _settings.layerFieldWidth > 0)
        {
            currentX -= _settings.layerFieldWidth;
            Rect layerRect = new Rect(currentX, availableRect.y, _settings.layerFieldWidth, availableRect.height);
            if (layerRect.xMin >= selectionRect.x + EditorGUIUtility.labelWidth + MIN_SPACE_AFTER_NAME_DEFAULT)
                DrawLayerField(go, layerRect);
            else {/* No hay espacio */}
            currentX -= _settings.componentIconSpacing;
        }

        if (_settings.showGameObjectTag && _settings.tagFieldWidth > 0)
        {
            currentX -= _settings.tagFieldWidth;
            Rect tagRect = new Rect(currentX, availableRect.y, _settings.tagFieldWidth, availableRect.height);
            if (tagRect.xMin >= selectionRect.x + EditorGUIUtility.labelWidth + MIN_SPACE_AFTER_NAME_DEFAULT)
                DrawTagField(go, tagRect);
            else {/* No hay espacio */}
            currentX -= _settings.componentIconSpacing;
        }

        float componentIconsRightBoundary = currentX;

        // Obtener todos los componentes EXCLUYENDO Transform y RectTransform para la visualización de iconos
        Component[] allComponents = go.GetComponents<Component>();
        List<Component> displayableComponents = allComponents
            .Where(c => c != null && !(c is Transform) && !(c is RectTransform)) // <<-- CAMBIO CLAVE AQUÍ
            .ToList();

        if (displayableComponents.Count == 0) return; // Si solo hay Transform/RectT, no mostrar iconos de componentes

        // paginatedComponents ahora es lo mismo que displayableComponents
        List<Component> paginatedComponents = displayableComponents;

        int currentPage = _gameObjectPageStates.TryGetValue(instanceID, out int page) ? page : 0;
        int maxComponentsPerPageSetting = _settings.maxComponentsPerPage;

        bool usePagination = paginatedComponents.Count > maxComponentsPerPageSetting;
        int componentsToDisplayThisPage = usePagination ? maxComponentsPerPageSetting : paginatedComponents.Count;

        int totalPages = usePagination ? Mathf.CeilToInt((float)paginatedComponents.Count / maxComponentsPerPageSetting) : 1;
        currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);
        _gameObjectPageStates[instanceID] = currentPage;

        // Ya no contamos el Transform/RectTransform para numIconsActuallyDrawn
        int numIconsActuallyDrawn = componentsToDisplayThisPage;

        float paginationButtonsWidth = (usePagination ? 2 : 0) * (_settings.componentIconWidth + _settings.componentIconSpacing);
        float componentIconsActualWidth = (numIconsActuallyDrawn > 0) ? (numIconsActuallyDrawn * (_settings.componentIconWidth + _settings.componentIconSpacing)) - _settings.componentIconSpacing : 0;

        // Si no hay iconos para dibujar (ej. solo hay Transform/RectT, o ningún componente), no continuar.
        if (numIconsActuallyDrawn == 0 && !usePagination) return;
        // Si solo hay botones de paginación pero no iconos (caso raro), también podríamos salir
        if (numIconsActuallyDrawn == 0 && paginationButtonsWidth == 0) return;


        float totalComponentAreaWidth = componentIconsActualWidth + paginationButtonsWidth;
        if (totalComponentAreaWidth <= 0) return; // No hay nada que dibujar

        float componentIconsStartX = componentIconsRightBoundary - totalComponentAreaWidth;

        float leftBoundForIcons = selectionRect.x + EditorGUIUtility.labelWidth + MIN_SPACE_AFTER_NAME_DEFAULT;
        if (componentIconsStartX < leftBoundForIcons)
        {
            float overflow = leftBoundForIcons - componentIconsStartX;
            componentIconsStartX = leftBoundForIcons;
            totalComponentAreaWidth -= overflow;
            if (totalComponentAreaWidth < _settings.componentIconWidth && !usePagination) return; // No hay espacio ni para un icono (si no hay pag)
            if (totalComponentAreaWidth < (_settings.componentIconWidth * (usePagination ? 2 : 1)) && usePagination) return; // No hay espacio ni para botones de pag

        }

        Rect componentDrawArea = new Rect(componentIconsStartX, availableRect.y, totalComponentAreaWidth, availableRect.height);
        Rect currentIconRect = new Rect(componentDrawArea.x, componentDrawArea.y, _settings.componentIconWidth, componentDrawArea.height);

        if (usePagination)
        {
            if (CanDrawAt(currentIconRect, componentIconsRightBoundary))
            {
                if (GUI.Button(currentIconRect, "<", EditorStyles.miniButtonLeft))
                {
                    _gameObjectPageStates[instanceID] = Mathf.Max(0, currentPage - 1);
                    EditorApplication.RepaintHierarchyWindow();
                }
                currentIconRect.x += _settings.componentIconWidth + _settings.componentIconSpacing;
            }
        }

        // Ya NO se dibuja Transform/RectTransform explícitamente aquí
        // if (transformComponent != null) { ... }

        int startIndex = usePagination ? (currentPage * maxComponentsPerPageSetting) : 0;
        int endIndex = usePagination ? (startIndex + maxComponentsPerPageSetting) : paginatedComponents.Count;
        endIndex = Mathf.Min(endIndex, paginatedComponents.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            Component component = paginatedComponents[i];
            // Ya está filtrado, así que no necesitamos chequear `c is Transform` aquí.
            if (component == null) continue;

            if (CanDrawAt(currentIconRect, componentIconsRightBoundary))
            {
                DrawComponentIcon(component, currentIconRect);
                currentIconRect.x += _settings.componentIconWidth + _settings.componentIconSpacing;
            }
            else break;
        }

        if (usePagination)
        {
            if (CanDrawAt(currentIconRect, componentIconsRightBoundary))
            {
                if (GUI.Button(currentIconRect, ">", EditorStyles.miniButtonRight))
                {
                    _gameObjectPageStates[instanceID] = Mathf.Min(totalPages - 1, currentPage + 1);
                    EditorApplication.RepaintHierarchyWindow();
                }
            }
        }
    }
    private static bool CanDrawAt(Rect iconRect, float rightBoundary)
    {
        // Asegura que haya espacio para dibujar el icono completo
        return iconRect.x + iconRect.width <= rightBoundary + float.Epsilon && iconRect.width > 0;
    }

    private static void DrawTagField(GameObject go, Rect fieldRect)
    {
        if (!_settings.showGameObjectTag || fieldRect.width <= 0) return;
        EditorGUI.BeginChangeCheck();
        string newTag = EditorGUI.TagField(fieldRect, GUIContent.none, go.tag);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(go, "Change GameObject Tag");
            go.tag = newTag;
            EditorUtility.SetDirty(go);
        }
    }

    private static void DrawLayerField(GameObject go, Rect fieldRect)
    {
        if (!_settings.showGameObjectLayer || fieldRect.width <= 0) return;
        EditorGUI.BeginChangeCheck();
        int newLayer = EditorGUI.LayerField(fieldRect, GUIContent.none, go.layer);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(go, "Change GameObject Layer");
            go.layer = newLayer;
            EditorUtility.SetDirty(go);
        }
    }

    private static void DrawComponentIcon(Component component, Rect iconRect)
    {
        if (iconRect.width <= 0) return;

        Texture icon = GetCachedComponentIcon(component.GetType());

        // Usar _settings.showComponentTypeName para determinar el detalle del tooltip
        string tooltip = _settings.showComponentTypeName ? component.GetType().FullName : component.GetType().Name;

        GUIContent content = new GUIContent(icon, tooltip);
        bool isEffectivelyEnabled = IsComponentEffectivelyEnabled(component);

        Color originalColor = GUI.color;
        // Usar _settings.disabledComponentAlpha. No hay booleano para activarlo/desactivarlo en tus settings,
        // así que se aplica si el componente está deshabilitado (y no es Transform).
        if (!isEffectivelyEnabled && !(component is Transform))
        {
            GUI.color = new Color(originalColor.r, originalColor.g, originalColor.b, _settings.disabledComponentAlpha);
        }

        GUI.Label(iconRect, content, GUIStyle.none);
        GUI.color = originalColor;

        Event currentEvent = Event.current;
        if (iconRect.Contains(currentEvent.mousePosition))
        {
            // Eliminada la lógica de highlightIconOnMouseOver y allowDragAndDropFromIcon
            // ya que no están en tus settings.

            if (currentEvent.type == EventType.MouseDown)
            {
                if (currentEvent.button == 0)
                {
                    EditorGUIUtility.PingObject(component.gameObject);
                    Selection.activeObject = component;
                    currentEvent.Use();
                }
                else if (currentEvent.button == 1)
                {
                    ShowContextMenu(component, isEffectivelyEnabled);
                    currentEvent.Use();
                }
            }
        }
    }

    private static bool IsComponentEffectivelyEnabled(Component component)
    {
        if (component == null) return false;
        if (component is Transform) return component.gameObject.activeSelf;

        PropertyInfo enabledProp = component.GetType().GetProperty("enabled", BindingFlags.Public | BindingFlags.Instance);
        if (enabledProp != null && enabledProp.PropertyType == typeof(bool))
        {
            // Un componente está efectivamente habilitado si su propiedad 'enabled' es true Y su GameObject está activo en la jerarquía.
            return (bool)enabledProp.GetValue(component, null) && component.gameObject.activeInHierarchy;
        }
        // Si no tiene propiedad 'enabled' (raro para Behaviours), su estado depende del GameObject.
        return component.gameObject.activeInHierarchy;
    }

    private static Texture GetCachedComponentIcon(System.Type componentType)
    {
        if (!_iconCache.TryGetValue(componentType, out Texture icon))
        {
            GUIContent content = EditorGUIUtility.ObjectContent(null, componentType);
            icon = content.image;

            // Lógica simplificada para reemplazar iconos genéricos, sin depender de settings adicionales.
            bool isGenericOrMissingIcon = icon == null ||
                                         icon.name.ToLowerInvariant().Contains("default") ||
                                         icon.name.ToLowerInvariant().Contains("scriptableobject") ||
                                         (icon.name.ToLowerInvariant().Contains("script icon") &&
                                          !typeof(MonoBehaviour).IsAssignableFrom(componentType) &&
                                          componentType != typeof(MonoBehaviour));

            if (isGenericOrMissingIcon)
            {
                if (componentType == typeof(Transform))
                    icon = EditorGUIUtility.IconContent("Transform Icon").image;
                else if (componentType == typeof(RectTransform))
                    icon = EditorGUIUtility.IconContent("RectTransform Icon").image;
                else if (typeof(MonoBehaviour).IsAssignableFrom(componentType) || componentType == typeof(MonoBehaviour))
                    icon = EditorGUIUtility.IconContent("cs Script Icon").image;
                else if (icon == null || icon.name.ToLowerInvariant().Contains("default")) // Fallback final
                    icon = EditorGUIUtility.IconContent("console.infoicon.sml").image;
            }
            _iconCache[componentType] = icon;
        }
        return icon;
    }

    private static void ShowContextMenu(Component component, bool isEffectivelyEnabled)
    {
        GenericMenu menu = new GenericMenu();
        GameObject go = component.gameObject;

        // --- Reset ---
        // No hay API pública directa ni ExecuteMenuItem funcional.
        // Podrías ofrecerlo solo si el componente tiene una interfaz IResettable.
        if (component is IResettable resettable) // Necesitarías definir esta interfaz
        {
            menu.AddItem(new GUIContent("Reset (Custom)"), false, () =>
            {
                Undo.RecordObject(component, "Reset Component (Custom)");
                resettable.CustomReset();
                EditorUtility.SetDirty(component);
            });
        }
        else
        {
            menu.AddDisabledItem(new GUIContent("Reset (N\\A - No API or IResettable)"));
        }
        menu.AddSeparator("");

        // --- Activar/Desactivar Componente --- (Esto debería funcionar)
        if (!(component is Transform))
        {
            PropertyInfo enabledProp = component.GetType().GetProperty("enabled", BindingFlags.Public | BindingFlags.Instance);
            if (enabledProp != null && enabledProp.PropertyType == typeof(bool))
            {
                menu.AddItem(new GUIContent(isEffectivelyEnabled ? "Disable Component" : "Enable Component"), false, () =>
                {
                    Undo.RecordObject(component, (isEffectivelyEnabled ? "Disable" : "Enable") + " Component");
                    enabledProp.SetValue(component, !isEffectivelyEnabled, null);
                    EditorUtility.SetDirty(component);
                    EditorApplication.RepaintHierarchyWindow();
                });
            }
            else menu.AddDisabledItem(new GUIContent("Enable/Disable Component (Property N\\A)"));
        }

        // --- Mover Componente ---
        // No hay forma segura de hacer esto sin APIs.
        menu.AddDisabledItem(new GUIContent("Move Up (N\\A - No API)"));
        menu.AddDisabledItem(new GUIContent("Move Down (N\\A - No API)"));
        menu.AddSeparator("");

        // --- Copy/Paste usando EditorJsonUtility (limitado) ---
        bool canBeJsonCopied = component is MonoBehaviour || component.GetType().IsSubclassOf(typeof(ScriptableObject));

        if (canBeJsonCopied)
        {
            menu.AddItem(new GUIContent("Copy Component (JSON)"), false, () =>
            {
                string json = EditorJsonUtility.ToJson(component, true);
                _copiedComponentJsonBuffer = new CopiedComponentData
                {
                    JsonData = json,
                    ComponentType = component.GetType(),
                    OriginalName = component.GetType().Name
                };
                Debug.Log($"Component {component.GetType().Name} copied as JSON.");
            });
        }
        else
        {
            menu.AddDisabledItem(new GUIContent($"Copy Component (JSON - Only for MonoBehaviours/ScriptableObjects)"));
        }

        // Paste Component Values (JSON)
        bool canPasteJsonValues = _copiedComponentJsonBuffer != null &&
                                  _copiedComponentJsonBuffer.ComponentType == component.GetType() &&
                                  canBeJsonCopied; // El destino también debe ser compatible
        if (canPasteJsonValues)
        {
            menu.AddItem(new GUIContent($"Paste Values from '{_copiedComponentJsonBuffer.OriginalName}' (JSON)"), false, () =>
            {
                Undo.RecordObject(component, "Paste Component Values (JSON)");
                EditorJsonUtility.FromJsonOverwrite(_copiedComponentJsonBuffer.JsonData, component);
                EditorUtility.SetDirty(component);
                EditorApplication.RepaintHierarchyWindow();
                Debug.Log($"Values pasted to {component.GetType().Name} from JSON.");
            });
        }
        else
        {
            string reason = "";
            if (_copiedComponentJsonBuffer == null) reason = "(No JSON component copied)";
            else if (!canBeJsonCopied) reason = "(Target not MB/SO)";
            else if (_copiedComponentJsonBuffer.ComponentType != component.GetType()) reason = $"(Type mismatch: copied {_copiedComponentJsonBuffer.ComponentType.Name})";
            menu.AddDisabledItem(new GUIContent($"Paste Component Values (JSON) {reason}"));
        }

        // Paste Component As New (JSON)
        bool canPasteJsonAsNew = _copiedComponentJsonBuffer != null &&
                                 (typeof(MonoBehaviour).IsAssignableFrom(_copiedComponentJsonBuffer.ComponentType) ||
                                  typeof(ScriptableObject).IsAssignableFrom(_copiedComponentJsonBuffer.ComponentType)); // Solo se pueden añadir estos tipos como nuevos generalmente
        if (canPasteJsonAsNew)
        {
            menu.AddItem(new GUIContent($"Paste '{_copiedComponentJsonBuffer.OriginalName}' As New (JSON)"), false, () =>
            {
                Undo.SetCurrentGroupName("Paste Component As New (JSON)");
                int group = Undo.GetCurrentGroup();

                Component newComp = Undo.AddComponent(go, _copiedComponentJsonBuffer.ComponentType);
                if (newComp != null)
                {
                    EditorJsonUtility.FromJsonOverwrite(_copiedComponentJsonBuffer.JsonData, newComp);
                    EditorUtility.SetDirty(newComp); // Marcar el nuevo componente
                    EditorUtility.SetDirty(go);      // Marcar el GameObject
                    Debug.Log($"Component {_copiedComponentJsonBuffer.ComponentType.Name} pasted as new (JSON) to {go.name}.");
                }
                else
                {
                    Debug.LogError($"Failed to add component of type {_copiedComponentJsonBuffer.ComponentType.Name} to {go.name}.");
                }
                Undo.CollapseUndoOperations(group);
                EditorApplication.RepaintHierarchyWindow();
            });
        }
        else
        {
            string reason = _copiedComponentJsonBuffer == null ? "(No JSON component copied)" : "(Copied type not MB/SO)";
            menu.AddDisabledItem(new GUIContent($"Paste Component As New (JSON) {reason}"));
        }

        // --- Remover Componente --- (Esto debería funcionar)
        if (!(component is Transform))
        {
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Remove Component"), false, () =>
            {
                Undo.DestroyObjectImmediate(component);
                EditorApplication.RepaintHierarchyWindow(); // Repintar para que desaparezca
            });
        }
        else
        {
            menu.AddSeparator("");
            menu.AddDisabledItem(new GUIContent("Remove Component (Transform)"));
        }

        menu.ShowAsContext();
    }
}

// Interfaz opcional para que los componentes implementen su propia lógica de Reset
public interface IResettable
{
    void CustomReset();
}