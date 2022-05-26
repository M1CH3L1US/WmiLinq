using Microsoft.ConfigurationManagement.ManagementProvider;
using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToWql.Model;

/// <summary>
/// Represents a resource base class. Because
/// Resources cannot inherit from anything other 
/// than an interface, because they already derive
/// from WqlResourceData.
/// 
/// Base classes can only have primitve datatypes 
/// as properties and cannot contain array properties.
/// </summary>
public interface IWqlResourceBase<TBase> : IResource {
  /// <summary>
  /// Adapt the base class to an implementation of
  /// type T
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  public T AdaptTo<T>() where T : TBase;
}
