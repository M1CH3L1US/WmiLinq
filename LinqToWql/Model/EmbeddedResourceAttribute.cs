using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToWql.Model;

/// <summary>
/// Marks the attributed class as an embedded
/// resource. Embedded resources are property
/// objects of recoures. E.g. A collection member
/// object is an embedded object of the collection 
/// resource.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class EmbeddedResourceAttribute : ResourceAttribute {
}
