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

            var entitiesToConvert = syntaxReceiver.EntitiesToConvert;
            if (entitiesToConvert == null) return;

            foreach (var entity in entitiesToConvert)
            {
                if (entity == null) continue;
                //get all properties
                var classProperties = entity
                    .DescendantNodesAndSelf()
                    .OfType<PropertyDeclarationSyntax>()
                    .ToList();

                SourceText sourceText = SourceText.From($@"
               
namespace GeneratorsTest
{{
    public partial class {entity.Identifier}Vm
    {{

        public {entity.Identifier}Vm(){{}}

        public {entity.Identifier}Vm({entity.Identifier} source)
        {{
            {GenerateMappingForPropertyList(classProperties)}
        }}

#nullable enable
        {GenerateListOfFieldsForPropertyList(classProperties)}
#nullable disable
    }}
}}", Encoding.UTF8);
                context.AddSource($"{entity.Identifier}Vm.Generated.cs", sourceText);
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

        public string GenerateMappingForPropertyList(IList<PropertyDeclarationSyntax> properties)
        {
            //Iterate over each public property and create a Field<T> from it.
            var sb = new StringBuilder();
            foreach (var prop in properties)
            {
                var name = prop.Identifier.ToString();
                // it is considered a FieldAccess, if it starts with Fa AND there is a field that has the exact name sans the "Fa"
                var isFieldAccess = name.StartsWith("Fa") && properties.Any(p => p.Identifier.ToString() == name.Substring(2));

                if (isFieldAccess) continue;

                //next check, if the field in question has a corresponding FieldAccess property. if not, it will not be mapped to a field
                var hasFieldAccess = properties.Any(pr => pr.Identifier.ToString() == $"Fa{name}");
                if (!hasFieldAccess) continue;

                var mapping = $"{name} = Field<{prop.Type}>.From(source.{name}, source.Fa{name});{Environment.NewLine}";
                sb.Append(mapping);
            }
            return sb.ToString();
        }
    }

}

