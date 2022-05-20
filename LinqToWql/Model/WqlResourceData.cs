using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Model;

public abstract class WqlResourceData {
  protected readonly IResultObject Resource;

  public WqlResourceData(IResultObject resource) {
    Resource = resource;
  }

  protected IResultObject ExecuteMethod(string command, params Tuple<string, dynamic>[] args) {
    var dictArgs = args.ToDictionary(x => x.Item1, x => (object) x.Item2);
    return Resource.ExecuteMethod(command, dictArgs);
  }

  protected Tuple<string, dynamic> Parameter(string name, dynamic value) {
    return new Tuple<string, dynamic>(name, value);
  }

  public void Update() {
    Resource.Put();
    Resource.Get();
  }
}