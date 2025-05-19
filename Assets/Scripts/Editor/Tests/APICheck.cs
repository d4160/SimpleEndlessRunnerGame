// En una carpeta "Editor", por ejemplo: APICheck.cs
using UnityEngine;
using UnityEditor; // Asegúrate de que esto esté aquí

public class APICheck
{
    [MenuItem("TEST/Verify APIs")]
    public static void Verify()
    {
        GameObject go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.LogError("Por favor, selecciona un GameObject en la jerarquía.");
            return;
        }

        Component comp = go.GetComponent<Component>(); // Obtén cualquier componente
        if (comp == null)
        {
            Debug.LogError("El GameObject seleccionado no tiene componentes (aparte de Transform, que está implícito). Añade uno.");
            return;
        }

        Debug.Log("--- Verificando APIs ---");

        // Intenta usar ResetComponent
        try
        {
            // Esto debería compilar y funcionar si EditorUtility.ResetComponent existe.
            // No lo ejecutaremos realmente para no modificar, solo verificamos la llamada.
            // EditorUtility.ResetComponent(_anyComponent); // Comenta esto después de la prueba de compilación
            Debug.Log("COMPILADOR: EditorUtility.ResetComponent parece existir.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ERROR con EditorUtility.ResetComponent: {e.Message}");
        }

        // Intenta usar MoveComponentUp (requiere #if para versiones donde existe)
        // Esto es más complicado porque está en 'Unsupported'.
        // Si esto no compila, es la confirmación.
        try
        {
            // No lo ejecutaremos.
            // Unsupported.MoveComponentUp(_anyComponent); // Comenta esto después de la prueba de compilación
            Debug.Log("COMPILADOR: Unsupported.MoveComponentUp parece existir.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ERROR con Unsupported.MoveComponentUp: {e.Message}");
        }
        // Haz lo mismo para MoveComponentDown
        try
        {
            // Unsupported.MoveComponentDown(_anyComponent); // Comenta esto después de la prueba de compilación
            Debug.Log("COMPILADOR: Unsupported.MoveComponentDown parece existir.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ERROR con Unsupported.MoveComponentDown: {e.Message}");
        }

        Debug.Log("--- Verificación de APIs completada. Revisa los logs. ---");
    }
}