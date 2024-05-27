namespace BuilderService.Attributes;

/// <summary>
/// Used to map a given property to a named field in the document.
/// </summary>
/// <param name="name"></param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public class FieldAttribute(string name) : Attribute
{
    /// <summary>
    /// The name of the field to map to in the document.
    /// </summary>
    public string Name { get; private set; } = name;
}