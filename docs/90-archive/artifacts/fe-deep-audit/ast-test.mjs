import { parse } from "@babel/parser";
import { readFileSync } from "fs";

const src = readFileSync("frontend/src/router/index.js", "utf8");

const ast = parse(src, {
  sourceType: "module",
  plugins: ["importAttributes", "jsx"]
});

// Find createRouter call
function findCreateRouterCall(node) {
  if (node.type === "CallExpression" && 
      node.callee.type === "Identifier" && 
      node.callee.name === "createRouter") {
    return node;
  }
  for (const key of Object.keys(node)) {
    if (node[key] && typeof node[key] === "object") {
      if (Array.isArray(node[key])) {
        for (const item of node[key]) {
          if (item && typeof item === "object") {
            const found = findCreateRouterCall(item);
            if (found) return found;
          }
        }
      } else {
        const found = findCreateRouterCall(node[key]);
        if (found) return found;
      }
    }
  }
  return null;
}

const routerCall = findCreateRouterCall(ast);
if (!routerCall) {
  console.log("Could not find createRouter call");
  process.exit(1);
}

// Find routes: property in the argument object
const routerArg = routerCall.arguments[0];
if (routerArg.type !== "ObjectExpression") {
  console.log("Router argument is not an object");
  process.exit(1);
}

const routesProp = routerArg.properties.find(p => 
  p.type === "ObjectProperty" && p.key.type === "Identifier" && p.key.name === "routes"
);

if (!routesProp) {
  console.log("No routes property found");
  process.exit(1);
}

function getStringValue(node) {
  if (node.type === "StringLiteral") return node.value;
  if (node.type === "TemplateLiteral" && node.quasis.length === 1) return node.quasis[0].value.cooked;
  return null;
}

function getKeyName(prop) {
  if (prop.type !== "ObjectProperty" && prop.type !== "ObjectMethod") return null;
  if (prop.key.type === "Identifier") return prop.key.name;
  if (prop.key.type === "StringLiteral") return prop.key.value;
  return null;
}

function getBooleanValue(node) {
  if (node.type === "BooleanLiteral") return node.value;
  return null;
}

function extractRouteObject(node, parentPath, parentRole) {
  if (node.type !== "ObjectExpression") return null;
  
  const route = {
    path: "",
    name: "",
    component: "",
    redirect: "",
    meta: { title: "", subtitle: "", section: "", requiresAuth: false, public: false, fullscreen: false, previewMode: false, role: null },
    children: [],
    isLayout: false
  };
  
  for (const prop of node.properties) {
    if (prop.type !== "ObjectProperty") continue;
    const key = getKeyName(prop);
    if (!key) continue;
    
    if (key === "path") {
      const val = getStringValue(prop.value);
      if (val !== null) route.path = val;
    } else if (key === "name") {
      const val = getStringValue(prop.value);
      if (val !== null) route.name = val;
    } else if (key === "component") {
      // component: () => import('...') or component: SomeComponent
      if (prop.value.type === "ArrowFunctionExpression" || 
          prop.value.type === "FunctionExpression") {
        const body = prop.value.body;
        if (body.type === "CallExpression" || 
            (body.type === "BlockStatement" && 
             body.body.length === 1 && 
             body.body[0].type === "ReturnStatement" && 
             body.body[0].argument.type === "CallExpression")) {
          const call = body.type === "CallExpression" ? body : body.body[0].argument;
          if (call.callee.type === "Import" && call.arguments.length > 0) {
            const impPath = getStringValue(call.arguments[0]);
            if (impPath) route.component = impPath;
          }
        }
      }
    } else if (key === "redirect") {
      if (prop.value.type === "StringLiteral") {
        route.redirect = prop.value.value;
      } else {
        route.redirect = "(function)";
      }
    } else if (key === "children") {
      if (prop.value.type === "ArrayExpression") {
        route.isLayout = true;
        for (const child of prop.value.elements) {
          if (child && child.type === "ObjectExpression") {
            route.children.push(child);
          }
        }
      }
    } else if (key === "meta") {
      if (prop.value.type === "ObjectExpression") {
        for (const mp of prop.value.properties) {
          if (mp.type !== "ObjectProperty") continue;
          const mk = getKeyName(mp);
          if (!mk) continue;
          if (mk === "title") route.meta.title = getStringValue(mp.value) || "";
          else if (mk === "subtitle") route.meta.subtitle = getStringValue(mp.value) || "";
          else if (mk === "section") route.meta.section = getStringValue(mp.value) || "";
          else if (mk === "requiresAuth") route.meta.requiresAuth = getBooleanValue(mp.value) || false;
          else if (mk === "public") route.meta.public = getBooleanValue(mp.value) || false;
          else if (mk === "fullscreen") route.meta.fullscreen = getBooleanValue(mp.value) || false;
          else if (mk === "previewMode") route.meta.previewMode = getBooleanValue(mp.value) || false;
          else if (mk === "role") {
            if (mp.value.type === "StringLiteral") route.meta.role = mp.value.value;
            else if (mp.value.type === "ArrayExpression") {
              route.meta.role = mp.value.elements
                .filter(e => e && e.type === "StringLiteral")
                .map(e => e.value)
                .join("|");
            }
          }
        }
      }
    }
  }
  
  return route;
}

