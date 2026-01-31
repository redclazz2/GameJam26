using UnityEngine;

/// <summary>
/// Script de prueba para crear personajes de ejemplo sin necesidad de prefabs
/// Úsalo mientras desarrollas y no tienes los modelos 3D listos
/// </summary>
public class CharacterSelectionDemo : MonoBehaviour
{
    [Header("Auto-Create Demo Characters")]
    [SerializeField] private bool createDemoCharacters = true;
    [SerializeField] private CharacterSelectionManager selectionManager;
    
    private void Awake()
    {
        if (createDemoCharacters && selectionManager != null)
        {
            CreateDemoCharacterData();
        }
    }
    
    private void CreateDemoCharacterData()
    {
        // Nota: En producción, crearías estos como ScriptableObjects desde el menú
        // Esto es solo para demostración rápida
        
        Debug.Log("Demo: Creando personajes de ejemplo...");
        Debug.Log("Para producción, crea CharacterData assets desde: Right Click > Create > Character Selection > Character Data");
    }
    
    // Método helper para crear sprites de prueba dinámicamente
    public static Sprite CreateColoredSprite(Color color, int width = 128, int height = 128)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }
    
    // Método para crear un placeholder 3D simple
    public static GameObject CreatePlaceholder3DCharacter(Color color, string characterName)
    {
        GameObject character = new GameObject(characterName + "_Preview");
        
        // Crear cuerpo (cápsula)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.transform.SetParent(character.transform);
        body.transform.localPosition = new Vector3(0, 1, 0);
        body.GetComponent<Renderer>().material.color = color;
        
        // Crear cabeza (esfera)
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.SetParent(character.transform);
        head.transform.localPosition = new Vector3(0, 2, 0);
        head.transform.localScale = Vector3.one * 0.5f;
        head.GetComponent<Renderer>().material.color = color * 1.2f;
        
        // Remover colliders (no necesarios para preview)
        Destroy(body.GetComponent<Collider>());
        Destroy(head.GetComponent<Collider>());
        
        return character;
    }
}