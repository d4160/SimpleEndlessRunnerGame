// En una carpeta "Editor"
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

[InitializeOnLoad]
public static class HierarchyComponentViewer
{
    private static HierarchyComponentViewerSettings _settings;
    private static Dictionary<int, int> _gameObjectPageStates = new Dictionary<int, int>();
    private static Dictionary<System.Type, Texture> _iconCache = new Dictionary<System.Type, Texture>();
    private static Component _copiedComponentBuffer = null;

    // Constante interna para el espacio mínimo después del nombre del GO
    private const float MIN_SPACE_AFTER_NAME_DEFAULT = 20f;

    static HierarchyComponentViewer()
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemGUI;
        EnsureSettingsExist();
    }

    private static void EnsureSettingsExist()
    {
        if (_settings == null)
        {
            _settings = HierarchyComponentViewerSettings.Instance; // Usa tu property Instance
            if (_settings == null)
            {
                Debug.LogError("HierarchyComponentViewerSettings no se pudieron cargar. Por favor, asegúrese de que el asset exista o permita que se cree automáticamente según su implementación de 'Instance'.");
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

        float currentX = selectionRect.xMax - 5f; // Margen derecho inicial

        if (_settings.showGameObjectLayer && _settings.layerFieldWidth > 0)
        {
            currentX -= _settings.layerFieldWidth;
            Rect layerRect = new Rect(currentX, availableRect.y, _settings.layerFieldWidth, availableRect.height);
            if (layerRect.xMin < selectionRect.x + EditorGUIUtility.labelWidth + MIN_SPACE_AFTER_NAME_DEFAULT) { /* No dibujar si invade nombre */ }
            else DrawLayerField(go, layerRect);
            currentX -= _settings.componentIconSpacing;
        }

        if (_settings.showGameObjectTag && _settings.tagFieldWidth > 0)
        {
            currentX -= _settings.tagFieldWidth;
            Rect tagRect = new Rect(currentX, availableRect.y, _settings.tagFieldWidth, availableRect.height);
            if (tagRect.xMin < selectionRect.x + EditorGUIUtility.labelWidth + MIN_SPACE_AFTER_NAME_DEFAULT) { /* No dibujar si invade nombre */ }
            else DrawTagField(go, tagRect);
            currentX -= _settings.componentIconSpacing;
        }

        float componentIconsRightBoundary = currentX;

        Component[] allComponents = go.GetComponents<Component>();
        List<Component> displayableComponents = allComponents.Where(c => c != null).ToList();
        if (displayableComponents.Count == 0) return;

        List<Component> paginatedComponents = displayableComponents
            .Where(c => !(c is Transform) && !(c is RectTransform))
            .ToList();

        int currentPage = _gameObjectPageStates.TryGetValue(instanceID, out int page) ? page : 0;
        // Usar _settings.maxComponentsPerPage. El Range(1,20) es para el inspector.
        int maxComponentsPerPageSetting = _settings.maxComponentsPerPage;

        // Si maxComponentsPerPageSetting es 0 o menos (aunque el Range lo limita a >=1), lo tratamos como "muchos" para no paginar esos.
        // Pero el Range(1,20) en tus settings implica que siempre será >= 1.
        bool usePagination = paginatedComponents.Count > maxComponentsPerPageSetting;

        int componentsToDisplayThisPage = usePagination ? maxComponentsPerPageSetting : paginatedComponents.Count;

        int totalPages = usePagination ? Mathf.CeilToInt((float)paginatedComponents.Count / maxComponentsPerPageSetting) : 1;
        currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);
        _gameObjectPageStates[instanceID] = currentPage;

        int numIconsActuallyDrawn = 0;
        Component transformComponent = displayableComponents.FirstOrDefault(c => c is Transform || c is RectTransform);
        if (transformComponent != null) numIconsActuallyDrawn++; // Transform/RectTransform siempre se muestra si existe

        numIconsActuallyDrawn += componentsToDisplayThisPage;

        float paginationButtonsWidth = (usePagination ? 2 : 0) * (_settings.componentIconWidth + _settings.componentIconSpacing);
        float componentIconsActualWidth = (numIconsActuallyDrawn > 0) ? (numIconsActuallyDrawn * (_settings.componentIconWidth + _settings.componentIconSpacing)) - _settings.componentIconSpacing : 0;
        float totalComponentAreaWidth = componentIconsActualWidth + paginationButtonsWidth;

        float componentIconsStartX = componentIconsRightBoundary - totalComponentAreaWidth;

        float leftBoundForIcons = selectionRect.x + EditorGUIUtility.labelWidth + MIN_SPACE_AFTER_NAME_DEFAULT;
        if (componentIconsStartX < leftBoundForIcons)
        {
            float overflow = leftBoundForIcons - componentIconsStartX;
            componentIconsStartX = leftBoundForIcons;
            totalComponentAreaWidth -= overflow;
            if (totalComponentAreaWidth < _settings.componentIconWidth) return;
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

        if (transformComponent != null) // Siempre se intenta dibujar si existe
        {
            if (CanDrawAt(currentIconRect, componentIconsRightBoundary))
            {
                DrawComponentIcon(transformComponent, currentIconRect);
                currentIconRect.x += _settings.componentIconWidth + _settings.componentIconSpacing;
            }
        }

        int startIndex = usePagination ? (currentPage * maxComponentsPerPageSetting) : 0;
        int endIndex = usePagination ? (startIndex + maxComponentsPerPageSetting) : paginatedComponents.Count;
        endIndex = Mathf.Min(endIndex, paginatedComponents.Count);

        for (int i = startIndex; i < endIndex; i++)
        {
            Component component = paginatedComponents[i];
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
        // Esta función ya era bastante independiente de los settings visuales,
        // por lo que no requiere muchos cambios aquí.
        // (El código de ShowContextMenu es el mismo que en la respuesta anterior, que se basa en ExecuteMenuItem)
        GenericMenu menu = new GenericMenu();
        GameObject go = component.gameObject;

        Object currentActiveObject = Selection.activeObject;
        GameObject currentActiveGo = Selection.activeGameObject;

        System.Action<string, string> AddMenuItemViaExecute = (menuPath, undoMessage) =>
        {
            // Extraer el nombre visible del path
            string visibleMenuName = menuPath;
            int lastSlash = menuPath.LastIndexOf('/');
            if (lastSlash != -1) visibleMenuName = menuPath.Substring(lastSlash + 1);

            menu.AddItem(new GUIContent(visibleMenuName), false, () =>
            {
                bool selectionWasChangedByUs = false;
                Object originalSelection = Selection.activeObject;
                GameObject originalGOSelection = Selection.activeGameObject;

                // Para la mayoría de los comandos CONTEXT/Component/
                if (Selection.activeObject != component)
                {
                    Selection.activeObject = component;
                    selectionWasChangedByUs = true;
                }
                // Paste As New suele operar sobre el GameObject
                if (menuPath.EndsWith("Paste Component As New") && Selection.activeGameObject != go)
                {
                    Selection.activeGameObject = go;
                    selectionWasChangedByUs = true;
                }

                if (!string.IsNullOrEmpty(undoMessage))
                {
                    if (menuPath.EndsWith("Reset")) Undo.RecordObject(component, undoMessage);
                    else Undo.RecordObject(go, undoMessage); // La mayoría de las operaciones afectan al GO o están cubiertas por Unity
                }

                EditorApplication.ExecuteMenuItem(menuPath);

                // Es crucial marcar sucio para que los cambios se guarden y se repinte la UI si es necesario
                if (component != null) EditorUtility.SetDirty(component); // Puede haber sido destruido (Remove)
                if (go != null) EditorUtility.SetDirty(go);


                // Restaurar selección solo si la cambiamos nosotros y la acción del menú no la cambió a algo diferente
                if (selectionWasChangedByUs)
                {
                    if (Selection.activeObject == component || (menuPath.EndsWith("Paste Component As New") && Selection.activeGameObject == go))
                    {
                        // La acción del menú no cambió la selección que establecimos, restaurar la original
                        Selection.activeObject = originalSelection;
                        Selection.activeGameObject = originalGOSelection;
                    }
                    // Si la acción del menú SÍ cambió la selección a algo nuevo, la dejamos así.
                }

                EditorApplication.RepaintHierarchyWindow();
                // A veces se necesita un repaint adicional del inspector si los valores cambiaron
                EditorApplication.RepaintProjectWindow(); // Menos probable, pero por si acaso
            });
        };

        // --- Reset ---
        AddMenuItemViaExecute("CONTEXT/Component/Reset", "Reset Component " + component.GetType().Name);
        menu.AddSeparator("");

        // --- Activar/Desactivar Componente ---
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
            else menu.AddDisabledItem(new GUIContent("Enable/Disable Component (N/A)"));
        }

        // --- Mover Componente ---
        if (!(component is Transform))
        {
            AddMenuItemViaExecute("CONTEXT/Component/Move Up", "Move Component Up");
            AddMenuItemViaExecute("CONTEXT/Component/Move Down", "Move Component Down");
        }
        else
        {
            menu.AddDisabledItem(new GUIContent("Move Up"));
            menu.AddDisabledItem(new GUIContent("Move Down"));
        }
        menu.AddSeparator("");

        // --- Copy/Paste ---
        menu.AddItem(new GUIContent("Copy Component"), false, () =>
        {
            Object prevSelection = Selection.activeObject;
            Selection.activeObject = component;
            EditorApplication.ExecuteMenuItem("CONTEXT/Component/Copy Component");
            _copiedComponentBuffer = component;
            Selection.activeObject = prevSelection;
        });

        bool canPasteValues = _copiedComponentBuffer != null && _copiedComponentBuffer.GetType() == component.GetType();
        if (canPasteValues) AddMenuItemViaExecute("CONTEXT/Component/Paste Component Values", "Paste Component Values");
        else
        {
            string reason = _copiedComponentBuffer == null ? "(No component copied)" : $"(Type mismatch: copied {_copiedComponentBuffer.GetType().Name}, target {component.GetType().Name})";
            menu.AddDisabledItem(new GUIContent($"Paste Component Values {reason}"));
        }

        bool canPasteAsNew = _copiedComponentBuffer != null;
        if (canPasteAsNew)
        {
            menu.AddItem(new GUIContent("Paste Component As New"), false, () =>
            {
                GameObject prevSelectedGo = Selection.activeGameObject;
                Object prevSelectedObj = Selection.activeObject;

                Selection.activeGameObject = go;
                Undo.RecordObject(go, "Paste Component As New");
                EditorApplication.ExecuteMenuItem("CONTEXT/Component/Paste Component As New");
                EditorUtility.SetDirty(go);

                // No restaurar selección aquí, ya que "Paste as New" a menudo selecciona el nuevo componente.
                // Dejar que Unity maneje la selección post-operación.
                EditorApplication.RepaintHierarchyWindow();
            });
        }
        else menu.AddDisabledItem(new GUIContent("Paste Component As New (No component copied)"));

        // --- Remover Componente ---
        if (!(component is Transform))
        {
            menu.AddSeparator("");
            AddMenuItemViaExecute("CONTEXT/Component/Remove Component", "Remove Component");
        }
        else
        {
            menu.AddSeparator("");
            menu.AddDisabledItem(new GUIContent("Remove Component (Transform)"));
        }
        menu.ShowAsContext();
    }
}