// Traverse the routes array
function flattenRoutes(routeNodes, parentPath, parentRole) {
  const result = [];
  for (const node of routeNodes) {
    if (!node || node.type !== "ObjectExpression") continue;
    const route = extractRouteObject(node, parentPath, parentRole);
    if (!route) continue;
    
    const fullPath = route.path.startsWith("/") ? route.path : 
                     parentPath ? parentPath + "/" + route.path : route.path;
    
    // Determine role
    let role = route.meta.role || parentRole || 
               (route.meta.public ? "Public" : 
                route.path.startsWith("/:pathMatch") ? "404" : null);
    if (!role && !parentPath) role = "Public";
    if (!role && parentRole) role = parentRole;
    if (!role) role = "Public";
    
    // RecordType
    let recordType = "PAGE";
    if (route.path.startsWith("/:pathMatch")) recordType = "CATCH_ALL";
    else if (route.redirect && !route.isLayout) recordType = "REDIRECT";
    else if (route.isLayout) recordType = "LAYOUT";
    else if (route.meta.fullscreen) recordType = "FULLSCREEN";
    
    // Skip empty-path redirect children (they're not separate page records)
    if (route.path === "" && route.redirect) continue;
    
    result.push({
      path: fullPath,
      name: route.name || "UNNAMED",
      component: route.component || "",
      redirect: route.redirect,
      meta: route.meta,
      isLayout: route.isLayout,
      recordType,
      role,
      children: []
    });
    
    // Recursively process children
    if (route.isLayout && route.children.length > 0) {
      const childRoutes = flattenRoutes(route.children, fullPath, role);
      result[result.length - 1].children = childRoutes;
      result.push(...childRoutes);
    }
  }
  return result;
}

const routesArray = routesProp.value;
if (routesArray.type !== "ArrayExpression") {
  console.log("Routes is not an array");
  process.exit(1);
}

const allRoutes = flattenRoutes(routesArray.elements, "", "");

// Count by RecordType
const byType = {};
const byRole = {};
for (const r of allRoutes) {
  byType[r.recordType] = (byType[r.recordType] || 0) + 1;
  byRole[r.role] = (byRole[r.role] || 0) + 1;
}

console.log("Total routes:", allRoutes.length);
console.log("\nBy RecordType:");
for (const [k, v] of Object.entries(byType).sort()) console.log(`  ${k}: ${v}`);
console.log("\nBy Role:");
for (const [k, v] of Object.entries(byRole).sort()) console.log(`  ${k}: ${v}`);

// Verify specific paths
console.log("\nSample routes:");
for (const r of allRoutes.slice(0, 10)) {
  console.log(`  ${r.recordType} ${r.role} ${r.path} name=${r.name}`);
}

// Check for /login
const login = allRoutes.find(r => r.path === "/login");
if (login) console.log("\n/login:", login.recordType, login.role, "name=", login.name);
