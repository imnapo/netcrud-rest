using Microsoft.EntityFrameworkCore;
using Nv.AspNetCore.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nv.AspNetCore.Data
{
  public class UnitOfWork<T> : IUnitOfWork where T : DbContext
  {
    private readonly T context;

    public UnitOfWork(T Context)
    {
      context = Context;
    }
    public async Task<int> CommitAsync()
    {
      return await context.SaveChangesAsync();
    }

    //public void Dispose()
    //{
    //  context.Dispose();
    //}

    //public ValueTask DisposeAsync()
    //{
    //  return context.DisposeAsync();
    //}

        // To detect redundant calls
        private bool _disposed = false;

        // Created in .ctor, omitted for brevity.
        private Utf8JsonWriter _jsonWriter;

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore();

            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsyncCore()
        {
            // Cascade async dispose calls
            if (_jsonWriter != null)
            {
                await _jsonWriter.DisposeAsync();
                _jsonWriter = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _jsonWriter?.Dispose();
                // TODO: dispose managed state (managed objects).
                context.Dispose();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.
        
            _disposed = true;
        }
    }
}
