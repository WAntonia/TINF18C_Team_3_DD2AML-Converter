# Dd2Aml.Lib

Welcome to DD2AML.Lib! 

This project was developed for GSD as a student project by (in alphabetical order)

1. [Nico Dietz](https://github.com/dillasyx)
2. [Steffen Gerdes](https://github.com/SteffenGerdes)
3. [Constantin Ruhdorfer](https://github.com/ConstantinRuhdorfer)
4. [Jonas Komarek](https://github.com/JonasKomarek)
5. [Vu Quang Phuc](https://github.com/VuQuangPhuc)
6. [Michael Weidmann](https://github.com/michaelweidmann)

and was further developed for IODD and CSP+ files by (in alphabetical order)

1. [Nora Baitinger](https://github.com/naboga)
2. [Lara Mack](https://github.com/Sophelec)
3. [Bastiane Storz](https://github.com/Maruny)
4. [Antonia Wermerskirch](https://github.com/WAntonia)

at [Baden-Wuerttemberg Cooperative State University (DHBW) Stuttgart](https://www.dhbw-stuttgart.de/home/) under supervision of [Markus Rentschler](http://wwwlehre.dhbw-stuttgart.de/~rentschler/) and Christian Ewertz.

This project is distributed via:

1. [GitHub](https://github.com/WAntonia/TINF18C_Team_3_DD2AML-Converter)

## About this project

This library converts a Profinet DD-formatted (GSD, IODD or CSP) file to AutomationML.
The library can either
1. return a string containing the AutomatonML content.
2. convert the DD file to an .aml file and package that, including all its dependencies, into an .amlx package. This process uses the [AML.Engine](https://www.nuget.org/packages/Aml.Engine).

## Contributing to this project

Contributions are always welcome!
If you want to contribute feel free to fork this repo and later perform a pull request. 

## File structure

The relevant files and folders are listed here.

### Logging/
Contains the logging service.

### Models/
Contains the classes representing AML, GSD, IODD and CSP.
Also contains the used XSD files.

### Properties/
Contains the assembly info.

### AMLPackager.cs
Contains all the logic that is required to:
1. Create a temporary folder
2. Find files and copy them to this folder
3. Uses the AML.Engine to build the .amlx package

### Converter.cs
Contains all the logic that traverses the DD file and uses the rulesset file to translate the DD file to AML.

### Util.cs
Contains the utility functionality.

### AML2/ and AML3/
These two folders contain the rulesset files.
Please have a look below.

## Conversion Rules

The rules for conversion are written in XML and are listed here.

### Tables of Content

1. [Structure](#structure)
2. [References](#references)
3. [Reference Types](#reference_types)
4. [GUID](#guid)
5. [The Rule Element](#rule_element)

### <a name="structure"></a>Structure

This section will explain the structure of a dd2aml rulesset file.

The dd2aml file will consist of one, and only one, element, the `<Body>`. Each seperate rule shall be direct child of this element.

A rule must start with the element, which is to be replaced. It must have a child `<Replacement>` describing the corresponding XML-structure of the AML replacement.

```xml
<ProfileBody>
    <Replacement>
        <SystemUnitClassLib Name="ComponentSystemUnitClassLib">
            <Version>1.0.0</Version>
        </SystemUnitClassLib>
    </Replacement>
</ProfileBody>
```

Additionally a rule can also have any number of `<Reference>` childs. References will be explained in the following section.

### <a name="references"></a>References

#### <a name="references-normal_references"></a>Normal Reference

It may not be possible to replace a DD element with a static replacements. Let's look at this example:

```xml
<SubslotItem SubslotNumber="32768" TextId="TOK_Subslot_8000" />
```

This element has a attribute "SubslotNumber". In order to transform this element to AML the attribute "SubslotNumber" should be converted into an `Attribute`, `<Value>` pair.

```xml
<ExternalInterface Name="" ID="">
    <Attribute Name="SubslotNumber" AttributeDataType="xs:integer">
        <Value></Value>
    </Attribute>
</ExternalInterface>
```

Unfortunately the converter itself cannot figure out where this information is located. A referece, signaled by a `$` followed by an identifier like `$1`, is necessary.

The conversion rule for "SubslotItem" may look like this:

```xml
<SubslotItem>
    <Replacement>
        <ExternalInterface Name="" ID="">
            <Attribute Name="SubslotNumber" AttriubteDataType="xs:integer">
                <Value>$1<Value>
            </Attribute>
        </ExternalInterface>
    </Replacement>
    <Reference Ref="$1">
        <ISO15745Profile.ProfileBody.ApplicationProcess.DeviceAccessPointList.DeviceAccessPointItem.SlotList.SlotItem SubslotNumber="" />
    </Reference>
</SubslotItem>
```

Every reference within the replacement element shall have a corresponding reference element. This element must have the attribute `Ref=""` with the identifier. There are different [types](#reference_types) of references. These will be explained later. The content of the reference shall be a the location of the referenced value. A reference only has one child. The child is the full qualified path to the referenced element in a point sepearted list without whitespace.

The element shall have one attribute, which shall be the same attribute that is being referenced. If the attribute exists, its value will be the return value.

#### <a name="references-true_references"></a>True Reference

You may have noticed that the example above never resolved the DD attribute TextId of the "SubslotItem" element. This is because the TextId attribute in itself is also a reference within the DD file and as such must be handled differently. To differentiate this case from a normal reference, it shall be named "true reference".

A true reference within an DD file will always reference another element within a "List", that has the attribute `ID` or other identifying attribute like `TextId`.

These references must be handled differently by the DD2AML Converter. The converter will know of the different "reference lists" (such as `ExternalTextList`) and their location within the DD. The converter merely needs to know which list to look in and the corresponding id. Therefore each list will receive its own "type" which can be used as an attribute of the reference element. The other way around, this means that the converter __cannot__ handle true references of lists that the converter does not know.

 Consider the example from above. `TextId=TOK_Subslot_8000` signifies that this attribute references an element within a text list with the id `TOK_Subslot_8000`.

```xml
<SubslotItem>
    <Replacement>
        <ExternalInterface Name="$1" ID="GUID" RefBaseClassPath="physicalEndPoint/SubSlot">
            <Attribute Name="SubslotNumber" AttributeDataType="xs:integer">
                <Value>$2</Value>
            </Attribute>
        </ExternalInterface>
    </Replacement>
    <Reference Ref="$1" Type="TextRef">
        <ISO15745Profile.ProfileBody.ApplicationProcess.DeviceAccessPointList.DeviceAccessPointItem.SlotList.SlotItem TextId="" />
    </Reference>
    <Reference Ref="$2">
        <ISO15745Profile.ProfileBody.ApplicationProcess.DeviceAccessPointList.DeviceAccessPointItem.SlotList.SlotItem SubslotNumber="" />
    </Reference>
</SubslotItem>
```

The type `TextRef` will tell the converter to search under `ExternalTextList/PrimaryLanguage`. The content of a true reference is the location of the corresponding id.

Other languages will be ignored.

### <a name="reference_types"></a>Reference Types

This section will list the different reference types and their uses.

#### <a name="reference_types-Normal-Ref></a>Normal Ref

No type is specified. The refernced value will be used.

#### <a name="reference_types-TextRef"></a>TextRef

A `TextRef` is a true reference. Within the DD it will reference an element within `<ExternalTextList>`. As the name suggest it will only return a single text. It will only over use the `<PrimaryLanguage>`. The location of the reference __id__ used in the GSD will be given as content of the reference.


#### <a name="reference_types-graphicref"></a>GraphicRef

A `GraphicRef` is a true reference. Within the DD it will reference an element within `GraphicsList`. The location of the reference __id__ used in the DD will be given as content of the reference.

#### <a name="special_reference_types"></a>RelGsdFilePath

`RelGsdFilePath` returns a relative path to the original DD file.

### <a name="guid"></a>GUID

Every time the converters reads the string `GUID`, it will be replaced with a real GUID.

### <a name="rule_element"></a> The Rule Element

In order to maintain modularity while also providing a way to define the structure of the result aml in terms of parent-child relationships, a `<Rule></Rule>` element is introduced.

The content of the `<Rule></Rule>` tag shall be a full a string consisting of the relative path from the parent rule element to the GSD element, which should be placed here.

That path shall a rule of its own and a child of the `<Body>` element.

Example:

```xml
<InternalElement CAEXObject.Name="LogicalInterface" CAEXObject.ID="GUID">
    <SystemUnitClassType.InternalElement>
    <Rule>
ProfileBody.ApplicationProcess.DeviceAccessPointList.DeviceAccessPointItem.SystemDefinedSubmoduleList.InterfaceSubmoduleItem
    </Rule>
    </SystemUnitClassType.InternalElement>
</InternalElement>

  <ProfileBody.ApplicationProcess.DeviceAccessPointList.DeviceAccessPointItem.SystemDefinedSubmoduleList.InterfaceSubmoduleItem>
  ...
</ProfileBody.ApplicationProcess.DeviceAccessPointList.DeviceAccessPointItem.SystemDefinedSubmoduleList.InterfaceSubmoduleItem>
```
## IODD2AML Rules
This section will explain the structure of a Iodd2aml rulesset file.

The iodd2aml file will consist of one, and only one, element, the `<Body>`. Each seperate rule shall be direct child of this element.

```xml
  <ProfileBody>
    <Replacement>
      <CAEXFile.SystemUnitClassLib>
        <SystemUnitClassLib CAEXObject.Name="ComponentSystemUnitClassLib">
        </SystemUnitClassLib>
      </CAEXFile.SystemUnitClassLib>
    </Replacement>
  </ProfileBody>
  ```
## CSPP2AML Rules
