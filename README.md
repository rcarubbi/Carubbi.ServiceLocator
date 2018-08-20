Carubbi.ServiceLocator
==================

## A simple helper to implement Service Locator

Example:
```XML
 <configSections>
    <section name="Implementations" type="System.Configuration.DictionarySectionHandler"/>
 </configSections>
<Implementations>
    <add key="Intetrface" value="Namespace.Class, AssemblyName"/>
    <add key="ConcreteTypeUsedByReflection" value="Namespace.Class, AssemblyName"/>
</Implementations>
```

```csharp
var implementation = ImplementationResolver.Resolve<Interface>();
```
or

```csharp
var implementation = ImplementationResolver.Resolve("ConcreteTypeUsedByReflection");
```
