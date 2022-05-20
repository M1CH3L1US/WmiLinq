using System.Collections;

namespace LinqToWql.Language;

public class QueryResultParseOptions {
  private readonly List<Func<object, object>> _postParsingProcessors = new();
  private readonly List<Func<IEnumerable, IEnumerable>> _postQueryProcessors = new();

  public IEnumerable<Func<object, object>> PostParsingProcessors => _postParsingProcessors;
  public IEnumerable<Func<IEnumerable, IEnumerable>> PostQueryProcessors => _postQueryProcessors;

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

  /// <summary>
  ///   The type to which the query result objects
  ///   should be parsed to.
  /// </summary>
  public Type QueryResultType { get; set; }

  public void AddPostParsingProcessor(Func<object, object> updateParseResult) {
    _postParsingProcessors.Add(updateParseResult);
  }

  public void AddPostQueryProcessor(Func<IEnumerable, IEnumerable> updateQueryResult) {
    _postQueryProcessors.Add(updateQueryResult);
  }
}