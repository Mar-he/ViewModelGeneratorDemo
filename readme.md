This demo solves a very specific requirement, I encountered in one of my current projects.

A database is handling all business logic, data access logic & authorization logic for a system. One of the results of this is, that by default, each column that is returned to the caller has a corresponding `FieldAccess` (Fa) column. For the sake of this demonstration, let's assume that the Fa columns meaning is not of interest. It could be a bit-level security or just 0 - no access, 1 - read, 2 - write.  
This resulted in very large entities that had double the amount of columns that they needed.

`ID | FaID | Name | FaName | Age | FaAge`

The web api consuming this database is not much more then a SQL to JSON converter. It validates tokens and maps entities to view models.
In order to make dealing with those entities a bit easier, my goal was to wrap 
each property in a `Field<T>` class, so you don't have to get the name of the actual field and look for another property called `Fa{name}` and get its value to determine the field access.

Naming is hard and because my naming skills are somewhat lacking I created a simple marker interface called `IGeneratable`. The demo class `Book`, which serves as a simple entity stub, is decorated with this interface. Using the `ViewModelGeneratorSyntaxReceiver` we now traverse the syntax tree and look for all classes that implement said interface.

Every class found is stored in a collection called `EntitiesToConvert` and subsequently we iterate of each entity in that collection to generate our new ViewModel.

Each public property is then converted in a `Field<T>`. The Columns `ID` and `FaID` are now encapsulated in the property `public Field<int> ID { get; set;}`, with its property `Value` and `FieldAccess`.
With all properties being captured I am now attaching this to the StringBuilder, that contains all the new properties:

```cs
    var newField = $"public Field<{prop.Type}> {name} {{ get; set; }}{Environment.NewLine}";
```

And the results of this are injected into the encapsulating "template":

```cs
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

```

Going forward I can now decorate around 3 dozen entities with the marker interface and automatically generate and update the viewmodels for the frontend, by rebuilding the source generator.

The next step was to provide a mechanism to automatically map a source entity into the newly generated viewmodel.
This is done by creating a constructor that will accept the source class as a parameter:

```cs
public {className.Identifier}Vm(){{}}
public {className.Identifier}Vm({className.Identifier} source)
{{
    {GenerateMappingForPropertyList(classProperties)}
}}
```

Now this is not valid C#. Look at it in the source and you'll see this is an interpolated string.

`GenerateListOfFieldsForPropertyList(IList<PropertyDeclarationSyntax> properties)` looks very similar to `GenerateListOfFieldsForPropertyList`. We look for every property, that is a) not a FieldAccess property and b) has a corresponding FieldAccess property. For each of those properties we leverage the static `Field<T>.From()` method: 

```cs
var mapping = $"{name} = Field<{prop.Type}>.From(source.{name}, source.Fa{name});{Environment.NewLine}";
```
