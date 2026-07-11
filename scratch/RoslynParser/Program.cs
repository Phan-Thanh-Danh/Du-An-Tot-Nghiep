using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace RoslynParser
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (!MSBuildLocator.IsRegistered)
            {
                var instances = MSBuildLocator.QueryVisualStudioInstances().ToArray();
                MSBuildLocator.RegisterInstance(instances.First());
            }

            var workspace = MSBuildWorkspace.Create();
            var projectPath = Path.GetFullPath(@"..\..\Backend\Backend.csproj");
            var project = await workspace.OpenProjectAsync(projectPath);
            var compilation = await project.GetCompilationAsync();

            if (compilation == null)
            {
                Console.WriteLine("Failed to get compilation.");
                return;
            }

            var endpoints = new List<EndpointInfo>();

            foreach (var syntaxTree in compilation.SyntaxTrees)
            {
                var root = await syntaxTree.GetRootAsync();
                var semanticModel = compilation.GetSemanticModel(syntaxTree);

                var classDeclarations = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                foreach (var classDecl in classDeclarations)
                {
                    var classSymbol = semanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
                    if (classSymbol == null || !classSymbol.Name.EndsWith("Controller")) continue;

                    string baseRoute = GetRouteAttributeValue(classSymbol);
                    var classAuthAttr = GetAuthorizeAttribute(classSymbol);

                    var injectedServices = GetInjectedServices(classDecl, semanticModel);

                    foreach (var methodSymbol in classSymbol.GetMembers().OfType<IMethodSymbol>())
                    {
                        if (methodSymbol.MethodKind != MethodKind.Ordinary || methodSymbol.DeclaredAccessibility != Accessibility.Public)
                            continue;

                        var httpMethod = GetHttpMethod(methodSymbol);
                        if (string.IsNullOrEmpty(httpMethod)) continue; // Not an endpoint

                        string actionRoute = GetRouteAttributeValue(methodSymbol);
                        string fullEndpoint = baseRoute;
                        if (!string.IsNullOrEmpty(actionRoute))
                        {
                            if (!fullEndpoint.EndsWith("/") && !actionRoute.StartsWith("/")) fullEndpoint += "/" + actionRoute;
                            else if (fullEndpoint.EndsWith("/") && actionRoute.StartsWith("/")) fullEndpoint += actionRoute.Substring(1);
                            else fullEndpoint += actionRoute;
                        }
                        fullEndpoint = fullEndpoint.Replace("[controller]", classSymbol.Name.Replace("Controller", ""));

                        if (!fullEndpoint.StartsWith("/")) fullEndpoint = "/" + fullEndpoint;
                        if (!fullEndpoint.StartsWith("/api")) fullEndpoint = "/api" + fullEndpoint;

                        var methodAuthAttr = GetAuthorizeAttribute(methodSymbol);
                        bool isAnonymous = HasAllowAnonymous(methodSymbol) || HasAllowAnonymous(classSymbol);

                        var (roles, policy, rawExpr) = ResolveAuth(methodAuthAttr ?? classAuthAttr, semanticModel);
                        
                        // Extract DTOs
                        var requestDtos = methodSymbol.Parameters
                            .Where(p => !p.Type.IsValueType && p.Type.SpecialType == SpecialType.None && p.Type.Name != "String")
                            .Select(p => p.Type.Name).ToList();

                        string responseDto = "";
                        var returnType = methodSymbol.ReturnType as INamedTypeSymbol;
                        if (returnType != null)
                        {
                            if (returnType.Name == "Task" && returnType.TypeArguments.Length > 0)
                            {
                                returnType = returnType.TypeArguments[0] as INamedTypeSymbol;
                            }
                            if (returnType != null && returnType.Name == "ActionResult" && returnType.TypeArguments.Length > 0)
                            {
                                responseDto = returnType.TypeArguments[0].Name;
                            }
                            else if (returnType != null)
                            {
                                responseDto = returnType.Name;
                            }
                        }

                        // We can trace method invocations here. Just get the syntax node
                        var methodSyntax = methodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;
                        var invokedMethods = new List<string>();
                        var dbSetsAccessed = new List<string>();

                        if (methodSyntax != null)
                        {
                            var invocations = methodSyntax.DescendantNodes().OfType<InvocationExpressionSyntax>();
                            foreach (var inv in invocations)
                            {
                                var symbol = semanticModel.GetSymbolInfo(inv).Symbol as IMethodSymbol;
                                if (symbol != null)
                                {
                                    if (symbol.ContainingType.Name.Contains("Service"))
                                    {
                                        invokedMethods.Add($"{symbol.ContainingType.Name}::{symbol.Name}");
                                    }
                                }
                            }
                            var memberAccesses = methodSyntax.DescendantNodes().OfType<MemberAccessExpressionSyntax>();
                            foreach (var acc in memberAccesses)
                            {
                                var symbol = semanticModel.GetSymbolInfo(acc).Symbol as IPropertySymbol;
                                if (symbol != null && symbol.Type.Name.StartsWith("DbSet"))
                                {
                                    dbSetsAccessed.Add(symbol.Name);
                                }
                            }
                        }

                        endpoints.Add(new EndpointInfo
                        {
                            Controller = classSymbol.Name,
                            Action = methodSymbol.Name,
                            HttpMethod = httpMethod,
                            Route = fullEndpoint,
                            IsAnonymous = isAnonymous,
                            HasAuthorize = (methodAuthAttr != null || classAuthAttr != null),
                            Roles = string.Join("|", roles),
                            Policy = policy,
                            RawAuthExpression = rawExpr,
                            RequestDtos = string.Join(",", requestDtos),
                            ResponseDto = responseDto,
                            InjectedServices = string.Join(",", injectedServices),
                            InvokedMethods = string.Join(",", invokedMethods.Distinct()),
                            DbSetsAccessed = string.Join(",", dbSetsAccessed.Distinct())
                        });
                    }
                }
            }

            File.WriteAllText("backend_semantic_model.json", JsonSerializer.Serialize(endpoints, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"Parsed {endpoints.Count} endpoints.");
        }

        static string GetRouteAttributeValue(ISymbol symbol)
        {
            var routeAttr = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "RouteAttribute" || a.AttributeClass?.Name == "Route");
            if (routeAttr != null && routeAttr.ConstructorArguments.Length > 0)
            {
                return routeAttr.ConstructorArguments[0].Value?.ToString() ?? "";
            }
            // Check Http method attributes that have route templates
            var httpAttr = symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name.StartsWith("Http") == true);
            if (httpAttr != null && httpAttr.ConstructorArguments.Length > 0)
            {
                return httpAttr.ConstructorArguments[0].Value?.ToString() ?? "";
            }
            return "";
        }

        static AttributeData? GetAuthorizeAttribute(ISymbol symbol)
        {
            return symbol.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "AuthorizeAttribute" || a.AttributeClass?.Name == "Authorize");
        }

        static bool HasAllowAnonymous(ISymbol symbol)
        {
            return symbol.GetAttributes().Any(a => a.AttributeClass?.Name == "AllowAnonymousAttribute" || a.AttributeClass?.Name == "AllowAnonymous");
        }

        static string GetHttpMethod(IMethodSymbol method)
        {
            if (method.GetAttributes().Any(a => a.AttributeClass?.Name == "HttpGetAttribute")) return "GET";
            if (method.GetAttributes().Any(a => a.AttributeClass?.Name == "HttpPostAttribute")) return "POST";
            if (method.GetAttributes().Any(a => a.AttributeClass?.Name == "HttpPutAttribute")) return "PUT";
            if (method.GetAttributes().Any(a => a.AttributeClass?.Name == "HttpDeleteAttribute")) return "DELETE";
            if (method.GetAttributes().Any(a => a.AttributeClass?.Name == "HttpPatchAttribute")) return "PATCH";
            return "";
        }

        static List<string> GetInjectedServices(ClassDeclarationSyntax classDecl, SemanticModel model)
        {
            var services = new List<string>();
            var ctor = classDecl.Members.OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
            if (ctor != null)
            {
                foreach (var param in ctor.ParameterList.Parameters)
                {
                    var typeSymbol = model.GetTypeInfo(param.Type!).Type;
                    if (typeSymbol != null)
                    {
                        services.Add(typeSymbol.Name);
                    }
                }
            }
            return services;
        }

        static (List<string> roles, string policy, string rawExpr) ResolveAuth(AttributeData? authAttr, SemanticModel semanticModel)
        {
            var roles = new List<string>();
            string policy = "";
            string rawExpr = "";

            if (authAttr == null) return (roles, policy, rawExpr);

            var rolesArg = authAttr.NamedArguments.FirstOrDefault(kv => kv.Key == "Roles");
            if (rolesArg.Key != null && rolesArg.Value.Value != null)
            {
                string rolesStr = rolesArg.Value.Value.ToString() ?? "";
                roles = rolesStr.Split(',').Select(r => r.Trim()).ToList();
                
                // Get the exact syntax node to extract raw expression
                var syntax = authAttr.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax;
                if (syntax != null)
                {
                    var rolesAssign = syntax.ArgumentList?.Arguments.FirstOrDefault(a => a.NameEquals?.Name.Identifier.Text == "Roles");
                    if (rolesAssign != null)
                    {
                        rawExpr = rolesAssign.Expression.ToString();
                    }
                }
            }

            var policyArg = authAttr.NamedArguments.FirstOrDefault(kv => kv.Key == "Policy");
            if (policyArg.Key != null && policyArg.Value.Value != null)
            {
                policy = policyArg.Value.Value.ToString() ?? "";
            }

            return (roles, policy, rawExpr);
        }
    }

    class EndpointInfo
    {
        public string Controller { get; set; } = "";
        public string Action { get; set; } = "";
        public string HttpMethod { get; set; } = "";
        public string Route { get; set; } = "";
        public bool IsAnonymous { get; set; }
        public bool HasAuthorize { get; set; }
        public string Roles { get; set; } = "";
        public string Policy { get; set; } = "";
        public string RawAuthExpression { get; set; } = "";
        public string RequestDtos { get; set; } = "";
        public string ResponseDto { get; set; } = "";
        public string InjectedServices { get; set; } = "";
        public string InvokedMethods { get; set; } = "";
        public string DbSetsAccessed { get; set; } = "";
    }
}
