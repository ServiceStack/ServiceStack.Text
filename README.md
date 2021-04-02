Follow [@ServiceStack](https://twitter.com/servicestack), [view the docs](https://docs.servicestack.net), use [StackOverflow](https://stackoverflow.com/questions/ask?tags=servicestack,servicestack.text) or [Customer Forums](https://forums.servicestack.net/) for support.

## [FREE high-perf Text Serializers](https://docs.servicestack.net/releases/v4.0.62#servicestacktext-is-now-free) and Core Utils powering [servicestack.net](https://servicestack.net)

ServiceStack.Text is an **independent, dependency-free** serialization library containing ServiceStack's core high-performance utils and text processing functionality, including:

 - [JSON](https://github.com/ServiceStack/ServiceStack.Text), 
 [JSV](http://docs.servicestack.net/jsv-format) and 
 [CSV](http://docs.servicestack.net/csv-format) Text Serializers
 - [AutoMapping Utils](http://docs.servicestack.net/auto-mapping)
 - [HTTP Utils](http://docs.servicestack.net/http-utils)
 - [Dump Utils](http://docs.servicestack.net/dump-utils)
 - [Fast Reflection Utils](http://docs.servicestack.net/reflection-utils)
 - Several String Extensions, Collection extensions, Reflection Utils and lots more.

### [Mobile Apps Support](https://github.com/ServiceStackApps/HelloMobile)

### Try out [ServiceStack.Text Live](http://gistlyn.com/text)

A great way to try out ServiceStack.Text is on [gistlyn.com](http://gistlyn.com) which lets you immediately 
run and explore all ServiceStack.Text features from the comfort of your browser with zero software install:

[![](https://raw.githubusercontent.com/ServiceStack/Assets/master/img/livedemos/gistlyn/home-screenshot.png)](http://gistlyn.com/text)

## Simple API

Like most of the interfaces in ServiceStack, the API is simple. Methods that you would commonly use include:

### Convenience Serialization Extension Methods

```csharp
string ToJson(T)
T FromJson()

string ToJsv(T)
T FromJsv()

string ToCsv(T)
T FromCsv()

string ToXml(T)
T FromXml()
```

### Explicit API

#### JSON

```csharp
string JsonSerializer.SerializeToString<T>(T value)
void JsonSerializer.SerializeToWriter<T>(T value, TextWriter writer)

T JsonSerializer.DeserializeFromString<T>(string value)
T JsonSerializer.DeserializeFromReader<T>(TextReader reader)
```

#### JSV

```csharp
string TypeSerializer.SerializeToString<T>(T value)
void TypeSerializer.SerializeToWriter<T>(T value, TextWriter writer)

T TypeSerializer.DeserializeFromString<T>(string value)
T TypeSerializer.DeserializeFromReader<T>(TextReader reader)
```

#### CSV

```csharp
string CsvSerializer.SerializeToString<T>(T value)
void CsvSerializer.SerializeToWriter<T>(T value, TextWriter writer)

T CsvSerializer.DeserializeFromString<T>(string value)
T CsvSerializer.DeserializeFromReader<T>(TextReader reader)
```

Where *T* can be any .NET POCO type. That's all there is - the API was intentionally left simple :)

### Dump Utils

Dump / Diagnostic Extensions:

```csharp
T Dump()
T Print()
T PrintDump()
string Fmt(args)
```

### Dynamic JSON parsing API

```csharp
JsonObject.Parse()
JsonArrayObjects.Parse()
```

### Pretty Print JSON

You can format JSON into a more readable format with the `IndentJson()` extension method, e.g: 

```csharp
var prettyJson = dto.ToJson().IndentJson();
```
	
URL Extensions:

```csharp
string GetStringFromUrl()
string GetJsonFromUrl()
string GetResponseStatus()
string UrlEncode() / UrlDecode()
string HexEscape() / HexUnescape()
string UrlFormat() / AppendPath() / AppendPaths() / WithTrailingSlash()    
string AddQueryParam() / SetQueryParam() AddHashParam() / SetHashParam()
string WithoutExtension() / ParentDirectory() / ReadAllText()
```
    
#### Stream Extensions:

```csharp
Stream WriteTo(Stream) / CopyTo()
Stream ReadLines()
Stream ReadFully() / ReadExactly()
```
    
#### String Utils:

```csharp
string SplitOnFirst() / SplitOnLast()
string IndexOfAny()
string StripHtml() / ToCamelCase()
string SafeSubstring()
string ToUtf8Bytes() / FromUtf8Bytes()
string LeftPart() / LastLeftPart() / RightPart() / LastRightPart()
```
    
more String, Reflection, List, Dictionary, DateTime extensions...    

### Supports Dynamic JSON

Although usually used to (de)serialize C#/.NET POCO types, it also includes a flexible API allowing you to deserialize any 
JSON payload without it's concrete type, see these real-world examples:

  - [Parsing GitHub's v3 API with typed DTOs](https://github.com/ServiceStack/ServiceStack.Text/blob/master/tests/ServiceStack.Text.Tests/UseCases/GithubV3ApiTests.cs)
  - [Parsing GitHub's JSON response](https://github.com/ServiceStack/ServiceStack.Text/blob/master/tests/ServiceStack.Text.Tests/UseCases/GitHubRestTests.cs)
  - [Parsing Google Maps JSON Response](https://github.com/ServiceStack/ServiceStack.Text/blob/master/tests/ServiceStack.Text.Tests/UseCases/GMapDirectionsTests.cs)
  - [Parsing Centroid](https://github.com/ServiceStack/ServiceStack.Text/blob/master/tests/ServiceStack.Text.Tests/UseCases/CentroidTests.cs)

Also a thin **.NET 4.0 Dynamic JSON** wrapper around ServiceStack's JSON library is included in the 
[ServiceStack.Razor](https://github.com/ServiceStack/ServiceStack.Text/blob/master/src/ServiceStack.Text/Pcl.Dynamic.cs) 
project. It provides a dynamic, but more succinct API than the above options.

### JS Utils

ServiceStack.Text APIs for deserializing arbitrary JSON requires specifying the Type to deserialize into. An alternative flexible approach to read any arbitrary JavaScript or JSON data structures is to use the high-performance and memory efficient JSON utils in 
[#Script](https://sharpscript.net/) implementation of JavaScript.

```csharp
JSON.parse("1")       //= int 1 
JSON.parse("1.1")     //= double 1.1
JSON.parse("'a'")     //= string "a"
JSON.parse("{a:1}")   //= new Dictionary<string, object> { {"a", 1 } }
JSON.parse("[{a:1}]") //= new List<object> { new Dictionary<string, object> { { "a", 1 } } }
```

#### Eval

Since JS Utils is an essential part of [#Script](https://sharpscript.net/) it allows for advanced scenarios like implementing a text DSL or scripting language for executing custom logic or business rules you want to be able to change without having to compile or redeploy your App. It uses [#Script Context](https://sharpscript.net/docs/methods) which lets you evaluate the script within a custom scope that defines what functions 
and arguments it has access to, e.g.:

```csharp
public class CustomMethods : ScriptMethods
{
    public string reverse(string text) => new string(text.Reverse().ToArray());
}

var scope = JS.CreateScope(
         args: new Dictionary<string, object> { { "arg", "value"} }, 
    functions: new CustomMethods());

JS.eval("arg", scope)                                        //= "value"
JS.eval("reverse(arg)", scope)                               //= "eulav"
JS.eval("3.itemsOf(arg.reverse().padRight(8, '_'))", scope) //= ["eulav___", "eulav___", "eulav___"]

//= { a: ["eulav___", "eulav___", "eulav___"] }
JS.eval("{a: 3.itemsOf(arg.reverse().padRight(8, '_')) }", scope)
```

ServiceStack's JS Utils is available in the [ServiceStack.Common](https://www.nuget.org/packages/ServiceStack.Common) NuGet package.

## Install ServiceStack.Text

    PM> Install-Package ServiceStack.Text

> From v4.0.62+ [ServiceStack.Text is now free!](https://docs.servicestack.net/releases/v4.0.62#servicestacktext-is-now-free)

## Copying

Since September 2013, ServiceStack source code is available under GNU Affero General Public License/FOSS License Exception, see license.txt in the source. Alternative commercial licensing is also available, contact team@servicestack.net for details.

## Contributing

Contributors need to approve the [Contributor License Agreement](https://docs.google.com/forms/d/16Op0fmKaqYtxGL4sg7w_g-cXXyCoWjzppgkuqzOeKyk/viewform) before any code will be reviewed, see the [Contributing wiki](https://github.com/ServiceStack/ServiceStack/wiki/Contributing) for more details. 

## ServiceStack.JsonSerializer

For reasons outlined [in this blog post](https://github.com/ServiceStackV3/mythz_blog/blob/master/pages/344.md) I decided to re-use *TypeSerializer's* text processing-core to create ServiceStack.JsonSerializer - the fastest JSON Serializer for .NET.
Based on the [Northwind Benchmarks](http://mono.servicestack.net/benchmarks/) it's *3.6x* faster than .NET's BCL JsonDataContractSerializer and *3x* faster than the previous fastest JSON serializer benchmarked - [JSON.NET](http://json.codeplex.com/).

A comprehensive set of other .NET benchmarks are maintained at [servicestack.net/benchmarks](http://mono.servicestack.net/benchmarks/) and [in the wiki](https://github.com/ServiceStack/ServiceStack/wiki/Real-world-performance).

## ServiceStack.CsvSerializer
As CSV is an important format in many data access and migration scenarios, it became [the latest format included in ServiceStack](https://github.com/ServiceStack/ServiceStack/wiki/CSV-Format) which allows all your existing web services to take advantage of the new format without config or code-changes. As its built using the same tech that makes the JSON and JSV serializers so fast, we expect it to be the fastest POCO CSV Serializer for .NET.

## ServiceStack.TypeSerializer and the JSV-format
Included in this project is `TypeSerializer` - A fast and compact text-based serializer for .NET. It's a light-weight compact Text Serializer which can be used to serialize .NET data types inc custom POCO's and DataContract's. More info on its JSV Format can be found on the [introductory post](https://github.com/ServiceStackV3/mythz_blog/blob/master/pages/176.md).

## T.Dump() Extension method
Another useful library to have in your .NET toolbox is the [T.Dump() Extension Method](https://github.com/ServiceStackV3/mythz_blog/blob/master/pages/202.md). Under the hood it uses a *Pretty Print* Output of the JSV Format to recursively dump the contents of any .NET object. Example usage and output: 

```csharp
var model = new TestModel();
model.PrintDump();

//Example Output
{
    Int: 1,
    String: One,
    DateTime: 2010-04-11,
    Guid: c050437f6fcd46be9b2d0806a0860b3e,
    EmptyIntList: [],
    IntList:
    [
        1,
        2,
        3
    ],
    StringList:
    [
        one,
        two,
        three
    ],
    StringIntMap:
    {
        a: 1,
        b: 2,
        c: 3
    }
}
```

# ServiceStack's JsonSerializer

ServiceStack's JsonSerializer is optimized for serializing C# POCO types in and out of JSON as fast, compact and cleanly as possible. In most cases C# objects serializes as you would expect them to without added json extensions or serializer-specific artefacts.

JsonSerializer provides a simple API that allows you to serialize any .NET generic or runtime type into a string, TextWriter/TextReader or Stream.

### Serialization API

```csharp
string SerializeToString<T>(T)
void SerializeToWriter<T>(T, TextWriter)
void SerializeToStream<T>(T, Stream)
string SerializeToString(object, Type)
void SerializeToWriter(object, Type, TextWriter)
void SerializeToStream(object, Type, Stream)
```

### Deserialization API

```csharp
T DeserializeFromString<T>(string)
T DeserializeFromReader<T>(TextReader)
object DeserializeFromString(string, Type)
object DeserializeFromReader(reader, Type)
object DeserializeFromStream(Type, Stream)
T DeserializeFromStream<T>(Stream)
```

### Extension methods

```csharp
string ToJson<T>(this T)
T FromJson<T>(this string)
```

Convenient **ToJson/FromJson** extension methods are also included reducing the amount of code required, e.g:

```csharp
new []{ 1, 2, 3 }.ToJson()   //= [1,2,3]
"[1,2,3]".FromJson<int[]>()  //= int []{ 1, 2, 3 }
```

## JSON Format 

JSON is a lightweight text serialization format with a spec that's so simple that it fits on one page: [https://www.json.org](https://www.json.org).

The only valid values in JSON are:

  * string
  * number
  * object
  * array
  * true
  * false
  * null

Where most allowed values are scalar and the only complex types available are objects and arrays. Although limited, the above set of types make a good fit and can express most programming data structures.

### number, true, false types

All C# boolean and numeric data types are stored as-is without quotes.

### null type

For the most compact output null values are omitted from the serialized by default. If you want to include null values set the global configuration:

```csharp
JsConfig.Init(new Config { IncludeNullValues = true });
```

### string type

All other scalar values are stored as strings that are surrounded with double quotes.

### C# Structs and Value Types

Because a C# struct is a value type whose public properties are normally just convenience properties around a single scalar value, they are ignored instead the **TStruct.ToString()** method is used to serialize and either the **static TStruct.Parse()** method or **new TStruct(string)** constructor will be used to deserialize the value type if it exists.

### array type

Any List, Queue, Stack, Array, Collection, Enumerables including custom enumerable types are stored in exactly the same way as a JavaScript array literal, i.e:

    [1,2,3,4,5]

All elements in an array must be of the same type. If a custom type is both an IEnumerable and has properties it will be treated as an array and the extra properties will be ignored.

### object type

The JSON object type is the most flexible and is how most complex .NET types are serialized. The JSON object type is a key-value pair JavaScript object literal where the key is always a double-quoted string.

Any IDictionary is serialized into a standard JSON object, i.e:

    {"A":1,"B":2,"C":3,"D":4}

Which happens to be the same as C# POCO types (inc. Interfaces) with the values:

`new MyClass { A=1, B=2, C=3, D=4 }`

    {"A":1,"B":2,"C":3,"D":4}

Only public properties on reference types are serialized with the C# Property Name used for object key and the Property Value as the value. At the moment it is not possible to customize the Property Name.

JsonSerializer also supports serialization of anonymous types in much the same way:

`new { A=1, B=2, C=3, D=4 }`

    {"A":1,"B":2,"C":3,"D":4}

### Parsing JSON Dates

The default WCF Date that's returned in ServiceStack.Text can be converted with:

```js
function todate (s) { 
    return new Date(parseFloat(/Date\(([^)]+)\)/.exec(s)[1])); 
};
```

Which if you're using the [servicestack-client](https://docs.servicestack.net/servicestack-client-umd) npm package can be resolved with:

```ts
import { todate } from "servicestack-client";
var date = todate(wcfDateString);
```

Or if using [ss-utils.js](https://docs.servicestack.net/ss-utils-js) that's built into ServiceStack:

```js
var date = $.ss.todate(wcfDateString);
```

If you change ServiceStack.Text default serialization of Date to either use the ISO8601 date format:

```csharp
JsConfig.DateHandler = DateHandler.ISO8601;
```

It can be parsed natively with:

```js
new Date(dateString)
```

Likewise when configured to return:

```csharp
JsConfig.DateHandler = DateHandler.UnixTimeMs;
```

It can also be converted natively with:

```js
new Date(unixTimeMs)
```

## Global Default JSON Configuration

The JSON/JSV and CSV serialization can be customized globally by configuring the `JsConfig` or type-specific `JsConfig<T>` static classes with your preferred defaults. Global static configuration can be configured once on **Startup** using `JsConfig.Init()`, e.g:

```csharp
JsConfig.Init(new Config {
    DateHandler = DateHandler.ISO8601,
    AlwaysUseUtc = true,
    TextCase = TextCase.CamelCase,
    ExcludeDefaultValues = true,                
});
```

The following is a list of `bool` options you can use to configure many popular preferences:

<table>
    <thead>
        <tr><th>Name</th><th>Alias</th></tr>
    </thead>
    <tr><td>IncludeNullValues</td><td>inv</td></tr>
    <tr><td>IncludeNullValuesInDictionaries</td><td>invid</td></tr>
    <tr><td>IncludeDefaultEnums</td><td>ide</td></tr>
    <tr><td>IncludePublicFields</td><td>ipf</td></tr>
    <tr><td>IncludeTypeInfo</td><td>iti</td></tr>
    <tr><td>ExcludeTypeInfo</td><td>eti</td></tr>
    <tr><td>ExcludeDefaultValues</td><td>edv</td></tr>
    <tr><td>ConvertObjectTypesIntoStringDictionary</td><td>cotisd</td></tr>
    <tr><td>TreatEnumAsInteger</td><td>teai</td></tr>
    <tr><td>TryToParsePrimitiveTypeValues</td><td>ttpptv</td></tr>
    <tr><td>TryToParseNumericType</td><td>ttpnt</td></tr>
    <tr><td>ThrowOnDeserializationError</td><td>tode</td></tr>
    <tr><td>EscapeUnicode</td><td>eu</td></tr>
    <tr><td>EscapeHtmlChars</td><td>ehc</td></tr>
    <tr><td>PreferInterfaces</td><td>pi</td></tr>
    <tr><td>SkipDateTimeConversion</td><td>sdtc</td></tr>
    <tr><td>AlwaysUseUtc</td><td>auu</td></tr>
    <tr><td>AssumeUtc</td><td>au</td></tr>
    <tr><td>AppendUtcOffset</td><td>auo</td></tr>
    <tr><td>EscapeHtmlChars</td><td>ehc</td></tr>
    <tr><td>EscapeUnicode</td><td>eu</td></tr>
    <tr><td>EmitCamelCaseNames</td><td>eccn</td></tr>
    <tr><td>EmitLowercaseUnderscoreNames</td><td>elun</td></tr>
</table>

### DateHandler (dh)

<table>
    <tr><td>TimestampOffset</td><td>to</td></tr>
    <tr><td>DCJSCompatible</td><td>dcjsc</td></tr>
    <tr><td>ISO8601</td><td>iso8601</td></tr>
    <tr><td>ISO8601DateOnly</td><td>iso8601do</td></tr>
    <tr><td>ISO8601DateTime</td><td>iso8601dt</td></tr>
    <tr><td>RFC1123</td><td>rfc1123</td></tr>
    <tr><td>UnixTime</td><td>ut</td></tr>
    <tr><td>UnixTimeMs</td><td>utm</td></tr>
</table>

### TimeSpanHandler (tsh)

<table>
    <tr><td>DurationFormat</td><td>df</td></tr>
    <tr><td>StandardFormat</td><td>sf</td></tr>
</table>

### TextCase (tc)

<table>
    <tr><td>Default</td><td>d</td></tr>
    <tr><td>PascalCase</td><td>pc</td></tr>
    <tr><td>CamelCase</td><td>cc</td></tr>
    <tr><td>SnakeCase</td><td>sc</td></tr>
</table>

### PropertyConvention (pc)

<table>
    <tr><td>Strict</td><td>s</td></tr>
    <tr><td>Lenient</td><td>l</td></tr>
</table>

### Custom Config Scopes

If you need to use different serialization settings from the global static defaults you can use `JsConfig.With()` to create a scoped configuration
using property initializers:

```csharp
using (JsConfig.With(new Config { 
    TextCase == TextCase.CamelCase, 
    PropertyConvention = PropertyConvention.Lenient
}))
{
    return text.FromJson<T>();
}
```

#### Create Custom Scopes using String config

You can also create a custom config scope from a string manually using `JsConfig.CreateScope()` where you can use the full config name or their aliases, e.g:

```csharp
using (JsConfig.CreateScope("IncludeNullValues,EDV,dh:ut")) 
{
    var json = dto.ToJson();
}
```

This feature is used to provide a number of different [JSON customizations in ServiceStack Services](http://docs.servicestack.net/customize-json-responses).

### Type Configuration

If you can't change the definition of a Type (e.g. because its in the BCL), you can specify a custom serialization /
deserialization routine to use instead. E.g. here's how you can add support for `System.Drawing.Color` and customize how `Guid` and `TimeSpan` Types are serialized:

```csharp
JsConfig<System.Drawing.Color>.SerializeFn = c => c.ToString().Replace("Color ","").Replace("[","").Replace("]","");
JsConfig<System.Drawing.Color>.DeSerializeFn = System.Drawing.Color.FromName;

JsConfig<Guid>.SerializeFn = guid => guid.ToString("D");
JsConfig<TimeSpan>.SerializeFn = time => 
    (time.Ticks < 0 ? "-" : "") + time.ToString("hh':'mm':'ss'.'fffffff");
```

## Strict Parsing

By default ServiceStack Serializers will try to deserialize as much as possible without error, if you prefer you can opt-in to stricter parsing with:

```csharp
Env.StrictMode = true;
```

Where it will instead fail fast and throw Exceptions on deserialization errors.

## Custom Serialization

Although JsonSerializer is optimized for serializing .NET POCO types, it still provides some options to change the convention-based serialization routine.

### Using Structs to Customize JSON

This makes it possible to customize the serialization routine and provide an even more compact wire format. 

E.g. Instead of using a JSON object to represent a point 

    { Width=20, Height=10 }
	
You could use a struct and reduce it to just: 

    "20x10" 

By overriding **ToString()** and providing a static **Size ParseJson()** method:

```csharp
public struct Size
{
    public double Width { get; set; }
    public double Height { get; set; }

    public override string ToString()
    {
        return Width + "x" + Height;
    }

    public static Size ParseJson(string json)
    {
        var size = json.Split('x');
        return new Size { 
            Width = double.Parse(size[0]), 
            Height = double.Parse(size[1]) 
        };
    }
}
```

Which would change it to the more compact JSON output:

```csharp
    new Size { Width = 20, Height = 10 }.ToJson() // = "20x10"
```

That allows you to deserialize it back in the same way:

```csharp
    var size = "20x10".FromJson<Size>(); 
```

### Using Custom IEnumerable class to serialize a JSON array

In addition to using a Struct you can optionally use a custom C# IEnumerable type to provide a strong-typed wrapper around a JSON array:

```csharp
public class Point : IEnumerable
{
    double[] points = new double[2];

    public double X 
    {
        get { return points[0]; }
        set { points[0] = value; }
    }

    public double Y
    {
        get { return points[1]; }
        set { points[1] = value; }
    }

    public IEnumerator GetEnumerator()
    {
        foreach (var point in points) 
            yield return point;
    }
}
```

Which serializes the Point into a compact JSON array:

```csharp
    new Point { X = 1, Y = 2 }.ToJson() // = [1,2]
```

## Custom Deserialization

Because the same wire format shared between Dictionaries, POCOs and anonymous types, in most cases what you serialize with one type can be deserialized with another, i.e. an Anonymous type can be deserialized back into a Dictionary<string,string> which can be deserialized into a strong-typed POCO and vice-versa.

Although the JSON Serializer is best optimized for serializing and deserializing .NET types, it's flexible enough to consume 3rd party JSON apis although this generally requires custom de-serialization to convert it into an idiomatic .NET type.

[GitHubRestTests.cs](https://github.com/ServiceStack/ServiceStack.Text/blob/master/tests/ServiceStack.Text.Tests/UseCases/GitHubRestTests.cs)

  1. Using [JsonObject](https://github.com/ServiceStack/ServiceStack.Text/blob/master/src/ServiceStack.Text/JsonObject.cs)
  2. Using Generic .NET Collection classes
  3. Using Customized DTO's in the shape of the 3rd party JSON response

[CentroidTests](https://github.com/ServiceStack/ServiceStack.Text/blob/master/tests/ServiceStack.Text.Tests/UseCases/CentroidTests.cs) is another example that uses the JsonObject to parse a complex custom JSON response. 

## Late-bound Object and Interface Runtime Types

In order to be able to deserialize late-bound objects like `object`, `interface` properties or `abstract` classes ServiceStack needs to emit type information
in the JSON payload. By default it uses `__type` property name, but can be customized with:

```csharp
JsConfig.TypeAttr = "$type";
```

You can also configure what Type Information is emitted with:

```csharp
JsConfig.TypeWriter = type => type.Name;
```

Which will just emit the name of the Type (e.g `Dog`) instead of the full Type Name.

By default ServiceStack will scan all loaded Assemblies to find the Type, but you can tell it to use your own Type Resolver implementation by overriding `TypeFinder`, e.g:

```csharp
JsConfig.TypeFinder = typeInfo =>  =>
{
    var regex = new Regex(@"^(?<type>[^:]+):#(?<namespace>.*)$");
    var match = regex.Match(value);
    var typeName = string.Format("{0}.{1}", match.Groups["namespace"].Value, match.Groups["type"].Value.Replace(".", "+"));
    return MyFindType(typeName);
};
```

### Runtime Type Whitelist

ServiceStack only allows you to serialize "known safe Types" in late-bound properties which uses a whitelist that's pre-populated with a safe-list of 
popular Data Types, DTOs and Request DTOs with the default configuration below:

```csharp
// Allow deserializing types with [DataContract] or [RuntimeSerializable] attributes
JsConfig.AllowRuntimeTypeWithAttributesNamed = new HashSet<string>
{
    nameof(DataContractAttribute),
    nameof(RuntimeSerializableAttribute), // new in ServiceStack.Text
};
 
// Allow deserializing types implementing any of the interfaces below
JsConfig.AllowRuntimeTypeWithInterfacesNamed = new HashSet<string>
{
    "IConvertible",
    "ISerializable",
    "IRuntimeSerializable", // new in ServiceStack.Text
    "IMeta",
    "IReturn`1",
    "IReturnVoid",
};
 
// Allow object property in ServiceStack.Messaging MQ classes
JsConfig.AllowRuntimeTypeInTypesWithNamespaces = new HashSet<string>
{
    "ServiceStack.Messaging",
};
```

The above rules can be extended to allow your own conventions. If you just need to allow a specific Type you can instead just implement:

```csharp
JsConfig.AllowRuntimeType = type => type == typeof(MyType);
```

If you’re in a trusted intranet environment this can also be used to disable the whitelist completely by allowing all Types to be deserialized into object properties with:

```csharp
JsConfig.AllowRuntimeType = _ => true;
```

### Custom Enum Serialization

You can use `[EnumMember]` to change what Enum value gets serialized, e.g:

```csharp
[DataContract]
public enum Day
{
    [EnumMember(Value = "MON")]
    Monday,
    [EnumMember(Value = "TUE")]
    Tuesday,
    [EnumMember(Value = "WED")]
    Wednesday,
    [EnumMember(Value = "THU")]
    Thursday,
    [EnumMember(Value = "FRI")]
    Friday,
    [EnumMember(Value = "SAT")]
    Saturday,
    [EnumMember(Value = "SUN")]
    Sunday,            
}

class EnumMemberDto
{
    public Day Day { get; set; }
}

var dto = new EnumMemberDto {Day = Day.Sunday};
var json = dto.ToJson();  //= {"Day":"SUN"}

var fromDto = json.FromJson<EnumMemberDto>();
fromDto.Day               //= Day.Sunday
```

## TypeSerializer Details (JSV Format)

Out of the box .NET provides a fairly quick but verbose Xml DataContractSerializer or a slightly more compact but slower JsonDataContractSerializer. 
Both of these options are fragile and likely to break with any significant schema changes. 
TypeSerializer addresses these shortcomings by being both smaller and significantly faster than the most popular options. 
It's also more resilient, e.g. a strongly-typed POCO object can be deserialized back into a loosely-typed string Dictionary and vice-versa.

With that in mind, TypeSerializer's main features are:

 - Fastest and most compact text-serializer for .NET
 - Human readable and writeable, self-describing text format
 - Non-invasive and configuration-free
 - Resilient to schema changes
 - Serializes / De-serializes any .NET data type (by convention)
   + Supports custom, compact serialization of structs by overriding `ToString()` and `static T Parse(string)` methods
   + Can serialize inherited, interface or 'late-bound objects' data types
   + Respects opt-in DataMember custom serialization for DataContract dto types.

These characteristics make it ideal for use anywhere you need to store or transport .NET data-types, e.g. for text blobs in a ORM, data in and out of a key-value store or as the text-protocol in .NET to .NET web services.  
 
As such, it's utilized within ServiceStack's other components:
 - OrmLite - to store complex types on table models as text blobs in a database field and
 - [ServiceStack.Redis](https://github.com/ServiceStack/ServiceStack.Redis) - to store rich POCO data types into the very fast [redis](http://redis.io) instances.

You may also be interested in the very useful [T.Dump() extension method](https://github.com/ServiceStackV3/mythz_blog/blob/master/pages/202.md) for recursively viewing the contents of any C# POCO Type.

---

# Performance
Type Serializer is actually the fastest and most compact *text serializer* available for .NET. 
Out of all the serializers benchmarked, it is the only one to remain competitive with [protobuf-net's](http://code.google.com/p/protobuf-net/) very fast implementation of [Protocol Buffers](http://code.google.com/apis/protocolbuffers/) - google's high-speed binary protocol.

Below is a series of benchmarks serialize the different tables in the [Northwind database](https://github.com/ServiceStackV3/ServiceStack.Benchmarks/blob/master/tests/ServiceStack.Northwind.Tests/Support/NorthwindData.cs) (3202 records) with the most popular serializers available for .NET:

### Combined results for serializing / deserialzing a single row of each table in the Northwind database 1,000,000 times
_[view the detailed benchmarks](http://mono.servicestack.net/benchmarks/)_

<table>
<thead>
<tr>
<th>Serializer</th>
<th>Size</th>
<th>Peformance</th>
</tr>
</thead>
<tbody>
<tr><td>Microsoft DataContractSerializer</td><td>4.68x</td><td>6.72x</td></tr>
<tr><td>Microsoft JsonDataContractSerializer</td><td>2.24x</td><td>10.18x</td></tr>
<tr><td>Microsoft BinaryFormatter</td><td>5.62x</td><td>9.06x</td></tr>
<tr><td>NewtonSoft.Json</td><td>2.30x</td><td>8.15x</td></tr>
<tr><td>ProtoBuf.net</td><td>1x</td><td>1x</td></tr>
<tr><td>ServiceStack TypeSerializer</td><td>1.78x</td><td>1.92x</td></tr>
</tbody>
</table>

_number of times larger in size and slower in performance than the best - lower is better_

Microsoft's JavaScriptSerializer was also benchmarked but excluded as it was up to 280x times slower - basically don't use it, ever. 


# JSV Text Format (JSON + CSV)

Type Serializer uses a hybrid CSV-style escaping + JavaScript-like text-based format that is optimized for both size and speed. I'm naming this JSV-format (i.e. JSON + CSV) 

In many ways it is similar to JavaScript, e.g. any List, Array, Collection of ints, longs, etc are stored in exactly the same way, i.e:

    [1,2,3,4,5]

Any IDictionary is serialized like JavaScript, i.e:

    {A:1,B:2,C:3,D:4}

Which also happens to be the same as C# POCO class with the values 

`new MyClass { A=1, B=2, C=3, D=4 }`

    {A:1,B:2,C:3,D:4}

JSV is *white-space significant*, which means normal string values can be serialized without quotes, e.g: 

`new MyClass { Foo="Bar", Greet="Hello World!"}` is serialized as:

    {Foo:Bar,Greet:Hello World!}


### CSV escaping

Any string with any of the following characters: `[]{},"`
is escaped using CSV-style escaping where the value is wrapped in double quotes, e.g:

`new MyClass { Name = "Me, Junior" }` is serialized as:

    {Name:"Me, Junior"}

A value with a double quote is escaped with another double quote e.g:

`new MyClass { Size = "2\" x 1\"" }` is serialized as:

    {Size:"2"" x 1"""}


## Rich support for resilience and schema versioning
To better illustrate the resilience of `TypeSerializer` and the JSV Format check out a real world example of it when it's used to [Painlessly migrate between old and new types in Redis](https://github.com/ServiceStack/ServiceStack.Redis/wiki/MigrationsUsingSchemalessNoSql). 

Support for dynamic payloads and late-bound objects is explained in the post [Versatility of JSV Late-bound objects](http://www.servicestack.net/mythz_blog/?p=314).


# Community Resources

  - [ServiceStack.Text has nice extension method called Dump and has a few friends - web archive](https://web.archive.org/web/20150318193203/http://blogs.lessthandot.com/index.php/desktopdev/mstech/servicestack-text-has-a-nice/) by [@chrissie1](https://twitter.com/chrissie1)
  - [JSON.NET vs ServiceStack - web archive](https://web.archive.org/web/20140721081359/http://daniel.wertheim.se/2011/02/07/json-net-vs-servicestack/)
  - [GithubSharp with ServiceStack.Text](https://github.com/xamarin/GithubSharp/tree/servicestack) by [@XTZGZoReX](https://twitter.com/XTZGZoReX)
