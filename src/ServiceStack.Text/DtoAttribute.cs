using System;

namespace ServiceStack.Text
{
    /// <summary>
    /// Flag that your POCO is a top-level Response DTO
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class DtoAttribute : Attribute {}

    public enum CsvBehavior
    {
        FirstEnumerable
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class CsvAttribute : Attribute
    {
        public CsvBehavior CsvBehavior { get; set; }
        public CsvAttribute(CsvBehavior csvBehavior)
        {
            this.CsvBehavior = csvBehavior;
        }
    }
}