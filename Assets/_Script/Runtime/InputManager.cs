using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    public override void Init()
    {
        base.Init();
        LockMouse();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            if (Cursor.visible)
                LockMouse();
            else
                UnlockMouse();
        }
    }

    void LockMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;  // ���콺�� �߾ӿ� ����
        Cursor.visible = false;                    // Ŀ���� ����
    }

    void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;    // ���콺 ����� ����
        Cursor.visible = true;                     // Ŀ���� �ٽ� ���̰� ��
    }
}
