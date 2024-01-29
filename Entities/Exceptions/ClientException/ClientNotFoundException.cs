using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Exceptions.ClientException
{
    public sealed class ClientNotFoundException : NotFoundException
    {
        public ClientNotFoundException(int clientId) : base($"The client with id: {clientId} doesn't exist in the database.")
        {
        }
    }
}
