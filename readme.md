This demo solves a very specific requirement, I encountered in a "legacy" project.

The source application is tied to a database, that is implementing column-level security and is calculating the field access (fa) to each field invdividually.
This resulted in very large entities that had double the amount of columns that they needed.

`ID | FaID | Name | FaName | Age | FaAge`

The backend in this system is not much more then a SQL to Json converter in this system and just pushes this through to the frontend. In order to make dealing with those entities a bit easier, my goal was to wrap 
each property in a `Field<T>` class, so you don't have to get the name of the actual field and look for another property called `Fa{name}` and get its value to determine the field access.

This source generator now retrieves each class that is marked with the `IGeneratable` interface, for lack of a better name and gets every public property.

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

A potential next step is to create a `From(SourceEntity entity)` method, that will automatically map the source entity coming from the db to the viewmodel.