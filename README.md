The fastest and most compact text-based serializer for .NET - 
more info on its JSV Format is on the [introductory blog post](http://www.servicestack.net/mythz_blog/?p=176).


# Fast new text-serialization format optimized for serializing C# POCO types

Included in the Service Stack libraries is *Type Serializer*, a fast, light-weight compact Text Serializer which can be used to serialize any .NET data type including your own custom POCO's and DataContract's.

Out of the box .NET provides a fairly quick but verbose Xml DataContractSerializer or a slightly more compact but slower JsonDataContractSerializer. 
Both of these options are fragile and likely to break with any significant schema changes. 
TypeSerializer addresses these shortcomings by being both smaller and significantly faster than the most popular options. 
It's also more resilient, e.g. a strongly-typed POCO object can be deserialized back into a loosely-typed string Dictionary and vice-versa.

With that in mind, TypeSerializer's main features are:
*  Fastest and most compact text-serializer for .NET
*  Human readable and writeable, self-describing text format
*  Non-invasive and configuration-free
*  Resilient to schema changes
*  Serializes / De-serializes any .NET data type (by convention)
  *  Supports custom, compact serialization of structs by overriding `ToString()` and `static T Parse(string)` methods
  *  Can serialize inherited, interface or 'late-bound objects' data types
  *  Respects opt-in DataMember custom serialization for DataContract dto types.

These characteristics make it ideal for use anywhere you need to store or transport .NET data-types, e.g. for text blobs in a ORM, data in and out of a key-value store or as the text-protocol in .NET to .NET web services.  
 
As such, it's utilized within ServiceStack's other components:
*  OrmLite - to store complex types on table models as text blobs in a database field and 
*  [ServiceStack.Redis](https://github.com/mythz/ServiceStack.Redis) - to store rich POCO data types into the very fast [redis](http://code.google.com/p/redis) instances.

# Download
`TypeSerializer` is included with [ServiceStack.zip](https://github.com/downloads/mythz/ServiceStack/ServiceStack.zip) 
or available to download separately in a standalone [ServiceStack.Text.zip](https://github.com/downloads/mythz/ServiceStack.Text/ServiceStack.Text.zip).

# Simple API

Like most of the interfaces in Service Stack, the API is simple and descriptive. In most cases these are the only methods that you would commonly use:

	string TypeSerializer.SerializeToString<T>(T value);
	void TypeSerializer.SerializeToWriter<T>(T value, TextWriter writer);

	T TypeSerializer.DeserializeFromString<T>(string value);
	T TypeSerializer.DeserializeFromReader<T>(TextReader reader);

Where *T* can be any .NET POCO type. That's all there is - the API was intentionally left simple :)

You may also be interested in the very useful [T.Dump() extension method](http://www.servicestack.net/mythz_blog/?p=202) for recursively viewing the contents of any C# POCO Type.

----

# Performance
Type Serializer is actually the fastest and most compact *text serializer* available for .NET. 
Out of all the serializers benchmarked, it is the only one to remain competitive with [http://code.google.com/p/protobuf-net/ protobuf-net]'s very fast implementation of [http://code.google.com/apis/protocolbuffers/ Protocol Buffers] - google's high-speed binary protocol.

Below is a series of benchmarks serialize the different tables in the [http://code.google.com/p/servicestack/source/browse/trunk/Common/Northwind.Benchmarks/Northwind.Common/DataModel/NorthwindData.cs Northwind database] (3202 records) with the most popular serializers available for .NET:

### Combined results for serializing / deserialzing a single row of each table in the Northwind database 1,000,000 times
_[view the detailed benchmarks](http://www.servicestack.net/benchmarks/NorthwindDatabaseRowsSerialization.1000000-times.2010-02-06.html)_

|| Serializer || Size || Peformance ||
|| Microsoft DataContractSerializer || 4.68x || 6.72x ||
|| Microsoft JsonDataContractSerializer|| 2.24x || 10.18x ||
|| Microsoft BinaryFormatter || 5.62x || 9.06x ||
|| NewtonSoft.Json || 2.30x || 8.15x ||
|| ProtoBuf.net || 1x || 1x ||
|| ServiceStack TypeSerializer || 1.78x || 1.92x ||

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
To better illustrate the resilience of `TypeSerializer` and the JSV Format check out a real world example of it when it's used to [Painlessly migrate between old and new types in Redis](http://code.google.com/p/servicestack/wiki/MigrationsUsingSchemalessNoSql). 

Support for dynamic payloads and late-bound objects is explained in the post [Versatility of JSV Late-bound objects](http://www.servicestack.net/mythz_blog/?p=314).
