using SharedKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Abstractions.Catalogs
{
    public interface IUsersCatalog
    {
        public Task<Result<string>> Login(string Email, string Password, CancellationToken cancellationToken);
    }
}
