using System.Linq.Expressions;
using LinqToWql.Language.Expressions;

namespace LinqToWql.Language;

public class WqlExpressionVisitor : QueryableExpressionVisitor {
  protected override Expression TranslateOrWhere(Expression source, LambdaExpression lambdaExpression) {
    return Builder(source)
           .AddWhereClauseFromLambda(lambdaExpression, ExpressionChainType.Or)
           .Build();
  }

  protected override Expression TranslateWhere(Expression source, LambdaExpression lambdaExpression) {
    return Builder(source)
           .AddWhereClauseFromLambda(lambdaExpression)
           .Build();
  }

  protected override Expression TranslateSelect(Expression source, LambdaExpression lambdaExpression) {
    return Builder(source)
           .AddSelectClauseFromLambda(lambdaExpression)
           .Build();
  }

  protected override Expression TranslateSingle(Expression source, LambdaExpression? lambdaExpression) {
    return Builder(source)
           .TryAddWhereClauseFromLambda(lambdaExpression)
           .AddQueryResultProcessor(result =>
             lambdaExpression is null
               ? result.Single()
               // We still apply the predicate if it exists to make
               // sure the query result is consistent with what the
               // user expects - Nested where queries, like we do here
               // may result in a different ordering, or multiple query
               // results where Single would throw, even though the 
               // predicate would only result in one item.
               : result.Single(ToFunc<bool>(lambdaExpression))
           )
           .Build();
  }

  protected override Expression TranslateSingleOrDefault(Expression source, LambdaExpression? lambdaExpression) {
    return Builder(source)
           .TryAddWhereClauseFromLambda(lambdaExpression)
           .AddQueryResultProcessor(result =>
             lambdaExpression is null
               ? result.SingleOrDefault()
               : result.SingleOrDefault(ToFunc<bool>(lambdaExpression))
           )
           .Build();
  }

  protected override Expression TranslateAll(Expression source, LambdaExpression lambdaExpression) {
    return Builder(source)
           .TryAddWhereClauseFromLambda(lambdaExpression)
           .AddQueryResultProcessor(result => result.All(ToFunc<bool>(lambdaExpression)))
           .Build();
  }

  protected override Expression TranslateAny(Expression source, LambdaExpression? lambdaExpression) {
    return Builder(source)
           .AddQueryResultProcessor(result =>
             lambdaExpression is null
               ? result.Any()
               : result.Any(ToFunc<bool>(lambdaExpression))
           )
           .Build();
  }

  protected override Expression TranslateFirst(Expression source, LambdaExpression? lambdaExpression) {
    return Builder(source)
           .TryAddWhereClauseFromLambda(lambdaExpression)
           .AddQueryResultProcessor(result =>
             lambdaExpression is null
               ? result.First()
               : result.First(ToFunc<bool>(lambdaExpression))
           )
           .Build();
  }

  protected override Expression TranslateSkip(Expression source, ConstantExpression count) {
    return Builder(source)
           .AddQueryResultProcessor(result => result.Skip((int) count.Value))
           .Build();
  }

  protected override Expression TranslateTake(Expression source, ConstantExpression count) {
    return Builder(source)
           .AddQueryResultProcessor(result => result.Take((int) count.Value))
           .Build();
  }

  protected override Expression TranslateAsEnumerable(Expression source) {
    return Builder(source)
           .AddQueryResultProcessor(result => result.AsEnumerable())
           .Build();
  }

  protected override Expression TranslateCount(Expression source) {
    return Builder(source)
           .AddQueryResultProcessor(result => result.Count())
           .Build();
  }

  protected override Expression TranslateWithin(Expression source, ConstantExpression timeout) {
    throw new NotImplementedException();
  }

  protected override Expression TranslateHaving(Expression source, LambdaExpression predicate) {
    throw new NotImplementedException();
  }

  private WqlStatementBuilder Builder(Expression source) {
    return new WqlStatementBuilder(source);
  }

  /// <summary>
  ///   [LambdaExpression].Compile returns a Func[TSource, TResult]
  ///   for IEnumerable methods. We cannot cast this to a Func[object, TResult]
  ///   and because of that we wrap the compiled method in another Func, which
  ///   complies with the Func[object, TResult] type. At runtime, we can invoke
  ///   the compiled delegate with the runtime type of [object].
  /// </summary>
  /// <param name="lambda"></param>
  /// <typeparam name="TResult"></typeparam>
  /// <returns></returns>
  private Func<object, TResult> ToFunc<TResult>(LambdaExpression lambda) {
    return obj => {
      // This is inlined into the func in order
      // to not create a closure
      var lambdaFunc = lambda.Compile();
      return (TResult) lambdaFunc.DynamicInvoke(obj);
    };
  }
}