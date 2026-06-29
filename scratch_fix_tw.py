import os
import re

directory = r"c:\Users\lapto\OneDrive\Desktop\Du-An-Tot-Nghiep\frontend\src"

pattern = re.compile(r'([a-z0-9A-Z_:]+)-\[var\(--([a-zA-Z0-9-]+)\)\]')

def process_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    new_content, count = pattern.subn(r'\1-(--\2)', content)
    
    if count > 0:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(new_content)
        print(f"Fixed {count} matches in {os.path.relpath(filepath, directory)}")

for root, _, files in os.walk(directory):
    for file in files:
        if file.endswith('.vue') or file.endswith('.js'):
            process_file(os.path.join(root, file))

print("Done fixing TW syntax.")
