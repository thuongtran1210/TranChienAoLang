## Bối Cảnh Hiện Tại (đã có gì)

* Battle HUD đã có energy + danh sách skill: [BattleUIManager.cs](file:///c:/Users/thuon/Unity/TranChienAoLang/Assets/_Scripts/Managers/BattleUIManager.cs), [SkillButtonView.cs](file:///c:/Users/thuon/Unity/TranChienAoLang/Assets/_Scripts/Views/SkillButtonView.cs).

* Event bus battle kiểu ScriptableObject đã có: [BattleEventChannelSO.cs](file:///c:/Users/thuon/Unity/TranChienAoLang/Assets/_Scripts/Models/ScriptsSO/Channel/BattleEventChannelSO.cs) (shot, energy, skill feedback, highlight, ghost, impact).

* Feedback “màu/highlight/ghost” cho skill có nền tảng nhưng hiện có điểm bất nhất: [SkillInteractionController.cs](file:///c:/Users/thuon/Unity/TranChienAoLang/Assets/_Scripts/Controllers/SkillInteractionController.cs) đang cast `gridLogic is IGridSystem` (khả năng luôn fail vì grid là `GridController`), nên nên chuyển sang dùng `gridLogic.GridSystem`.

* Setup phase đặt vịt có ghost + màu valid/invalid nhưng lý do thất bại chỉ log: [SetupState.cs](file:///c:/Users/thuon/Unity/TranChienAoLang/Assets/_Scripts/Models/GameState/SetupState.cs), [GridSystem.cs](file:///c:/Users/thuon/Unity/TranChienAoLang/Assets/_Scripts/Models/GridSystem.cs).

* Thiếu UI “phase/turn indicator”, thiếu toast/log in-game; `OnSkillFeedback` có phát nhưng chưa có UI subscribe.

## Mục Tiêu UX (người chơi luôn biết)

1. Trạng thái game: đang Setup hay Battle, và Battle thì lượt ai.
2. Hành động thành công/thất bại: đặt vịt, bắn (hit/miss/sunk), dùng skill.
3. Lý do không làm được: không đủ energy, target sai grid, ô đã bắn rồi, đặt chồng/out-of-bounds…
4. Feedback nhiều lớp đồng thời:

* Text: toast nhanh + combat log.

* Màu/highlight grid.

* Icon/animation: skill selected, skill cooldown/disable, tile indicator.

## Thiết Kế Kiến Trúc (event-driven, ít phụ thuộc)

### 1) Tách 2 loại sự kiện

* **Game Flow Events** (phase/turn): tạo `GameFlowEventChannelSO` mới.

  * `OnPhaseChanged(GamePhase phase)`

  * `OnTurnChanged(Owner turnOwner)`

* **UI Feedback Requests** (toast/log/popup): tạo `UIFeedbackChannelSO` mới (hoặc 1 Mono “router” phát thẳng vào UI).

  * `OnToastRequested(UIFeedbackPayload payload)`

  * `OnLogRequested(UIFeedbackPayload payload)`

  * (tuỳ chọn) `OnWorldPopupRequested(UIWorldPopupPayload payload)`

### 2) Router/Presenter rõ trách nhiệm

* `UIFeedbackRouter` (MonoBehaviour): lắng nghe

  * `BattleEventChannelSO` (OnShotFired, OnSkillFeedback, OnSkillSelected/Deselected, OnEnergyChanged)

  * `GameFlowEventChannelSO` (phase/turn)
    rồi *chuyển* thành toast/log/popup + gọi thêm highlight/indicator nếu cần.

* `ToastPresenter` (UI): hiển thị 1 toast có queue + fade.

* `BattleLogPresenter` (UI): append log (giữ N dòng) + optional ScrollRect.

* `PhaseTurnHUD` (UI): hiển thị Phase + Turn (text + màu/icon).

## Contract Thông Điệp (để feedback nhất quán)

* `UIFeedbackType`: Info / Success / Warning / Error.

* `UIFeedbackPayload`:

  * `string message`

  * `UIFeedbackType type`

  * `Owner? owner` (player/enemy nếu liên quan)

  * `Vector2Int? gridPos` (nếu gắn với ô)

  * `float duration`

  * `UIFeedbackSource` (Setup/Shot/Skill/System) để lọc log.

## Kế Hoạch Triển Khai (theo lớp feedback)

### A) Phase / Turn Indicator

1. Tạo `GamePhase` enum: MainMenu / Setup / Battle / GameOver.
2. Trong [GameManager.ChangeState](file:///c:/Users/thuon/Unity/TranChienAoLang/Assets/_Scripts/Managers/GameManager.cs): khi chuyển state -> raise phase.
3. Trong [BattleState.EnterState](file:///c:/Users/thuon/Unity/TranChienAoLang/Assets/_Scripts/Models/GameState/BattleState.cs): raise `TurnChanged(Player)`.
4. Trong `BattleState.SwitchTurn()`: raise `TurnChanged(Player/Enemy)`.
5. UI `PhaseTurnHUD` subscribe và cập nhật (ví dụ: Setup màu xanh, Battle màu đỏ; Turn player highlight).

### B) Text Feedback: Toast + Combat Log

1. Thêm `ToastPresenter` + `BattleLogPresenter` vào một Canvas UI root (có thể nằm trong Battle UI root và Setup UI root, hoặc một UI root dùng chung).
2. `UIFeedbackRouter` map sự kiện:

   * Shot: `OnShotFired` -> log “Player bắn (x,y): Hit/Miss/Sunk”; toast ngắn (“Hit!”/“Miss!”).

   * Skill: `OnSkillFeedback` -> log + toast (ví dụ “Not enough energy!”, “Invalid Target!”).

   * Setup placement: khi đặt thành công/thất bại -> log/toast.
3. (Tuỳ chọn) World popup (floating text trên ô) cho hit/miss/skill result.

### C) Màu/Highlight Grid

1. Sửa `SkillInteractionController` để dùng `gridLogic.GridSystem` thay vì cast `IGridSystem`.
2. Khi action fail có `gridPos`: gọi `RaiseSkillImpactVisual` (màu đỏ ngắn) hoặc `RaiseTileIndicator` để đánh dấu ô lỗi.
3. Với shot result: tận dụng TilemapGridView/OnShotFired hiện có; bổ sung “flash” nhẹ theo result nếu cần.

### D) Icon / Animation

1. Skill button:

   * Giữ selected frame (đã có).

   * Overlay hiện tại đang dùng cho “không đủ energy”; chuẩn hoá tên/ý nghĩa.
2. (Tuỳ chọn mở rộng) Cooldown thật:

   * Thêm `cooldownTurns` vào [DuckSkillSO.cs](file:///c:/Users/thuon/Unity/TranChienAoLang/Assets/_Scripts/Models/ScriptsSO/DuckSkillSO.cs).

   * BattleState quản lý cooldown theo skill, giảm mỗi lần đổi lượt.

   * SkillButtonView hiển thị số turn còn lại + disable.

## Bổ Sung “Tại Sao Không Làm Được” (reason codes)

### Setup placement

* Mở rộng logic trong [GridSystem.cs](file:///c:/Users/thuon/Unity/TranChienAoLang/Assets/_Scripts/Models/GridSystem.cs) để trả về lý do:

  * `PlacementFailReason`: OutOfBounds / Occupied / NullData / Unknown.

  * Cung cấp hàm `CheckPlacement(...) -> (bool ok, PlacementFailReason reason, Vector2Int failedCell)`.

* SetupState dùng reason để toast/log: “Không thể đặt: trùng vịt” / “Ngoài biên” / “Chỉ đặt trên bảng Player”.

### Battle / Skill

* Khi không đủ energy: toast/log (đã raise).

* Khi target sai grid theo targetType: trong hover/preview có thể hiển thị hint nhỏ (toast throttle) + highlight đỏ.

* Khi bắn vào ô đã bắn: thay vì im lặng, toast “Ô này đã bắn rồi”.

## Kiểm Thử / Xác Minh

* Setup:

  * đặt đúng -> toast “Đặt thành công …” + log.

  * đặt sai (chồng/out-of-bounds) -> toast có lý do.

* Battle:

  * kiểm tra Phase/Turn đổi đúng; AI turn hiển thị.

  * bắn hit/miss/sunk -> toast + log + popup (nếu bật).

  * chọn skill đủ energy -> preview highlight/ghost hoạt động.

  * chọn skill thiếu energy / target invalid -> toast có lý do.

## Ghi Chú Về Skill Tool

* Bộ công cụ “skill-creator” dùng để tạo *plugin skill* mới cho trợ lý, không liên quan trực tiếp đến việc thiết kế hệ thống UI feedback trong dự án Unity này, nên mình không dùng nó ở bước lập kế hoạch.

