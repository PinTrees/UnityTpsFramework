## UI System 확장 가이드

#### 개요
UI System Manager는 프로젝트 전반의 UI 요소(View, Popup, Slot 등)를 일관성 있게 관리하기 위해 설계된 시스템임.
UI의 생성, 초기화, 표시, 닫기, 애니메이션, 풀링, 드래그/드롭 등 다양한 UI 동작을 통합적으로 제어할 수 있도록 구성됨.

#### 주요 구성 요소
UIViewBase
일반 화면(View) UI의 기본 클래스임. UI의 초기화, 표시, 닫기, 애니메이션 처리 등의 공통 기능을 포함함.

UIPopupBase
팝업 전용 UI 클래스임. UIViewBase를 상속하며 닫기 버튼 처리, 뒤로가기 대응, 오디오 클립 재생 등을 포함함.

UIObjectBase
슬롯, 버튼 등 단위 UI 오브젝트의 기반 클래스임. 풀링 지원, 자식 UI 관리, 포인터 이벤트 감지 기능을 포함함.

UISystemManager
모든 UI 오브젝트 및 캔버스를 관리하는 싱글톤 매니저임. 명시적 초기화가 필요하며, View 및 Object를 등록 및 생성함.

초기화
모든 UI 요소는 Init() 또는 Start() 시점에 자동으로 초기화되며, OnInit()은 UI 생애 주기 동안 단 한 번만 호출됨.

UISystemManager는 런타임에서 명시적으로 초기화되어야 하며, UI 진입 시점에 Instance 접근을 통해 자동으로 생성됨.

OnInit() 내에서 컴포넌트 캐싱, 버튼 이벤트 등록 등의 초기 설정을 수행해야 함.

```csharp
protected override void OnInit()
{
    base.OnInit();
    button.SetOnClickListener(OnClickButton);
}
```

풀링 기반 UI 사용
풀 등록
UI 오브젝트는 런타임에서 UISystemManager.Instance.CreatePool<T>()을 통해 풀에 등록됨.
중복 등록은 내부적으로 자동 방지됨.

```csharp
protected override void OnInit()
{
    base.OnInit();
    UISystemManager.Instance.CreatePool<UI_PlayFabEmail_Button>(gameObject);
}
```

해제 처리
풀링된 오브젝트는 Close() 시점에서 Release() 호출을 통해 반환되어야 함.

```csharp
public override void Close()
{
    base.Close();
    UISystemManager.Instance.Release(baseObject);
}
```

버튼 이벤트 확장
다음 확장 메서드를 통해 버튼 이벤트를 간편하게 등록할 수 있음.

SetOnClickListener : 기존 리스너 제거 후 등록
AddOnClickListener : 기존 유지, 중복 방지
SetOnPointDownListener, SetOnPointUpListener : 포인터 이벤트에 대응

```csharp
button.SetOnClickListener(() => { /* 처리 */ });
button.SetOnPointDownListener(() => { /* 처리 */ });
// 가능한 모든 연결 이벤트는 람다로 처리하지 말것, 모호성 증가
```

드래그 & 드롭 처리
UIDragBase
IBeginDragHandler, IDragHandler, IEndDragHandler를 구현함.

드래그 시작 시 UISystemManager.IsDragging 값이 true로 설정됨.

```csharp
protected override void OnDragging(PointerEventData eventData)
{
    // 드래그 중 처리
}
```

UI_DropTarget
IDropHandler, IPointerEnterHandler, IPointerExitHandler를 구현함.

드롭 대상에서 오브젝트 수신 및 콜백 처리 가능함.

```csharp
public Action<GameObject> OnDropEvent;
```

동적 UI 생성 및 해제
슬롯, 버튼 등은 런타임에서 동적으로 생성 및 해제하여 사용함.

생성
```csharp
var slot = UISystemManager.Instance.Create<UI_UnitList_UnLock_Slot>();
slot.transform.SetParentResetTransform(container);
slot.Show(unitData);
```

해제
```csharp
slot.Close(); // 내부적으로 Release() 호출됨
```

데이터 바인딩 및 자동 갱신
데이터 변경 시, 이벤트를 통해 UI 갱신을 자동으로 처리함.
```csharp
protected override void OnShow()
{
    userData.OnChange += RefreshUI;
}
protected override void OnClose()
{
    userData.OnChange -= RefreshUI;
}
```

탭 UI 처리
UITabGroup, UITabButton 구성 요소를 사용하여 탭 전환 UI를 구현할 수 있음.
탭 선택 시 콜백을 통해 View 제어가 가능함.
```csharp
tabButton.SetOnSelectListener(() => ShowView());
tabGroup.SelectTabButton(defaultTab);
```

레이아웃 갱신
동적으로 슬롯이 추가되거나 제거된 경우, 레이아웃 강제 갱신이 필요함.
RefreshLayout() 호출을 통해 LayoutGroup 요소를 즉시 갱신할 수 있음.
```csharp
base.RefreshLayout();
```

## 요약
OnInit() :	초기화 진입점. UI 생애 주기 중 1회 호출됨.
풀 등록 :	CreatePool<T>()을 통해 등록. 중복 방지 처리 포함됨.
해제 처리	: Close() 내부에서 Release() 호출 필요함.
버튼 리스너 :	확장 메서드로 등록. 리스너 중복 제거 가능함.
드래그 처리 :	UIDragBase 상속 후 전용 메서드 오버라이딩함.
드롭 처리	: UI_DropTarget을 통해 드롭 수신 처리 가능함.
탭 : UI	UITabGroup, UITabButton으로 탭 UI 구성 가능함.
레이아웃	: RefreshLayout() 호출로 강제 갱신 처리 가능함.
초기화 조건	: UISystemManager는 명시적으로 초기화되어야 함.

이 가이드는 UI 시스템 구조에 대한 전반적인 이해를 돕기 위한 문서임.
특정 View나 Slot 구조에 따라 커스터마이징이 필요할 수 있으며, 공통 동작은 UIObjectBase, UIViewBase, UIPopup, UISystemManager를 중심으로 구성됨.
