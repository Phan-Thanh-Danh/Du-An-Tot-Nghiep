import fs from 'fs';
import path from 'path';

const VIEWS_DIR = 'frontend/src/views';
const AUDIT_FILE = 'docs/P20_SKELETON_LOADING_AUDIT.md';

function scanDirectory(dir, fileList = []) {
  const files = fs.readdirSync(dir);
  for (const file of files) {
    const filePath = path.join(dir, file);
    if (fs.statSync(filePath).isDirectory()) {
      scanDirectory(filePath, fileList);
    } else if (filePath.endsWith('.vue')) {
      fileList.push(filePath);
    }
  }
  return fileList;
}

const vueFiles = scanDirectory(VIEWS_DIR);

let markdown = `# P20 Skeleton Loading Audit\n\n`;
markdown += `| Route/Screen | View file | API load state exists? | Current loading UI | Required skeleton? | Status |\n`;
markdown += `| --- | --- | --- | --- | --- | --- |\n`;

for (const file of vueFiles) {
  const content = fs.readFileSync(file, 'utf8');
  const relPath = file.replace(/\\/g, '/').replace('frontend/src/views/', '');
  
  const hasLoadingState = /loading|isLoading|pending|fetching/i.test(content);
  const hasSkeleton = /LoadingSkeleton|SkeletonTable|SkeletonCard|SkeletonDashboard|SkeletonBlock|SkeletonChart|animate-pulse/i.test(content);
  const hasSpinner = /spinner|Loader2|lucide-vue-next.*Loader|loading-icon/i.test(content);
  const hasApiCall = /Api\./i.test(content) || /fetch/i.test(content) || /axios/i.test(content) || /apiClient/i.test(content);

  let currentUi = 'None';
  if (hasSkeleton) currentUi = 'Skeleton';
  else if (hasSpinner) currentUi = 'Spinner';
  else if (hasLoadingState) currentUi = 'Text/Opacity';

  let status = 'NEEDS_SKELETON';
  let requiredSkeleton = 'Yes';
  let apiLoadState = hasLoadingState ? 'Yes' : 'No';

  if (!hasApiCall && !hasLoadingState) {
    status = 'UI_ONLY_NO_SKELETON_NEEDED';
    requiredSkeleton = 'No';
  } else if (hasSkeleton) {
    status = 'HAS_GOOD_SKELETON';
  } else if (hasSpinner) {
    status = 'SPINNER_OK';
  }

  // Simple heuristic for screen name
  const screenName = relPath.split('.')[0];
  
  markdown += `| ${screenName} | ${relPath} | ${apiLoadState} | ${currentUi} | ${requiredSkeleton} | ${status} |\n`;
}

fs.writeFileSync(AUDIT_FILE, markdown);
console.log('Audit generated at ' + AUDIT_FILE);
