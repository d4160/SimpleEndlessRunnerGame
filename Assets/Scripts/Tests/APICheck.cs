// En una carpeta "Editor", por ejemplo: APICheck.cs
using UnityEngine;
using UnityEditor; // Aseg�rate de que esto est� aqu�

public class APICheck
{
    [MenuItem("TEST/Verify APIs")]
    public static void Verify()
    {
        GameObject go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.LogError("Por favor, selecciona un GameObject en la jerarqu�a.");
            return;
        }

        Component comp = go.GetComponent<Component>(); // Obt�n cualquier componente
        if (comp == null)
        {
            Debug.LogError("El GameObject seleccionado no tiene componentes (aparte de Transform, que est� impl�cito). A�ade uno.");
            return;
        }

        Debug.Log("--- Verificando APIs ---");

        // Intenta usar ResetComponent
        try
        {
            // Esto deber�a compilar y funcionar si EditorUtility.ResetComponent existe.
            // No lo ejecutaremos realmente para no modificar, solo verificamos la llamada.
            // EditorUtility.ResetComponent(_anyComponent); // Comenta esto despu�s de la prueba de compilaci�n
            Debug.Log("COMPILADOR: EditorUtility.ResetComponent parece existir.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ERROR con EditorUtility.ResetComponent: {e.Message}");
        }

        // Intenta usar MoveComponentUp (requiere #if para versiones donde existe)
        // Esto es m�s complicado porque est� en 'Unsupported'.
        // Si esto no compila, es la confirmaci�n.
        try
        {
            // No lo ejecutaremos.
            // Unsupported.MoveComponentUp(_anyComponent); // Comenta esto despu�s de la prueba de compilaci�n
            Debug.Log("COMPILADOR: Unsupported.MoveComponentUp parece existir.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ERROR con Unsupported.MoveComponentUp: {e.Message}");
        }
        // Haz lo mismo para MoveComponentDown
        try
        {
            // Unsupported.MoveComponentDown(_anyComponent); // Comenta esto despu�s de la prueba de compilaci�n
            Debug.Log("COMPILADOR: Unsupported.MoveComponentDown parece existir.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ERROR con Unsupported.MoveComponentDown: {e.Message}");
        }

        Debug.Log("--- Verificaci�n de APIs completada. Revisa los logs. ---");
    }
}