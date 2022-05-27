using System.Linq.Expressions;
using System.Runtime.Serialization;
using LinqToWql.Data;
using LinqToWql.Infrastructure;
using LinqToWql.Test.Mocks.ResultObject;

namespace LinqToWql.Test.Mocks;

public class MockResourceFactory {
  public static WqlResource<T> CreateEmpty<T>() {
    return (WqlResource<T>) FormatterServices.GetUninitializedObject(typeof(WqlResource<T>));
  }

  public IResourceObject CreateResourceObject<T>(Expression<Func<T>> initExpression) {
    var options = GetValuesFromMemberInitExpression(initExpression.Body);
    return new ResourceObject(options);
  }

  public IResourceObject CreateResourceObject(Expression<Func<IResourceObject>> initExpression) {
    var options = GetValuesFromMemberInitExpression(initExpression.Body);
    return new ResourceObject(options);
  }

  private ResourceObjectOptions GetValuesFromMemberInitExpression(Expression expression) {
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

  private void AddMemberDefToOptions(MemberAssignment assignment, ResourceObjectOptions options) {
    var propertyName = assignment.Member.Name;
    var value = GetValueFromInitExpression(assignment.Expression);

    options.Properties.Add(propertyName, value);
  }

  private object GetValueFromInitExpression(Expression expression) {
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