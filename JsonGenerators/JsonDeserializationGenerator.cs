using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
            var classesToDeserialize = new List<ClassDeclarationSyntax>();
            foreach (var candidate in candidates)
            {
                var typeSymbol = (ITypeSymbol) context.Compilation.GetSemanticModel(candidate.SyntaxTree).GetDeclaredSymbol(candidate);
                foreach (var attribute in typeSymbol.GetAttributes())
                {
                    if (attribute.AttributeClass.Name == nameof(GenerateJsonDeserializerAttribute))
                    {
                        classesToDeserialize.Add(candidate);
                    }
                }
            }
            
            
        }

        private class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> DeserializationCandidates { get; } = new List<ClassDeclarationSyntax>();
            
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax classNode)
                {
                    DeserializationCandidates.Add(classNode);
                }
            }
        }
    }
}