using LinqToWql.Infrastructure;
using LinqToWql.Language;
using LinqToWql.Test.Mocks.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using LinqToWql.Test.Mocks.Stubs;
using LinqToWql.Test.Mocks.ResultObject;
using LinqToWql.Data;
using System.Linq.Expressions;
using System.Reflection;
using LinqToWql.Model;

namespace LinqToWql.Test.Mocks;

public class MockResourceFactory {
  public static WqlResource<T> CreateEmpty<T>() {
    return (WqlResource<T>) FormatterServices.GetUninitializedObject(typeof(WqlResource<T>));
  }

  public IResourceObject CreateResourceObject<T>(Expression<Func<T>> initExpression) {
    var options = GetValuesFromMemberInitExpression(initExpression.Body);
    return new ResourceObject(options);
  }

  public IResourceObject CreateResourceObject(Expression<Func<IResourceObject>> initExpression)
  {
    var options = GetValuesFromMemberInitExpression(initExpression.Body);
    return new ResourceObject(options);
  }

  private ResourceObjectOptions GetValuesFromMemberInitExpression(Expression expression)
  {
    var options = new ResourceObjectOptions();
    if (expression is not MemberInitExpression memberInitExpression)
    {
      return options;
    }

    var memberAssignments = memberInitExpression.Bindings.Select(binding => (MemberAssignment)binding);

    foreach (var assignment in memberAssignments)
    {
      AddMemberDefToOptions(assignment, options);
    }

    return options;
  }

  private void AddMemberDefToOptions(MemberAssignment assignment, ResourceObjectOptions options)
  {
    var propertyName = assignment.Member.Name;
    var propertyType = ((PropertyInfo)assignment.Member).PropertyType;
    var value = GetValueFromInitExpression(assignment.Expression);

    if (propertyType.IsEnumerable(out var enumerableType))
    {
      if (enumerableType.GetInterfaces().Contains(typeof(IResource)))
      {
        options.EmbeddedListProperties.Add(propertyName, (List<IResource>)value);
        return;
      }
      options.ListProperties.Add(propertyName, (List<object>)value);
      return;
    }

    if (propertyType.GetInterfaces().Contains(typeof(IResource)))
    {
      options.EmbeddedProperties.Add(propertyName, (IResource)value);
      return;
    }

    options.Properties.Add(propertyName, value);
  }

  private object GetValueFromInitExpression(Expression expression)
  {
    ConstantExpression value;
    // Conversion from WqlResourceProperty<T> to T
    if (expression is UnaryExpression { NodeType: ExpressionType.Convert } unary)
    {
      value = (ConstantExpression)unary.Operand;
    }
    else
    {
      value = (ConstantExpression)expression;
    }

    return value.Value!;
  }
}
