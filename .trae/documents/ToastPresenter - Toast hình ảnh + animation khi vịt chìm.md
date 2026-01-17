## Hiện Trạng
- Toast hiện chỉ hiển thị text qua `TextMeshProUGUI` và fade bằng `CanvasGroup` trong [ToastPresenter.cs](file:///c:/Users/thuon/Unity/TranChienAoLang/Assets/_Scripts/Views/ToastPresenter.cs).
- Case “vịt chìm” hiện được tạo ở [UIFeedbackRouter.cs](file:///c:/Users/thuon/Unity/TranChienAoLang/Assets/_Scripts/Managers/UIFeedbackRouter.cs) khi `ShotResult == Sunk`, nhưng chỉ render chuỗi `"… bắn Sunk"`.

## Mục Tiêu Thay Đổi (đúng 6 yêu cầu)
1) Toast “vịt chìm” dùng hình minh hoạ thay cho text.
2) Thêm animation vui nhộn (bong bóng + gợn nước).
3) Tự co giãn trong khung toast (hiện toast là 300×100) và không tràn.
4) Tối ưu hiệu suất: không Instantiate/Destroy khi chạy, dùng object pool và hạn chế layout rebuild.
5) Giữ thời gian hiển thị tương tự phiên bản cũ (shot toast 1.2s).
6) Responsive theo CanvasScaler (scene đang dùng Scale With Screen Size 1920×1080).

## Thiết Kế Kỹ Thuật
### 1) Gắn “kiểu hiển thị” vào payload để ToastPresenter nhận biết đúng case
- Mở rộng [UIFeedbackPayload.cs](file:///c:/Users/thuon/Unity/TranChienAoLang/Assets/_Scripts/Models/UIFeedbackPayload.cs) thêm trường nhẹ (enum) kiểu `ToastVisual = DefaultText | DuckSunk`.
- Cập nhật [UIFeedbackRouter.cs](file:///c:/Users/thuon/Unity/TranChienAoLang/Assets/_Scripts/Managers/UIFeedbackRouter.cs) để khi `result == ShotResult.Sunk` thì đặt `ToastVisual = DuckSunk`, đồng thời vẫn giữ `Duration = 1.2f` như cũ.

### 2) Mở rộng ToastPresenter để hỗ trợ 2 mode: text và “DuckSunk visual”
- Cập nhật [ToastPresenter.cs](file:///c:/Users/thuon/Unity/TranChienAoLang/Assets/_Scripts/Views/ToastPresenter.cs):
  - Thêm reference UI: `GameObject textRoot`, `GameObject sunkRoot`, `Image duckImage`, `RectTransform fxRoot`, `RectTransform waterMask` (RectMask2D) và 1–2 `Image` overlay (mặt nước).
  - Khi `DuckSunk`: tắt `textRoot`, bật `sunkRoot`.
  - Khi mode text: bật `textRoot`, tắt `sunkRoot`.

### 3) Animation “vịt chìm” + bong bóng + gợn nước (không cần thư viện tween)
- Vì project không có DOTween/Animator/ParticleSystem UI sẵn, dùng coroutine + `Time.unscaledDeltaTime`.
- “Vịt chìm”: dùng `RectMask2D` để tạo hiệu ứng mực nước che dần (mask tăng) + duck trôi xuống nhẹ.
- “Bong bóng”: pool sẵn N (ví dụ 6) `Image` nhỏ (sprite vòng tròn có sẵn trong Assets/Arts, ưu tiên `outline.png` nếu phù hợp), random vị trí trong khung, bay lên + fade out.
- “Gợn nước”: 1–2 `Image` vòng tròn scale up + fade out theo nhịp (pulse) trong thời gian toast hiển thị.
- Khi toast bắt đầu fade-out: dừng FX coroutine, reset trạng thái và trả object về pool.

### 4) Kích thước + responsive
- Không dùng `LayoutGroup/ContentSizeFitter` để tránh rebuild; dùng anchors + tính kích thước dựa trên `RectTransform.rect` của panel.
- Quy tắc sizing đề xuất:
  - Duck visual tối đa ~80% chiều cao toast và không vượt quá ~60–70% chiều rộng.
  - FX root full-stretch trong panel nhưng alpha thấp để không gây rối.
- Scene đã có `CanvasScaler (Scale With Screen Size)`, nên chỉ cần cấu hình anchors/pivot đúng trong hierarchy UI.

### 5) Cập nhật scene Gameplay để gắn UI mới
- Chỉnh `ToastPresenter/Panel` trong [Gameplay.unity](file:///c:/Users/thuon/Unity/TranChienAoLang/Assets/Scenes/Gameplay.unity):
  - Giữ `Toast` (TMP) làm text mode.
  - Thêm `SunkVisual` (inactive mặc định) chứa duck + mask + fx images.
  - Kéo thả reference vào `ToastPresenter` fields mới.

### 6) Kiểm thử & xác nhận hành vi
- Test nhanh trong Gameplay:
  - Bắn trúng khiến `ShotResult.Sunk` xảy ra → toast hiển thị hình + bong bóng/gợn nước.
  - Các case khác vẫn hiển thị text như cũ.
  - Thời gian hiển thị vẫn 1.2s (shot) + fade 0.15s.
- Kiểm tra ở nhiều tỉ lệ màn hình (16:9, 19.5:9, 4:3) để chắc chắn không tràn/không bị méo.

## Ghi Chú Về Asset
- Ưu tiên tái dùng sprite hiện có trong `Assets/Arts/` (ví dụ `vit2.png` làm duck, `outline.png` làm bubble/ripple). Nếu sprite không phù hợp thị giác, sẽ bổ sung 1–2 sprite mới trong `Assets/Arts/` (không đổi logic).