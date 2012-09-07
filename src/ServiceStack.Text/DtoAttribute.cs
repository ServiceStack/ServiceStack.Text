using System;

namespace ServiceStack.Text
{
    /// <summary>
    /// Flag that your POCO is a top-level Response DTO
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class DtoAttribute : Attribute {}
}