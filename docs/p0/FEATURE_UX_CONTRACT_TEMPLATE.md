# Feature UX Contract Template

## 1. Feature Identification
- **Feature ID**: [e.g., FEAT-ATT-001]
- **Role**: [e.g., Teacher, Student]
- **Module**: [e.g., Academic, Finance]
- **Business objective**: [What business value does this bring?]
- **Primary actor**: [Who performs this?]

## 2. Workflows & Flows
- **Entry points**: [How does the user get here? URL/Menu]
- **Main flow**: [Step-by-step sunny day scenario]
- **Alternative flows**: [What else can happen?]
- **Preconditions**: [What must be true before starting?]

## 3. Authorization & Permissions
- **Authorization**: [Roles allowed]
- **Permissions**: [Specific policies or claims required]
- **Backend capability status**: [IMPLEMENTED, PARTIAL, MISSING, UNVERIFIED]
- **Supported operations**: [List from capability matrix]
- **Missing operations**: [What is not yet supported in BE]

## 4. UI/UX Details
- **State transitions**: [From State A -> State B]
- **API contracts**: [Request/Response definitions]
- **Data fields**: [List of visible fields, formats, validations]
- **Primary action**: [The main CTA]
- **Secondary actions**: [Other actions]
- **Validation**: [Frontend validation rules]
- **Loading**: [Skeleton, spinners, states]
- **Empty**: [Empty state design]
- **Filter-empty**: [No results found state]
- **Error**: [Error handling]
- **Forbidden**: [403 state]
- **Offline**: [What happens if network drops]
- **Saving**: [Saving indicators]
- **Unsaved changes**: [Prompt before leaving?]
- **Success feedback**: [Toast, redirect?]
- **Destructive confirmation**: [Are you sure?]

## 5. Platforms & Responsiveness
- **Desktop UX**: [Layout on lg screens]
- **Tablet UX**: [Layout on md screens]
- **Mobile UX**: [Layout on sm screens]
- **Keyboard behavior**: [Tab order, shortcuts]
- **Accessibility**: [ARIA, contrast]

## 6. Audit & Edge Cases
- **Audit requirements**: [Activity logging, tracking]
- **Notification requirements**: [Push, email, in-app]
- **Edge cases**: [Weird data, slow network]

## 7. Delivery
- **Acceptance criteria**: [Definition of Done]
- **Out of scope**: [What is intentionally excluded]
- **Backend backlog references**: [IDs of missing BE tasks]
