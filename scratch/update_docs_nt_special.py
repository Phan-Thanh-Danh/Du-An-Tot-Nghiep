import os

# Update API_CONTRACT.md
with open('docs/API_CONTRACT.md', 'r', encoding='utf-8') as f:
    api_contract = f.read()

if 'POST /api/admin/specialized-notifications/tuition' not in api_contract:
    api_addition = """
### NT-SPECIAL - Specialized Notifications (DL/RD)
- `GET /api/admin/specialized-notifications/categories`
- `POST /api/admin/specialized-notifications/preview-recipients`
- `POST /api/admin/specialized-notifications/tuition`
- `POST /api/admin/specialized-notifications/academic`
- `POST /api/admin/specialized-notifications/urgent`
- `POST /api/admin/specialized-notifications/maintenance`
"""
    api_contract += api_addition
    with open('docs/API_CONTRACT.md', 'w', encoding='utf-8') as f:
        f.write(api_contract)

# Update BACKEND_ARCHITECTURE.md
with open('docs/BACKEND_ARCHITECTURE.md', 'r', encoding='utf-8') as f:
    architecture = f.read()

if 'SpecializedNotificationService' not in architecture:
    arch_addition = """
### Specialized Notifications (NT-SPECIAL)
- **SpecializedNotificationService**: Handles specialized notification targeting (tuition, academic, urgent, maintenance, all_students, campus, class, major, admins, custom_students). Includes idempotency mechanism using `ThongBao.DoiTuongLienKet` and `LoaiSuKien`.
"""
    architecture += arch_addition
    with open('docs/BACKEND_ARCHITECTURE.md', 'w', encoding='utf-8') as f:
        f.write(architecture)
