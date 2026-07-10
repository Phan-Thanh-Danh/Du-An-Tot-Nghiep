# P16C Safe Seed Notes

### Action: Staff - /staff/assignments -> bulk assign / assign teacher if safe seed exists
- **Seed record id:** 
- **Rollback strategy:** 
- **Soft delete?:** 
- **Safe to execute?:** 
- **Decision:** RUNTIME_SKIPPED_NO_SAFE_SEED (Default pending review)

### Action: Staff - /staff/schedule -> create/update schedule if safe seed exists
- **Seed record id:** 
- **Rollback strategy:** 
- **Soft delete?:** 
- **Safe to execute?:** 
- **Decision:** RUNTIME_SKIPPED_NO_SAFE_SEED (Default pending review)

### Action: Teacher - /teacher/grading -> grade assignment submission if safe seed exists
- **Seed record id:** 
- **Rollback strategy:** 
- **Soft delete?:** 
- **Safe to execute?:** 
- **Decision:** RUNTIME_SKIPPED_NO_SAFE_SEED (Default pending review)

### Action: Student - /student/exams/{id}/take -> guarded redirect or start exam only if safe session exists
- **Seed record id:** 
- **Rollback strategy:** 
- **Soft delete?:** 
- **Safe to execute?:** 
- **Decision:** RUNTIME_SKIPPED_NO_SAFE_SEED (Default pending review)

### Action: Student - /student/requests -> create draft/submit/cancel/resubmit only if safe seed exists
- **Seed record id:** 
- **Rollback strategy:** 
- **Soft delete?:** 
- **Safe to execute?:** 
- **Decision:** RUNTIME_SKIPPED_NO_SAFE_SEED (Default pending review)

### Action: Student - /student/support-tickets -> create ticket/reply/close only if safe seed exists
- **Seed record id:** 
- **Rollback strategy:** 
- **Soft delete?:** 
- **Safe to execute?:** 
- **Decision:** RUNTIME_SKIPPED_NO_SAFE_SEED (Default pending review)

### Action: Parent - /parent/finance/payment -> do not create payment unless safe sandbox payment config exists
- **Seed record id:** 
- **Rollback strategy:** 
- **Soft delete?:** 
- **Safe to execute?:** 
- **Decision:** RUNTIME_SKIPPED_NO_SAFE_SEED (Default pending review)

### Action: SuperAdmin - /super-admin/users -> create/update/lock/unlock only if safe test account exists
- **Seed record id:** 
- **Rollback strategy:** 
- **Soft delete?:** 
- **Safe to execute?:** 
- **Decision:** RUNTIME_SKIPPED_NO_SAFE_SEED (Default pending review)

### Action: SuperAdmin - /super-admin/organizations -> create/update only if safe test org exists
- **Seed record id:** 
- **Rollback strategy:** 
- **Soft delete?:** 
- **Safe to execute?:** 
- **Decision:** RUNTIME_SKIPPED_NO_SAFE_SEED (Default pending review)

### Action: SuperAdmin - /super-admin/notifications/send -> send only to safe test recipient group
- **Seed record id:** 
- **Rollback strategy:** 
- **Soft delete?:** 
- **Safe to execute?:** 
- **Decision:** RUNTIME_SKIPPED_NO_SAFE_SEED (Default pending review)

### Action: ContentCouncil - /content-council/question-bank -> create/update/toggle/delete only if safe test question exists
- **Seed record id:** 
- **Rollback strategy:** 
- **Soft delete?:** 
- **Safe to execute?:** 
- **Decision:** RUNTIME_SKIPPED_NO_SAFE_SEED (Default pending review)

