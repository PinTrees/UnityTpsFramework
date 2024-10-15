using System.Linq;
using UnityEngine;

public static class TransformEx 
{
    public static void SetPositionY(this Transform transform, float y)
    {
        Vector3 position = transform.position;  
        position.y = y;                         
        transform.position = position;          
    }
    public static Transform TryFindChild(this Transform transform, string name)
    {
        Transform find = transform.GetComponentsInChildren<Transform>().Where(x => x.name == name).First();
        return find;
    }

    public static void SetLayerAll(this Transform transform, int layer)
    {
        if (transform == null)
        {
            return;
        }

        transform.gameObject.layer = layer;

        foreach (Transform child in transform)
        {
            SetLayerAll(child, layer);
        }
    }

    public static void SetZeroLocalPositonAndRotation(this Transform transform)
    {
        if (transform == null) return;

        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;
    }

    public static void SetTransformForTarget(Transform parent, Transform child, Transform target)
    {
        parent.SetParent(target, true);
        parent.SetZeroLocalPositonAndRotation();

        var dir = (child.position - parent.position).normalized;
        var dit = Vector3.Distance(parent.position, child.position);

        parent.position -= dir * dit;
    }

    public static void LookCameraY(this Transform transform, float rotationSpeed)
    {
        Transform cameraTransform = Camera.main.transform;
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        Quaternion targetRotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    public static void LookCameraY(this Transform transform)
    {
        Transform cameraTransform = Camera.main.transform;
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        transform.rotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
    }

    public static void LookAt_Y(this Transform transform, Transform target, float rotatePerSec_EulerAngle=360)
    {
        if (target == null)
        {
            return;
        }

        Vector3 lookPos = target.position - transform.position;
        lookPos.y = 0; 

        Quaternion targetRotation = Quaternion.LookRotation(lookPos);
        targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0); 

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotatePerSec_EulerAngle * Time.deltaTime);
    }


    public static void LookAt_Y(this Transform transform, Vector3 targetPosition)
    {
        Vector3 lookPos = targetPosition - transform.position;
        lookPos.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(lookPos);
        targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);

        transform.rotation = targetRotation;
    }

    public static void LookAt_Y(this Transform transform, Vector3 targetPosition, float rotatePerSec_EulerAngle)
    {
        Vector3 lookPos = targetPosition - transform.position;
        lookPos.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(lookPos);
        targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotatePerSec_EulerAngle * Time.deltaTime);
    }
}
