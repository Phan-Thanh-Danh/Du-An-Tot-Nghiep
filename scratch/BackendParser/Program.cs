using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BackendParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var controllersPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Backend", "Controllers"));
            var files = Directory.GetFiles(controllersPath, "*.cs", SearchOption.AllDirectories);

            Console.WriteLine("Controller|BaseRoute|Action|HttpMethod|Endpoint|Authorize|Roles|Policy|AllowAnonymous|RequestDto|ResponseDto|InjectedServices");

            foreach (var file in files)
            {
                var code = File.ReadAllText(file);
                var tree = CSharpSyntaxTree.ParseText(code);
                var root = tree.GetCompilationUnitRoot();

                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                foreach (var classDecl in classes)
                {
                    var className = classDecl.Identifier.Text;
                    if (!className.EndsWith("Controller")) continue;

                    string baseRoute = "";
                    string classAuth = "";
                    string classRoles = "";
                    string classPolicy = "";
                    bool classAllowAnonymous = false;

                    // Extract injected services (Constructor parameters)
                    var constructor = classDecl.DescendantNodes().OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
                    var injectedServices = new List<string>();
                    if (constructor != null)
                    {
                        foreach (var param in constructor.ParameterList.Parameters)
                        {
                            if (param.Type != null) {
                                string typeName = param.Type.ToString();
                                if (typeName.Contains("Service") || typeName.Contains("Context") || typeName.Contains("Manager") || typeName.Contains("Repo"))
                                {
                                    injectedServices.Add(typeName);
                                }
                            }
                        }
                    }

                    foreach (var attrList in classDecl.AttributeLists)
                    {
                        foreach (var attr in attrList.Attributes)
                        {
                            var attrName = attr.Name.ToString();
                            if (attrName == "Route")
                                baseRoute = attr.ArgumentList?.Arguments.FirstOrDefault()?.Expression.ToString().Trim('"') ?? "";
                            else if (attrName == "Authorize")
                            {
                                classAuth = "Yes";
                                if (attr.ArgumentList != null)
                                {
                                    foreach (var arg in attr.ArgumentList.Arguments)
                                    {
                                        var nameEquals = arg.NameEquals?.Name.Identifier.Text;
                                        if (nameEquals == "Roles") classRoles = arg.Expression.ToString().Trim('"');
                                        if (nameEquals == "Policy") classPolicy = arg.Expression.ToString().Trim('"');
                                    }
                                }
                            }
                            else if (attrName == "AllowAnonymous")
                                classAllowAnonymous = true;
                        }
                    }

                    var methods = classDecl.DescendantNodes().OfType<MethodDeclarationSyntax>();
                    foreach (var method in methods)
                    {
                        if (method.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword)))
                        {
                            string actionName = method.Identifier.Text;
                            string httpMethod = "";
                            string actionRoute = "";
                            string actionAuth = classAuth;
                            string actionRoles = classRoles;
                            string actionPolicy = classPolicy;
                            bool actionAllowAnonymous = classAllowAnonymous;

                            // Extract RequestDto (From method parameters)
                            var requestDtos = new List<string>();
                            foreach (var param in method.ParameterList.Parameters)
                            {
                                if (param.Type != null)
                                {
                                    string pType = param.Type.ToString();
                                    if (pType.EndsWith("Dto") || pType.EndsWith("Request") || pType.EndsWith("Query") || pType.EndsWith("Command"))
                                    {
                                        requestDtos.Add(pType);
                                    }
                                }
                            }
                            
                            // Extract ResponseDto (From return type)
                            string responseDto = "";
                            if (method.ReturnType != null)
                            {
                                string ret = method.ReturnType.ToString();
                                if (ret.Contains("<"))
                                {
                                    int start = ret.IndexOf('<');
                                    int end = ret.LastIndexOf('>');
                                    if (start != -1 && end > start) {
                                        responseDto = ret.Substring(start + 1, end - start - 1);
                                    }
                                }
                            }

                            foreach (var attrList in method.AttributeLists)
                            {
                                foreach (var attr in attrList.Attributes)
                                {
                                    var attrName = attr.Name.ToString();
                                    if (attrName.StartsWith("HttpGet")) { httpMethod = "GET"; actionRoute = GetRoute(attr); }
                                    else if (attrName.StartsWith("HttpPost")) { httpMethod = "POST"; actionRoute = GetRoute(attr); }
                                    else if (attrName.StartsWith("HttpPut")) { httpMethod = "PUT"; actionRoute = GetRoute(attr); }
                                    else if (attrName.StartsWith("HttpDelete")) { httpMethod = "DELETE"; actionRoute = GetRoute(attr); }
                                    else if (attrName.StartsWith("HttpPatch")) { httpMethod = "PATCH"; actionRoute = GetRoute(attr); }
                                    else if (attrName == "Route") { actionRoute = GetRoute(attr); }
                                    else if (attrName == "Authorize")
                                    {
                                        actionAuth = "Yes";
                                        if (attr.ArgumentList != null)
                                        {
                                            foreach (var arg in attr.ArgumentList.Arguments)
                                            {
                                                var nameEquals = arg.NameEquals?.Name.Identifier.Text;
                                                if (nameEquals == "Roles") actionRoles = arg.Expression.ToString().Trim('"');
                                                if (nameEquals == "Policy") actionPolicy = arg.Expression.ToString().Trim('"');
                                            }
                                        }
                                    }
                                    else if (attrName == "AllowAnonymous")
                                        actionAllowAnonymous = true;
                                }
                            }

                            if (!string.IsNullOrEmpty(httpMethod))
                            {
                                string fullEndpoint = baseRoute;
                                if (!string.IsNullOrEmpty(actionRoute)) {
                                    if (!fullEndpoint.EndsWith("/") && !actionRoute.StartsWith("/")) fullEndpoint += "/" + actionRoute;
                                    else if (fullEndpoint.EndsWith("/") && actionRoute.StartsWith("/")) fullEndpoint += actionRoute.Substring(1);
                                    else fullEndpoint += actionRoute;
                                }
                                fullEndpoint = fullEndpoint.Replace("[controller]", className.Replace("Controller", ""));

                                Console.WriteLine($"{className}|{baseRoute}|{actionName}|{httpMethod}|{fullEndpoint}|{actionAuth}|{actionRoles}|{actionPolicy}|{actionAllowAnonymous}|{string.Join(",", requestDtos)}|{responseDto}|{string.Join(",", injectedServices)}");
                            }
                        }
                    }
                }
            }
        }

        static string GetRoute(AttributeSyntax attr)
        {
            if (attr.ArgumentList != null && attr.ArgumentList.Arguments.Count > 0)
            {
                var arg = attr.ArgumentList.Arguments.FirstOrDefault();
                if (arg != null && arg.NameEquals == null)
                {
                    return arg.Expression.ToString().Trim('"');
                }
            }
            return "";
        }
    }
}
