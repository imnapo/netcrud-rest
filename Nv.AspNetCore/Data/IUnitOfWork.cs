using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nv.AspNetCore.Data
{
  public interface IUnitOfWork : IAsyncDisposable, IDisposable
    {
    Task<int> CommitAsync();
  }
}
