using UnityEngine;
using UnityEngine.SceneManagement;

public class ReloadScene : MonoBehaviour
{
    /// <summary>
    /// Reinicia la escena actual. Asigna este método al evento OnClick del botón.
    /// </summary>
    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Reinicia la escena actual por nombre.
    /// </summary>
    public void ReloadByName()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
