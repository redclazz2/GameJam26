using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Character Selection/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Character Info")]
    public string characterName;
    public Sprite characterIcon;
    public Sprite characterPortrait;
    public GameObject characterPrefab; // Puede estar null por ahora
    
    [Header("Character Stats")]
    [TextArea(3, 5)]
    public string characterDescription;
    public int health = 100;
    public int attack = 10;
    public int defense = 10;
    public int speed = 10;
    
    [Header("Visual")]
    public Color characterColor = Color.white;
    public RuntimeAnimatorController characterAnimator;
}