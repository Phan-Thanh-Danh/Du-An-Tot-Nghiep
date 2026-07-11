const fs = require('fs');
const crypto = require('crypto');

const endpoints = JSON.parse(fs.readFileSync('scratch/RoslynParser/backend_semantic_model.json', 'utf8'));

function getEvidence(ep) {
    if (!ep) return 'MISSING';
    
    if (ep.Action === "NotImplemented" || ep.ResponseDto === "NotImplementedException") return 'MISSING';

    const evidence = [
        `Controller: ${ep.Controller}::${ep.Action}`
    ];
    
    let hasLogic = false;
    if (ep.InvokedMethods) {
        evidence.push(`ServiceMethod: ${ep.InvokedMethods}`);
        hasLogic = true;
    } else if (ep.InjectedServices && ep.InjectedServices.includes('DbContext')) {
        evidence.push(`DIRECT_DB_CONTEXT`);
        hasLogic = true;
    }
    
    if (ep.RequestDtos) evidence.push(`RequestDto: ${ep.RequestDtos}`);
    if (ep.ResponseDto) evidence.push(`ResponseDto: ${ep.ResponseDto}`);
    
    let hasDbSet = false;
    if (ep.DbSetsAccessed) {
        evidence.push(`EntityOrDbSet: ${ep.DbSetsAccessed}`);
        hasDbSet = true;
    } else {
        evidence.push(`EntityOrDbSet: UNVERIFIED`);
    }

    if (ep.HasAuthorize) {
        evidence.push(`Authorization: ${ep.Roles || 'Yes'}`);
        if (ep.Policy) evidence.push(`Policy: ${ep.Policy}`);
    } else if (ep.IsAnonymous) {
        evidence.push(`Authorization: Anonymous`);
    }
    
    // Determine strict status based on extracted evidence
    if (!hasLogic && !hasDbSet) return { status: 'PARTIAL', text: evidence.join(' | ') };
    return { status: 'IMPLEMENTED', text: evidence.join(' | ') };
}

const capabilities = [
    { id: 'CAP-ATT-001', op: 'Teacher opens attendance', role: 'Teacher', fe: 'IMPLEMENTED', be: 'IMPLEMENTED', findEp: e => e.Route.includes('/attendance/start') },
    { id: 'CAP-ATT-002', op: 'Teacher bulk-updates attendance', role: 'Teacher', fe: 'IMPLEMENTED', be: 'IMPLEMENTED', findEp: e => e.Route.includes('/attendance/bulk') },
    { id: 'CAP-ATT-003', op: 'Teacher submits attendance', role: 'Teacher', fe: 'IMPLEMENTED', be: 'IMPLEMENTED', findEp: e => e.Route.includes('/attendance/submit') },
    { id: 'CAP-PAY-001', op: 'Parent views tuition', role: 'Parent', fe: 'IMPLEMENTED', be: 'IMPLEMENTED', findEp: e => e.Route.includes('/tuition') && e.Route.includes('parent') },
    { id: 'CAP-PAY-002', op: 'Parent initiates payment', role: 'Parent', fe: 'PARTIAL', be: 'PARTIAL', findEp: e => e.Route.includes('/payment') && e.Route.includes('parent') },
    { id: 'CAP-APP-001', op: 'Student submits an application', role: 'Student', fe: 'IMPLEMENTED', be: 'IMPLEMENTED', findEp: e => e.Controller === 'StudentApplicationsController' && e.HttpMethod === 'POST' && e.Route.includes('/submit') },
    { id: 'CAP-APP-002', op: 'Student views applications', role: 'Student', fe: 'IMPLEMENTED', be: 'IMPLEMENTED', findEp: e => e.Controller === 'StudentApplicationsController' && e.HttpMethod === 'GET' && !e.Route.includes('{id') },
    { id: 'CAP-QZ-001', op: 'Content Council publishes a quiz', role: 'HoiDongQuanLyNoiDung', fe: 'DESIGNED_NOT_CONNECTED', be: 'MISSING', findEp: e => e.Route.includes('/quizzes') && e.Route.includes('publish') },
    { id: 'CAP-QB-001', op: 'Content Council manages question bank', role: 'HoiDongQuanLyNoiDung', fe: 'IMPLEMENTED', be: 'IMPLEMENTED', findEp: e => e.Controller.includes('QuestionBank') || e.Controller.includes('Questions') },
    { id: 'CAP-GRD-001', op: 'Student views grades', role: 'Student', fe: 'IMPLEMENTED', be: 'IMPLEMENTED', findEp: e => e.Route.includes('/student/grades') },
    { id: 'CAP-SCH-001', op: 'Academic Staff generates schedule', role: 'AcademicStaff', fe: 'IMPLEMENTED', be: 'IMPLEMENTED', findEp: e => e.Controller === 'ThoiKhoaBieuController' && e.Route.includes('generate') },
    { id: 'CAP-USR-001', op: 'Admin creates user', role: 'Admin', fe: 'IMPLEMENTED', be: 'IMPLEMENTED', findEp: e => e.Controller === 'AdminAccountsController' && e.HttpMethod === 'POST' }
];

let csv = 'CapabilityId,BusinessOperation,FrontendRole,BackendStatus,FrontendStatus,MatchedEndpointIds,Evidence\n';

for (const cap of capabilities) {
    const matched = endpoints.filter(cap.findEp);
    
    let epIds = [];
    let evidenceParts = [];
    let calculatedBeStatus = cap.be;
    
    if (matched.length > 0) {
        let anyPartial = false;
        for (const ep of matched) {
            const eid = 'EP-' + crypto.createHash('md5').update(`${ep.Controller} ${ep.HttpMethod} ${ep.Route}`).digest('hex').substring(0, 8).toUpperCase();
            epIds.push(eid);
            const ev = getEvidence(ep);
            evidenceParts.push(`[${ep.HttpMethod} ${ep.Route}] ` + ev.text);
            if (ev.status === 'PARTIAL') anyPartial = true;
        }
        if (anyPartial && calculatedBeStatus === 'IMPLEMENTED') calculatedBeStatus = 'PARTIAL';
    } else {
        calculatedBeStatus = 'MISSING';
    }
    
    let evidence = evidenceParts.join(' || ');
    if (calculatedBeStatus === 'MISSING') evidence = 'No backend endpoint found.';
    else if (calculatedBeStatus === 'PARTIAL' && cap.be !== 'PARTIAL') {
        evidence = 'Endpoint lacks robust implementation evidence (Service method or DB access not explicitly tracked). ' + evidence;
    } else if (calculatedBeStatus === 'PARTIAL' && cap.be === 'PARTIAL') {
        evidence = 'Endpoint exists but workflow/side-effects are incomplete. ' + evidence;
    }

    csv += `${cap.id},"${cap.op}",${cap.role},${calculatedBeStatus},${cap.fe},"${epIds.join('|')}","${evidence}"\n`;
}

fs.writeFileSync('docs/p0/P0_BACKEND_CAPABILITY_MATRIX.csv', csv, 'utf8');
console.log('Generated Capability Matrix');
