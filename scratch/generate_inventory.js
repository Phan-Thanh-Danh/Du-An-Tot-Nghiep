const fs = require('fs');
const path = require('path');
const crypto = require('crypto');

const endpoints = JSON.parse(fs.readFileSync('scratch/RoslynParser/backend_semantic_model.json', 'utf8'));

let inventoryCsv = 'EndpointId,Controller,Action,HttpMethod,Route,IsAnonymous,HasAuthorize,CanonicalRoles,Policy,RequestDtos,ResponseDto,InjectedServices,InvokedMethods,DbSetsAccessed\n';

const endpointMap = new Map();

for (const ep of endpoints) {
    // Include controller to handle duplicate routes gracefully
    const epId = 'EP-' + crypto.createHash('md5').update(`${ep.Controller} ${ep.HttpMethod} ${ep.Route}`).digest('hex').substring(0, 8).toUpperCase();
    
    let canonicalRoles = ep.Roles.split('|').filter(r => r).map(r => r.replace('AuthRoles.', ''));
    
    inventoryCsv += `${epId},${ep.Controller},${ep.Action},${ep.HttpMethod},${ep.Route},${ep.IsAnonymous},${ep.HasAuthorize},${canonicalRoles.join('|')},${ep.Policy},"${ep.RequestDtos}","${ep.ResponseDto}","${ep.InjectedServices}","${ep.InvokedMethods}","${ep.DbSetsAccessed}"\n`;
    
    endpointMap.set(epId, ep);
}

fs.writeFileSync('docs/p0/P0_BACKEND_ENDPOINT_INVENTORY.csv', inventoryCsv, 'utf8');
console.log('Generated docs/p0/P0_BACKEND_ENDPOINT_INVENTORY.csv');
