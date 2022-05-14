using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace LinqToWql.Language; 

public interface IQueryCompiler {
  public TResult Execute<TResult>(Expression query);
}