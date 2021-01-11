using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace JsonGenerators
{
    [Generator]
    public class JsonDeserializationGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var receiver = (SyntaxReceiver) context.SyntaxReceiver;
            var candidates = receiver.DeserializationCandidates;
            foreach (var candidate in candidates)
            {
                var typeSymbol = (ITypeSymbol) context.Compilation.GetSemanticModel(candidate.SyntaxTree).GetDeclaredSymbol(candidate);
                foreach (var attribute in typeSymbol.GetAttributes())
                {
                    if (attribute.AttributeClass.Name == nameof(GenerateJsonDeserializerAttribute))
                    {
                        var ns = GetNamespaceName(candidate, context.Compilation);
                        var code = GetGeneratedDeserializerCode(typeSymbol, ns);
                        
                        context.AddSource($"{ns}.{typeSymbol.Name}Generator", SourceText.From(code, Encoding.Default));
                    }
                }
            }
        }

        private static string GetGeneratedDeserializerCode(ITypeSymbol symbol, string namespaceName)
        {
            var propertyCodePieces = new List<string>();
            
            var properties = symbol.GetMembers().OfType<IPropertySymbol>().ToArray();
            foreach (var property in properties)
            {
                string template;
                switch (property.Type.ToDisplayString())
                {
                    case "int":
                        template = GeneratorCodeTemplates.IntPropertyTemplate;
                        break;
                    
                    case "int?":
                        template = GeneratorCodeTemplates.NullableIntPropertyTemplate;
                        break;
                    
                    case "bool":
                        template = GeneratorCodeTemplates.BoolPropertyTemplate;
                        break;
                    
                    case "string":
                        template = GeneratorCodeTemplates.StringPropertyTemplate;
                        break;
                    
                    case "double":
                        template = GeneratorCodeTemplates.DoublePropertyTemplate;
                        break;
                    
                    default:
                        throw new InvalidOperationException($"No template exists for type '{property.Type.Name}'");
                }
                
                propertyCodePieces.Add(string.Format(template, symbol.Name, property.Name));
            }

            var propertyText = string.Concat(propertyCodePieces);
            return string.Format(GeneratorCodeTemplates.OverallCodeTemplate, namespaceName, symbol.Name, propertyText);
        }

        private static string GetNamespaceName(SyntaxNode node, Compilation compilation)
        {
            // I could not find an easy and reliable way to get the namespace from the semantic model or type symbol.
            // So the only way I can figure out to get the go up the syntax tree until we find a namespace declaration.
            // This
            var ns = new StringBuilder();
            var nsFound = false;
            node = node.Parent;
            while (node != null)
            {
                var model = compilation.GetSemanticModel(node.SyntaxTree);
                var name = (string) null;
                if (node is ClassDeclarationSyntax classNode)
                {
                    name = ((ITypeSymbol) model.GetDeclaredSymbol(node)).Name;
                }
                else if (node is NamespaceDeclarationSyntax namespaceNode)
                {
                    name = ((INamespaceSymbol) model.GetDeclaredSymbol(node)).ToString();
                    nsFound = true;
                }

                if (!string.IsNullOrWhiteSpace(name))
                {
                    if (ns.Length > 0)
                    {
                        ns.Insert(0, ".");
                    }

                    ns.Insert(0, name);
                }

                node = node.Parent;
            }

            if (!nsFound)
            {
                throw new InvalidOperationException("No namespace node found in the syntax tree");
            }

            return ns.ToString();
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> DeserializationCandidates { get; } = new List<ClassDeclarationSyntax>();
            
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax classNode &&
                    classNode.AttributeLists.Any())
                {
                    // The syntax alone can't tell us the exact attribute type is attached, it only knows the
                    // string of the attribute syntax.  So since this class may or may not have the correct
                    // generate json deserializer attribute we should track it as a candidate, and verify later
                    // when we have a compilation unit.
                    DeserializationCandidates.Add(classNode);
                }
            }
        }

        private class ClassDetails
        {
            public ITypeSymbol TypeSymbol { get; set; }
            public string Namespace { get; set; }
        }
    }
}