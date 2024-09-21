using UnityEditor;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponData weaponData;

    [Header("Transforms")]
    public Transform hitboxTransform;

    [Header("Editor Test Value")]
    public HitboxEvent hitboxEvent;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDrawGizmos()
    {
        if(hitboxTransform)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hitboxTransform.position, 0.025f);

            if (hitboxEvent)
                hitboxEvent.OnDrawGizmo(hitboxTransform);
        }
    }

#if UNITY_EDITOR
    [Button("Save RightHand")]
    public void _Editor_SaveRightHand()
    {
        if (weaponData == null)
            return;

        weaponData.Save(new EquipPositionContainer()
        {
            parentBoneType = HumanBodyBones.RightHand,
            localPosition = transform.localPosition,
            localEulerAngle = transform.localEulerAngles,   
        });

        EditorUtility.SetDirty(weaponData);
    }
    [Button("Save LeftHand")]
    public void _Editor_SaveLeftHand()
    {

    }
    [Button("Save Spine3")]
    public void _Editor_SaveLeftSpine3()
    {

    }
#endif
}
