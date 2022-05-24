using System.Linq.Expressions;
using LinqToWql.Infrastructure;
using LinqToWql.Model;
using LinqToWql.Test.Mocks.ResultObject;
using LinqToWql.Test.Mocks.Stubs;
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Test.Mocks;

public class MockResultObjectBuilder<T> where T : WqlResourceData<T> {
  private readonly List<Expression> _enumerableItems = new();

  public MockResultObjectBuilder() {
  }

  public MockResultObjectBuilder(Expression<Func<T>> itemGenerator) {
    _enumerableItems.Add(itemGenerator.Body);
  }

  public MockResultObjectBuilder<T> AddEnumerableItem(Expression<Func<T>> itemGenerator) {
    _enumerableItems.Add(itemGenerator.Body);
    return this;
  }

  public IResultObject Build() {
    if (_enumerableItems.Count == 0) {
      var dict = new Dictionary<string, object>();
      return new StubResultObject(dict);
    }

    return MakeResultObject();
  }

  /// <summary>
  ///   Returns a IWqlConnection that returns
  ///   the result object on queries.
  /// </summary>
  /// <returns></returns>
  public IWqlConnection BuildToConnection() {
    return new StubWqlConnection(Build());
  }

  public IWqlQueryProcessor BuildToQueryProcessor() {
    return new StubWqlQueryProcessor(Build());
  }

  public IWqlContextOptions BuildToContextOptions() {
    return new StubWqlContextOptions(Build());
  }

  private IResultObject MakeResultObject() {
    var resultObjectItems = _enumerableItems.Select(GetValuesFromMemberInitExpression).ToList();
    return new StubResultObject(resultObjectItems);
  }

  private Dictionary<string, object> GetValuesFromMemberInitExpression(Expression expression) {
    if (expression is not MemberInitExpression memberInitExpression) {
      return new Dictionary<string, object>();
    }

    return memberInitExpression.Bindings
                               .Select(binding => (MemberAssignment) binding)
                               .ToDictionary(init => init.Member.Name,
                                 init => GetValueFromInitExpression(init.Expression));
  }

  private object GetValueFromInitExpression(Expression expression) {
    ConstantExpression value;
    if (expression is UnaryExpression {NodeType: ExpressionType.Convert} unary) {
      value = (ConstantExpression) unary.Operand;
    }
    else {
      value = (ConstantExpression) expression;
    }

    return value.Value!;
  }
}