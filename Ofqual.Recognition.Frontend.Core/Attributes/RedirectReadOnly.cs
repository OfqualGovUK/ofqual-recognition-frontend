namespace Ofqual.Recognition.Frontend.Core.Attributes;

/*
 * This attribute is used to indicate to middleware that it should redirect when accessing an area inaccessible to read only applications
 *
 * Use this attribute whenever a client makes a request related to a specific application. This should only be used in Controller endpoints
 * 
 */
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RedirectReadOnly : Attribute
{
}
