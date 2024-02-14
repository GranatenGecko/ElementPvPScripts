using UnityEngine;

[CreateAssetMenu(fileName = "New Element", menuName = "Element")]
public class Element : ScriptableObject
{
    [Header("General Settings")]
    [SerializeField]    private string elementName;
    [SerializeField]    private int elementID;
                        public Sprite elementEmblem;

    [Header("Strength & Weakness Settings")]
    [SerializeField]    private string strength;
    [SerializeField]    private string weakness;

    [Header("Element Modifier")]
    [SerializeField]    private float healthM;
    [SerializeField]    private float strengthM;
    [SerializeField]    private float weaknessM;
    [SerializeField]    private float zoneHealthM;
    [SerializeField]    private float zoneAttackM;
}
