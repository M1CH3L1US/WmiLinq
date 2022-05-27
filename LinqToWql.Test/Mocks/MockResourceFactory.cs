using System.Linq.Expressions;
using System.Runtime.Serialization;
using LinqToWql.Data;
using LinqToWql.Infrastructure;
using LinqToWql.Model;
using LinqToWql.Test.Mocks.ResultObject;

namespace LinqToWql.Test.Mocks;

public class MockResourceFactory {
  public static WqlResource<T> CreateEmpty<T>() {
    return (WqlResource<T>) FormatterServices.GetUninitializedObject(typeof(WqlResource<T>));
  }

  public static IResourceObject
    CreateResourceObject<T>(Expression<Func<T>> initExpression, WqlResourceContext context) {
    var options = CreateOptionsFromMemberInitExpression(initExpression.Body);
    options.Context = context;
    return new ResourceObject(options);
  }

  public static IResourceObject CreateResourceObject<T>(Expression<Func<T>> initExpression) where T : IResource {
    var options = CreateOptionsFromMemberInitExpression(initExpression.Body);
    return new ResourceObject(options);
  }

  public static IResourceObject CreateResourceObject(Expression<Func<IResource>> initExpression) {
    var options = CreateOptionsFromMemberInitExpression(initExpression.Body);
    return new ResourceObject(options);
  }

  private static ResourceObjectOptions CreateOptionsFromMemberInitExpression(Expression expression) {
    var options = new ResourceObjectOptions();
    if (expression is not MemberInitExpression memberInitExpression) {
      return options;
    }

    var memberAssignments = memberInitExpression.Bindings.Select(binding => (MemberAssignment) binding);

    foreach (var assignment in memberAssignments) {
      AddMemberDefToOptions(assignment, options);
    }

    return options;
  }

  private static void AddMemberDefToOptions(MemberAssignment assignment, ResourceObjectOptions options) {
    var propertyName = assignment.Member.Name;
    var value = GetValueFromInitExpression(assignment.Expression);

    options.Properties.Add(propertyName, value);
  }

  private static object GetValueFromInitExpression(Expression expression) {
    ConstantExpression value;
    // Conversion from WqlResourceProperty<T> to T
    if (expression is UnaryExpression {NodeType: ExpressionType.Convert} unary) {
      value = (ConstantExpression) unary.Operand;
    }
    else {
      value = (ConstantExpression) expression;
    }

    return value.Value!;
  }
}