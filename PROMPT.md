# PROMPT ĐỂ XÂY DỰNG DỰ ÁN AI GATEWAY TỪ ĐẦU

Đây là Prompt bạn sẽ dùng để nộp bài (để giám khảo thấy bạn đã thiết kế một prompt cực kỳ chất lượng với đầy đủ Bối cảnh, Vai trò và Yêu cầu đầu ra).

---

**Nội dung Prompt nộp bài:**

```markdown
# Role (Vai trò)
Bạn là một Senior Backend Developer và System Architect chuyên nghiệp, có nhiều kinh nghiệm xây dựng các hệ thống API Gateway bằng ngôn ngữ C# (.NET 8). Bạn rất giỏi trong việc tối ưu hóa hiệu suất, thiết kế kiến trúc và xử lý lỗi hệ thống.

# Context (Bối cảnh)
Tôi đang tham gia một Hackathon (7-Day AI Builder Challenge). Bài toán là xây dựng một "AI Gateway API" - một dịch vụ trung gian (central layer) nằm giữa các ứng dụng nội bộ của công ty và các nhà cung cấp AI bên ngoài (OpenAI, Gemini, Claude...). 
Mục tiêu là các ứng dụng nội bộ không gọi trực tiếp cho OpenAI nữa, mà sẽ gọi qua Gateway này để Gateway lo phần xác thực, định tuyến (routing), tính toán chi phí (usage) và dự phòng lỗi (fallback).

# Objective (Mục tiêu)
Phát triển một bản prototype hoàn chỉnh, hoạt động được ngay (Enterprise-ready) đáp ứng đủ các tính năng tối thiểu và ăn được nhiều điểm bonus nhất có thể theo yêu cầu của đề bài.

# Requirements (Yêu cầu đầu ra)
1. **Công nghệ:** Sử dụng C# ASP.NET Core (.NET 8). Database dùng SQLite + Entity Framework Core cho dễ chạy demo.
2. **Tính năng bắt buộc:**
   - **Authentication:** Tạo API Key cho mỗi app và dùng custom middleware để xác thực qua header `X-Api-Key`.
   - **Database & Storage:** Lưu thông tin API keys, lưu lịch sử mỗi lượt request (latency, tokens, status, model).
   - **LLM Integration:** Kết nối thật với API của OpenAI và Anthropic.
   - **Error & Resilience:** Bắt lỗi global. Cấu hình tự động Retry và Timeout bằng `Microsoft.Extensions.Http.Resilience`.
   - **Rate Limiting:** Giới hạn 100 requests / 1 phút / 1 API Key.
   - **Usage API:** Trả về tổng request, tổng token, độ trễ trung bình, tỷ lệ lỗi.
3. **Tính năng Bonus (Bắt buộc phải có để ăn điểm tối đa):**
   - **Multi-model routing & Fallback:** Nếu gọi OpenAI lỗi (VD hết tiền, server sập), code phải tự động bắt lỗi và nhảy sang gọi Claude (Anthropic).
   - **Queue-based processing:** Không ghi log thẳng vào database trong lúc call API (làm chậm tốc độ trả về). Phải đưa log vào một hàng đợi (Queue) trên RAM và dùng một `BackgroundService` để từ từ bốc ra lưu vào DB.
   - **Caching:** Nếu có 2 user hỏi trùng một câu hỏi, trả về ngay từ MemoryCache trong vòng 5 phút để tiết kiệm tiền.
4. **Deliverables:**
   - Viết toàn bộ code hoàn chỉnh, có thể build và run.
   - Viết mã Mermaid sinh ra sơ đồ kiến trúc (Architecture diagram) và cấu trúc DB (Database schema) trong file README.
   - Viết file Postman Collection JSON để test.
   - Viết comment giải thích logic rõ ràng bằng tiếng Việt.
```

---
**Giải thích cho giám khảo:**
- **Bối cảnh, Vai trò:** Được định nghĩa rất rõ ngay từ đầu, thiết lập tiêu chuẩn code ở mức Senior.
- **Yêu cầu đầu ra:** Liệt kê rành mạch các Design Pattern sẽ dùng (Resilience, Background Queue, Cache, Fallback) để chứng tỏ người ra lệnh (bạn) có tư duy hệ thống rất tốt, không phó mặc hoàn toàn cho AI.
