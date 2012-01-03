using System.Collections.Generic;
using NUnit.Framework;

namespace ServiceStack.Text.Tests
{
	[TestFixture]
	public class DictionaryTests
		: TestBase
	{

		[Test]
		public void Can_serialize_one_level_dictionary()
		{
			var map = new Dictionary<string, int>
          	{
				{"One", 1}, {"Two", 2}, {"Three", 3}, 
          	};

			Serialize(map);
		}

		[Test]
		public void Can_serialize_empty_map()
		{
			var emptyMap = new Dictionary<string, int>();

			Serialize(emptyMap);
		}

		[Test]
		public void Can_serialize_empty_string_map()
		{
			var emptyMap = new Dictionary<string, string>();

			Serialize(emptyMap);
		}

		[Test]
		public void Can_serialize_two_level_dictionary()
		{
			var map = new Dictionary<string, Dictionary<string, int>>
          		{
					{"map1", new Dictionary<string, int>
			         	{
							{"One", 1}, {"Two", 2}, {"Three", 3}, 
			         	}
					},
					{"map2", new Dictionary<string, int>
			         	{
							{"Four", 4}, {"Five", 5}, {"Six", 6}, 
			         	}
					},
          		};

			Serialize(map);
		}

		[Test]
		public void Can_serialize_two_level_dictionary_with_int_key()
		{
			var map = new Dictionary<int, Dictionary<string, int>>
          		{
					{1, new Dictionary<string, int>
			         	{
							{"One", 1}, {"Two", 2}, {"Three", 3}, 
			         	}
					},
					{2, new Dictionary<string, int>
			         	{
							{"Four", 4}, {"Five", 5}, {"Six", 6}, 
			         	}
					},
          		};

			Serialize(map);
		}


        [Test]
        public void Can_serialize_two_level_dictionary_with_custom_class_key()
        {
            var map = new Dictionary<CustomKey, Dictionary<string, int>>
          		{
					{new CustomKey { Id = 42, IsRoot = true, Name = "key1" }, new Dictionary<string, int>
			         	{
							{"One", 1}, {"Two", 2}, {"Three", 3}, 
			         	}
					},
					{new CustomKey { Id = 21, IsRoot = true, Name = "key2" }, new Dictionary<string, int>
			         	{
							{"Four", 4}, {"Five", 5}, {"Six", 6}, 
			         	}
					},
          		};

            Serialize(map);
        }

        [Test]
        public void Can_serialize_two_level_dictionary_with_custom_class_key2()
        {
            var map = new Dictionary<CustomKey, CustomValue>
          		{
					{
                        new CustomKey { Id = 42, IsRoot = true, Name = "key1" },
                        new CustomValue { SortOrder = 0, Title = "dede" }
					},
					{
                        new CustomKey { Id = 21, IsRoot = true, Name = "key2" }, 
                        new CustomValue { SortOrder = 1, Title = null, Parent = new CustomValue{} }
					},
          		};

            Serialize(map);
        }

        public class CustomKey
        {
            public string Name { get; set; }
            public int Id { get; set; }
            public bool IsRoot { get; set; }
        }

        public class CustomValue
        {
            public int SortOrder { get; set; }
            public string Title { get; set; }
            public CustomValue Parent { get; set; }
        }
	}
}