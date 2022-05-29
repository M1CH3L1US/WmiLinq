using LinqToWql.Infrastructure;

namespace LinqToWql.Language;

public class QueryResultParseOptions {
  private readonly List<Func<IEnumerable<object>, object?>> _resultProcessors = new();
  public WqlResourceContext Context;
  public IEnumerable<Func<IEnumerable<object>, object?>> ResultProcessors => _resultProcessors;

  /// <summary>
  ///   If property is true, the query result will be parsed
  ///   to the value of the property specified.
  /// </summary>
  public bool ShouldSelectSingleProperty => SinglePropertyToSelect is not null;

  public string? SinglePropertyToSelect { get; set; }

  /// <summary>
  ///   The type of the resource from which the WqlResource
  ///   was created.
  /// </summary>
  public Type ResourceType { get; set; }

  public QueryResultParseOptions(WqlResourceContext context) {
    Context = context;
  }

  /// <summary>
  ///   Add a post processor function for the query result.
  ///   Optionally, return an updated version of the input.
  ///   Beware that, if the query operator is an intermediate
  ///   operator that does return another IEnumerable, you
  ///   should make sure that the returned value is an IEnumerable
  ///   because it will be casted as such for the next operation in the
  ///   pipeline.
  /// </summary>
  /// <param name="resultProcessor"></param>
  public void AddResultProcessor(Func<IEnumerable<object>, object?> resultProcessor) {
    _resultProcessors.Add(resultProcessor);
  }
}