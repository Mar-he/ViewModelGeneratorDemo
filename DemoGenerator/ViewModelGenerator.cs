using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DemoGenerator
{
    [Generator]
    public class ViewModelGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            var mainMethod = context.Compilation.GetEntryPoint(context.CancellationToken);

            //this way of retrieving the namespace does not work. it will output <general namespace>
            //todo: find out way to get namespace
            var generatedNamespace = mainMethod.ContainingNamespace.ToDisplayString();

            var syntaxReceiver = (ViewModelGeneratorSyntaxReceiver)context.SyntaxReceiver;

            var classesMarkedForGeneration = syntaxReceiver.ClassesToAugment;
            if (classesMarkedForGeneration == null) return;

            foreach (var className in classesMarkedForGeneration)
            {
                if (className == null) continue;
                //get all properties
                var classProperties = className.DescendantNodesAndSelf().OfType<PropertyDeclarationSyntax>();

                SourceText sourceText = SourceText.From($@"
               
namespace GeneratorsTest
{{
    public partial class {className.Identifier}Vm
    {{
#nullable enable
        {GenerateListOfFieldsForPropertyList(classProperties.ToList())}
#nullable disable
    }}
}}", Encoding.UTF8);
                context.AddSource($"{className.Identifier}Vm.Generated.cs", sourceText);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            //uncomment this if you need to debug. Rebuild the console to trigger the debug.
            //warning: if you start debugging in another instance of Visual Studio it might trigger a crash-loop, which I haven't
            //been able to figure out on how to solve.
// #if DEBUG
//            if (!Debugger.IsAttached)
//            {
//                Debugger.Launch();
//            }
// #endif 
            context.RegisterForSyntaxNotifications(() => new ViewModelGeneratorSyntaxReceiver());
        }


        public string GenerateListOfFieldsForPropertyList(IList<PropertyDeclarationSyntax> properties)
        {
            //Iterate over each public property and create a Field<T> from it.
            var sb = new StringBuilder();
            foreach (var prop in properties)
            {
                var name = prop.Identifier.ToString();
                // it is considered a FieldAccess, if it starts with Fa AND there is a field that has the exact name sans the "Fa"
                var isFieldAccess = name.StartsWith("Fa") && properties.Any(p => p.Identifier.ToString() == name.Substring(2));

                if (isFieldAccess) continue;

                var newField = $"public Field<{prop.Type}> {name} {{ get; set; }}{Environment.NewLine}";
                sb.Append(newField);
            }
            return sb.ToString();
        }
    }

}

