const fs = require('fs');

const capabilitiesCsv = fs.readFileSync('docs/p0/P0_BACKEND_CAPABILITY_MATRIX.csv', 'utf8').split('\n').filter(Boolean).slice(1);
const capabilities = capabilitiesCsv.map(line => {
    // Basic CSV parse (ignores commas in quotes for now since we generated them without internal commas)
    const parts = line.split(',');
    return {
        id: parts[0],
        op: parts[1].replace(/"/g, ''),
        role: parts[2],
        beStatus: parts[3],
        feStatus: parts[4],
        endpoints: parts[5],
        evidence: parts[6]
    };
});

const roles = [
  {
    name: 'STUDENT',
    canonical: 'Student',
    dbCode: 'hoc_sinh',
    aliases: 'SinhVien, Student',
    homeRoute: '/student/dashboard',
    layout: 'Layout_SinhVien.vue',
    ownership: 'frontend/src/views/Student/, frontend/src/views/SinhVien/'
  },
  {
    name: 'TEACHER',
    canonical: 'Teacher',
    dbCode: 'giao_vien',
    aliases: 'GiangVien, Teacher',
    homeRoute: '/teacher/dashboard',
    layout: 'Layout_GiangVien.vue',
    ownership: 'frontend/src/views/GiangVien/'
  },
  {
    name: 'ACADEMIC_STAFF',
    canonical: 'AcademicStaff',
    dbCode: 'nhan_vien',
    aliases: 'GiaoVu, Staff',
    homeRoute: '/staff/dashboard',
    layout: 'Layout_GiaoVu.vue',
    ownership: 'frontend/src/views/GiaoVu/'
  },
  {
    name: 'PARENT',
    canonical: 'Parent',
    dbCode: 'phu_huynh',
    aliases: 'PhuHuynh, Parent',
    homeRoute: '/parent/dashboard',
    layout: 'Layout_PhuHuynh.vue',
    ownership: 'frontend/src/views/PhuHuynh/'
  },
  {
    name: 'PRINCIPAL',
    canonical: 'Principal',
    dbCode: 'hieu_truong',
    aliases: 'BGH, Principal',
    homeRoute: '/bgh/dashboard',
    layout: 'Layout_BGH.vue',
    ownership: 'frontend/src/views/BGH/'
  },
  {
    name: 'SUPER_ADMIN',
    canonical: 'SuperAdmin',
    dbCode: 'sieu_quan_tri',
    aliases: 'SuperAdmin, Admin',
    homeRoute: '/super-admin/dashboard',
    layout: 'Layout_Admin.vue',
    ownership: 'frontend/src/views/SuperAdmin/'
  },
  {
    name: 'CONTENT_COUNCIL',
    canonical: 'HoiDongQuanLyNoiDung',
    dbCode: 'hoidong_quanly_noidung',
    aliases: 'ContentCouncil',
    homeRoute: '/content-council/subjects',
    layout: 'Layout_ContentCouncil.vue',
    ownership: 'frontend/src/pages/content-council/, frontend/src/components/content-council/'
  }
];

if (!fs.existsSync('docs/p0/roles')) {
    fs.mkdirSync('docs/p0/roles', { recursive: true });
}

for (const role of roles) {
  const roleCaps = capabilities.filter(c => c.role === role.canonical || c.role === 'Admin'); // including Admin for some shared ones if needed, but strict to canonical is better. Let's just do canonical.
  const strictCaps = capabilities.filter(c => c.role === role.canonical);
  
  const implemented = strictCaps.filter(c => c.beStatus === 'IMPLEMENTED').map(c => `- ${c.op} (${c.endpoints})`);
  const partial = strictCaps.filter(c => c.beStatus === 'PARTIAL').map(c => `- ${c.op} (${c.endpoints})`);
  const missing = strictCaps.filter(c => c.beStatus === 'MISSING').map(c => `- ${c.op}`);

  const content = `# ${role.name} Handoff Package

## 1. Identity
- **Canonical backend role**: \`${role.canonical}\`
- **Database role code**: \`${role.dbCode}\`
- **Existing frontend aliases**: ${role.aliases}

## 2. Architecture & Ownership
- **Exact folder ownership**: \`${role.ownership}\`
- **Actual home route**: \`${role.homeRoute}\`
- **Layout**: \`${role.layout}\`
- **Menu source**: \`frontend/src/router/index.js\` and API dynamic menus
- **Shared components**: \`frontend/src/components/common/\`

## 3. Capabilities

### Supported operations
${implemented.length > 0 ? implemented.join('\n') : '- None fully implemented'}

### PARTIAL operations
${partial.length > 0 ? partial.join('\n') : '- None'}

### MISSING operations
${missing.length > 0 ? missing.join('\n') : '- None'}

## 4. UI/UX
- **Wrong-context views**: Ensure no other role's logic leaks into \`${role.ownership}\`
- **Static/mock screens**: Must be connected to real APIs
- **UX direction**: Follow the feature UX contracts. (Priority: High)

## 5. Rules
- **Files that must not be modified**: \`router/index.js\`, \`stores/auth.js\`, \`SafeHtmlRenderer.vue\` (Require Core Team review)
- **Prioritized implementation tasks**: Complete all MISSING capabilities first.
- **Definition of Done**:
  - Endpoint fully connected.
  - No mock data used.
  - SafeHtmlRenderer used for any rich text.
  - Skeleton loading implemented.
  - Permissions strictly enforced on FE and BE.
`;

  fs.writeFileSync(`docs/p0/roles/${role.name}_HANDOFF.md`, content, 'utf8');
}
console.log('Generated role handoffs');
