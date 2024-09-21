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
        Cursor.lockState = CursorLockMode.Locked;  // 마우스를 중앙에 고정
        Cursor.visible = false;                    // 커서를 숨김
    }

    void UnlockMouse()
    {
        Cursor.lockState = CursorLockMode.None;    // 마우스 잠금을 해제
        Cursor.visible = true;                     // 커서를 다시 보이게 함
    }
}
