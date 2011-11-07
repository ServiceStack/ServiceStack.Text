[Join the new google group](http://groups.google.com/group/servicestack) or
follow [@demisbellot](http://twitter.com/demisbellot) and [@ServiceStack](http://twitter.com/servicestack)
for twitter updates.

# The Home of [.NET's fastest JSON](http://www.servicestack.net/mythz_blog/?p=344), [JSV](http://www.servicestack.net/mythz_blog/?p=176) and CSV Text Serializers.

ServiceStack.Text is an independent, dependency-free assembly that contains all of ServiceStack's text processing functionality, including:

* [JsonSerializer](http://www.servicestack.net/mythz_blog/?p=344)
* [TypeSerializer (JSV-Format)](https://github.com/ServiceStack/ServiceStack.Text/wiki/JSV-Format)
* CsvSerializer
* [T.Dump extension method](http://www.servicestack.net/mythz_blog/?p=202)
* StringExtensions - Xml/Json/Csv/Url encoding, BaseConvert, Rot13, Hex escape, etc.
* Stream, Reflection, List, DateTime, etc extensions and utils

# Simple API

Like most of the interfaces in Service Stack, the API is simple and descriptive. In most cases these are the only methods that you would commonly use:

	string TypeSerializer.SerializeToString<T>(T value);
	void TypeSerializer.SerializeToWriter<T>(T value, TextWriter writer);

	T TypeSerializer.DeserializeFromString<T>(string value);
	T TypeSerializer.DeserializeFromReader<T>(TextReader reader);

Where *T* can be any .NET POCO type. That's all there is - the API was intentionally left simple :)

### Parses Dynamic Payloads as well:

Although usually used to deserialize C#/.NET POCO types, it also includes a flexible API allowing you to deserialize any 
JSON payload without it's concrete type, see these real-world examples:

  - [Parsing GitHub's JSON response][2]
  - [Parsing Google Maps JSON Response][3]
  - [Parsing Centroid][4] 
   

## NuGet ServiceStack.Text

![Install-Pacakage ServiceStack.Text](http://servicestack.net/img/nuget-servicestack.text.png)


## ServiceStack.JsonSerializer - the fastest JSON Serializer for .NET
For reasons outlined [in this blog post](http://www.servicestack.net/mythz_blog/?p=344) I decided to re-use *TypeSerializer's* text processing-core to create ServiceStack.JsonSerializer - the fastest JSON Serializer for .NET.
Based on the [Northwind Benchmarks](http://www.servicestack.net/benchmarks/NorthwindDatabaseRowsSerialization.100000-times.2010-08-17.html) it's *3.6x* faster than .NET's BCL JsonDataContractSerializer and *3x* faster then the previous fastest JSON serializer benchmarked - [JSON.NET](http://json.codeplex.com/).

## ServiceStack.CsvSerializer
As CSV is an important format in many data access and migration scenarios, it became [the latest format included in ServiceStack](https://github.com/ServiceStack/ServiceStack/wiki/ServiceStack-CSV-Format) which allows all your existing web services to take advantage of the new format without config or code-changes. As its built using the same tech that makes the JSON and JSV serializers so fast, we expect it to be the fastest POCO CSV Serializer for .NET.

## ServiceStack.TypeSerializer and the JSV-format
Included in this project is `TypeSerializer` - The fastest and most compact text-based serializer for .NET. It's a light-weight compact Text Serializer which can be used to serialize any .NET data type including your own custom POCO's and DataContract's. More info on its JSV Format can be found on the [introductory post](http://www.servicestack.net/mythz_blog/?p=176).

## T.Dump() Extension method
Another useful library to have in your .NET toolbox is the [T.Dump() Extension Method](http://www.servicestack.net/mythz_blog/?p=202). Under the hood it uses a *Pretty Print* Output of the JSV Format to recursively dump the contents of any .NET object. Example usage and output: 

	var model = new TestModel();
	Console.WriteLine(model.Dump());

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


# Download
### ServiceStack.Text is included with [ServiceStack.zip](https://github.com/ServiceStack/ServiceStack/downloads) 
### or available to download separately in a standalone [ServiceStack.Text.zip](https://github.com/ServiceStack/ServiceStack.Text/downloads). 

<hr />

#TypeSerializer Details

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
 - [ServiceStack.Redis](https://github.com/ServiceStack/ServiceStack.Redis) - to store rich POCO data types into the very fast [redis](http://code.google.com/p/redis) instances.

You may also be interested in the very useful [T.Dump() extension method](http://www.servicestack.net/mythz_blog/?p=202) for recursively viewing the contents of any C# POCO Type.

---

# Performance
Type Serializer is actually the fastest and most compact *text serializer* available for .NET. 
Out of all the serializers benchmarked, it is the only one to remain competitive with [protobuf-net's](http://code.google.com/p/protobuf-net/) very fast implementation of [Protocol Buffers](http://code.google.com/apis/protocolbuffers/) - google's high-speed binary protocol.

Below is a series of benchmarks serialize the different tables in the [Northwind database](http://code.google.com/p/servicestack/source/browse/trunk/Common/Northwind.Benchmarks/Northwind.Common/DataModel/NorthwindData.cs) (3202 records) with the most popular serializers available for .NET:

### Combined results for serializing / deserialzing a single row of each table in the Northwind database 1,000,000 times
_[view the detailed benchmarks](http://www.servicestack.net/benchmarks/NorthwindDatabaseRowsSerialization.1000000-times.2010-02-06.html)_

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

A value with a double-quote is escaped with another double quote e.g:

`new MyClass { Size = "2\" x 1\"" }` is serialized as:

	{Size:"2"" x 1"""}


## Rich support for resilience and schema versioning
To better illustrate the resilience of `TypeSerializer` and the JSV Format check out a real world example of it when it's used to [Painlessly migrate between old and new types in Redis](https://github.com/ServiceStack/ServiceStack.Redis/wiki/MigrationsUsingSchemalessNoSql). 

Support for dynamic payloads and late-bound objects is explained in the post [Versatility of JSV Late-bound objects](http://www.servicestack.net/mythz_blog/?p=314).
