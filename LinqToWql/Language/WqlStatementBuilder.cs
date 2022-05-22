using System.Linq.Expressions;
using LinqToWql.Language.Expressions;
using LinqToWql.Language.Statements;
using LinqToWql.Model;

namespace LinqToWql.Language;

public class WqlStatementBuilder {
  private readonly List<Action<WqlQueryBuilder>> _builderActions = new();
  private readonly Expression _source;

  public WqlStatementBuilder(Expression source) {
    _source = source;
  }

  public WqlStatement Build() {
    return new WqlStatement(_source, _builderActions);
  }

  public WqlStatementBuilder TryAddWhereClauseFromLambda(LambdaExpression? lambdaExpression,
    ExpressionChainType chainType = ExpressionChainType.And) {
    if (lambdaExpression is null) {
      return this;
    }

    return AddWhereClauseFromLambda(lambdaExpression, chainType);
  }

  public WqlStatementBuilder AddWhereClauseFromLambda(LambdaExpression lambdaExpression,
    ExpressionChainType chainType = ExpressionChainType.And) {
    var innerExpression = GetInnerExpressionFromLambda(lambdaExpression);
    AddActionInBuilderCtx(builder => builder.AddWhereClause(innerExpression, chainType));

    return this;
  }

  public WqlStatementBuilder AddSelectClauseFromLambda(LambdaExpression lambdaExpression) {
    var memberAccesses = GetInnerMemberAccessFromLambda(lambdaExpression);
    AddActionInBuilderCtx(builder => builder.AddSelectProperties(memberAccesses));

    // If we select down to a single property,
    // we need to make sure that that information
    // is passed down to the type mapper through the
    // ParseOptions.
    if (memberAccesses.Count == 1) {
      var propertyToSelect = memberAccesses.Single().PropertyName;
      AddActionInBuilderCtx(builder => builder.ParseOptions.SinglePropertyToSelect = propertyToSelect);
    }

    return this;
  }

  public WqlStatementBuilder AddQueryResultProcessor(Func<IEnumerable<object>, object?> resultProcessor) {
    AddActionInBuilderCtx(builder => builder.ParseOptions.AddResultProcessor(resultProcessor));
    return this;
  }

  public void AddActionInBuilderCtx(Action<WqlQueryBuilder> builderAction) {
    _builderActions.Add(builderAction);
  }

  private WqlExpression GetInnerExpressionFromLambda(LambdaExpression lambda) {
    if (lambda.Body is MethodCallExpression methodCall) {
      return GetInnerMethodCallFromLambda(lambda, methodCall);
    }

    if (lambda.Body is BinaryExpression binary) {
      return ConvertToBinaryWqlExpression(binary);
    }

    throw new NotSupportedException();
  }

  private WqlExpression GetInnerMethodCallFromLambda(LambdaExpression lambda, MethodCallExpression methodCall) {
    var method = methodCall.Method;

    if (method.DeclaringType != typeof(WqlResourcePropertyQueryExtensions)) {
      throw new NotSupportedException("Only methods on WqlResourceProperties are supported");
    }

    var property = (MemberExpression) methodCall.Arguments.First();
    var argument = (ConstantExpression) methodCall.Arguments.Skip(1).First();

    var propertyName = property.Member.Name;

    return method.Name switch {
      nameof(WqlResourcePropertyQueryExtensions.IsA) =>
        new IsAWqlExpression(propertyName, (string) argument.Value!),
      nameof(WqlResourcePropertyQueryExtensions.Like) =>
        new LikeWqlExpression(propertyName, (string) argument.Value!),
      _ => throw new NotImplementedException()
    };
  }

  private BinaryWqlExpression ConvertToBinaryWqlExpression(BinaryExpression binary) {
    var left = ConvertToWqlExpression(binary.Left);
    var right = ConvertToWqlExpression(binary.Right, true);
    var op = binary.NodeType;

    return new BinaryWqlExpression(left, op, right);
  }

  private List<PropertyWqlExpression> GetInnerMemberAccessFromLambda(LambdaExpression lambda) {
    if (lambda.Body is MemberExpression memberAccess) {
      var member = memberAccess.Member.Name;
      return new List<PropertyWqlExpression> {new(member)};
    }

    if (lambda.Body is NewExpression newExpression) {
      return newExpression.Arguments
                          .Select(argument => ConvertToWqlExpression(argument))
                          .Cast<PropertyWqlExpression>()
                          .ToList();
    }

    throw new NotSupportedException("The select operation is not supported");
  }

  private WqlExpression ConvertToWqlExpression(Expression expression, bool isValueExpected = false) {
    if (expression is MemberExpression memberExpression) {
      if (!isValueExpected) {
        // This is a property expression in .Where(x => x.Name ...)
        return new PropertyWqlExpression(memberExpression.Member.Name);
      }

      var value = GetValueFromClosureMemberAccess(memberExpression);
      return new ConstantWqlExpression(value);
    }

    if (expression is ConstantExpression constant) {
      return new ConstantWqlExpression(constant.Value);
    }

    if (expression is BinaryExpression binary) {
      return ConvertToBinaryWqlExpression(binary);
    }

    // This is the case when we convert a resource property,
    // which is always of type WqlResourceProperty<TValue>,
    // in a lambda expression like:
    // resource.Where(x => x.Name == this.Name)
    // where this.Name is any member of a resource class
    if (expression is UnaryExpression unary) {
      return ConvertToWqlExpression(unary.Operand, isValueExpected);
    }

    throw new NotSupportedException();
  }

  /// <summary>
  ///   In cases, where the value to compare in a lambda
  ///   is a value from a closure e.g.
  ///   <code>
  /// var foo = "Foo";
  /// queryable.Where(x => x.Name == foo);
  /// </code>
  ///   We need to get that value instead of placing the member name
  ///   in the query. This method gets that value from the runtime
  ///   member info.
  /// </summary>
  /// <param name="expression"></param>
  /// <returns></returns>
  private object GetValueFromClosureMemberAccess(MemberExpression expression) {
    var objectMember = Expression.Convert(expression, typeof(object));
    var getValueLambda = Expression.Lambda<Func<object>>(objectMember);
    var valueGetter = getValueLambda.Compile();

    var value = valueGetter();

    if (!IsWqlResourceProperty(value)) {
      return value;
    }

    return GetValueFromWqlResourceProperty(value);
  }

  private bool IsWqlResourceProperty(object obj) {
    var objectType = obj.GetType();

    if (!objectType.IsGenericType) {
      return false;
    }

    return objectType.GetGenericTypeDefinition() == typeof(WqlResourceProperty<>);
  }

  private object GetValueFromWqlResourceProperty(object property) {
    // We should have already determined that the type of `property`
    // is WqlResourceProperty so we can just get the reflection property
    // field and return its value.
    return property.GetType()!
                   .GetProperty(nameof(WqlResourceProperty<object>.Value))!
                   .GetValue(property);
  }
}