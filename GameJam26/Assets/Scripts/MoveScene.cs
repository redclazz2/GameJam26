using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveScene : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Nombre de la escena a cargar")]
    public string sceneName;

    [Tooltip("Índice de la escena en Build Settings (alternativa al nombre)")]
    public int sceneIndex = -1;

    /// <summary>
    /// Carga la escena por nombre. Asigna este método al evento OnClick del botón.
    /// </summary>
    public void LoadSceneByName()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("ExitScript: El nombre de la escena está vacío.");
        }
    }

    /// <summary>
    /// Carga la escena por índice. Asigna este método al evento OnClick del botón.
    /// </summary>
    public void LoadSceneByIndex()
    {
        if (sceneIndex >= 0)
        {
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogWarning("ExitScript: El índice de la escena no es válido.");
        }
    }

    /// <summary>
    /// Carga la escena usando el nombre si está definido, o el índice como respaldo.
    /// </summary>
    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else if (sceneIndex >= 0)
        {
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogWarning("ExitScript: No se ha configurado ninguna escena.");
        }
    }

    /// <summary>
    /// Cierra la aplicación (útil para un botón de salir).
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
