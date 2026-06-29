import re

def fix_file(path):
    with open(path, 'r', encoding='utf-8') as f:
        content = f.read()

    # Matches throw new ApiException("...", (int)HttpStatusCode.XXX);
    # And throw new ApiException("...", 401);
    pattern = r'throw new ApiException\(\s*\"(.*?)\"\s*,\s*(\(int\)[^)]+|\d+)\s*\);'
    
    def repl(m):
        msg = m.group(1)
        code = m.group(2)
        return f'throw new ApiException({code}, "{msg}");'
        
    content = re.sub(pattern, repl, content)

    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)

fix_file('Backend/Services/Notification/NotificationTemplateService.cs')
fix_file('Backend/Controllers/AdminNotificationTemplatesController.cs')
