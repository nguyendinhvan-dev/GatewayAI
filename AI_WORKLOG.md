# AI Worklog

## AI Tools Used
- Gemini 3.1 Pro (via Antigravity IDE)

## How AI Helped
- Tư vấn chọn công nghệ (C# .NET 8) vì nó phù hợp tuyệt vời cho các tính năng hệ thống như Middleware, Retry (Resilience), Rate Limiting.
- Tự động sinh ra cấu trúc thư mục, code Entity Framework (Model & DB Context).
- Viết sẵn các middleware xử lý xác thực (API Key) và bắt lỗi toàn cục (Global Error Handling).
- Triển khai logic tính toán metrics (latency, token, error rate) cho API `/usage` một cách nhanh chóng.

## Incorrect AI Outputs & Improvements
- Ban đầu, tôi có ý định dùng Polly cho tính năng retry, nhưng AI đã cập nhật kiến thức về .NET 8 và gợi ý dùng gói `Microsoft.Extensions.Http.Resilience` tích hợp sẵn, giúp code gọn hơn.
- Lệnh cài đặt package bằng bash `&&` bị lỗi trên Windows PowerShell, AI đã tự nhận diện ra lỗi này và sửa lại thành cách phân tách lệnh đúng chuẩn của PowerShell.

## What I Would Improve with 7 More Days
- Tích hợp thêm **Redis Cache**: Lưu lại các câu trả lời của AI nếu người dùng hỏi trùng lặp để tiết kiệm chi phí (Cost estimation & Caching).
- **Multi-model routing & Fallback**: Cải tiến logic `LlmService` để nếu OpenAI sập, Gateway sẽ tự động định tuyến (route) câu hỏi đó sang mô hình Gemini hoặc Claude mà không ảnh hưởng tới người dùng.
- Thêm **RabbitMQ/Kafka**: Chuyển việc ghi log (AiRequests) thành cơ chế bất đồng bộ (Queue-based processing) thay vì ghi thẳng vào DB ngay lập tức làm chậm response time.
- Tạo Dashboard UI bằng React/Next.js cho trang xem Usage.
