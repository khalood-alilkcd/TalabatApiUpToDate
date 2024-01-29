using Contracts;
using Entities.ErrorModel;
using Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Repository;
using Service.Contracts;
using Shared.DataTransfierObject;
using Shared.RequestFeature;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Talabat.ActionFilters;

namespace Talabat.Presentation.Controllers
{
    [ApiController]
    [Route("api/clients")]
    public class ClientController : ControllerBase
    { 
        private readonly IServiceManager _service;
        private readonly RepositoryContext _context;

        public ClientController(
            IServiceManager service, 
            RepositoryContext context 
            )
        {
            _service=service;
            _context=context;
        }

        /// must prompt to open redis file by input redis-server and then another prompt input redis-cli
        [Cached(600)]
        [HttpGet]
        public async Task<IActionResult> GetAllClients([FromQuery] CLientParameter cLientParameter)
        {
            var pagedResult = await _service.Client.GetAllClient(cLientParameter, trackChanges: false);
            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagedResult.metaData));
            return Ok( pagedResult.clients);
        }
        [Cached(600)]
        [HttpGet("address/{address}")]
        public IActionResult GetClientByAddress(string address)
        {
            var clients = _service.Client.GetAllClientByAddress(address);
            return Ok(clients);
        }
        [Cached(600)]
        [HttpGet("types")]
        public IActionResult GetTypes()
        {
            var types = _context.Set<ClientType>().ToList();
            return Ok(types);
        }
        [Cached(600)]
        [HttpGet("{clientId}")]
        public async Task<IActionResult> GetClientAsync(int clientId)
        {
            var client = await _service.Client.GetClientAsync(clientId, trackChages: false);
            if (client == null) return NotFound(new ApiResponse(404, "the client you want to delete not exist"));
            return Ok(client);
        }

        [HttpPost]
        public async Task<IActionResult> CreateClient([FromForm] ClientForCreatingDto clintForCreating)
        {
            
             
            var client = await _service.Client.CreateClientAsync(clintForCreating);
            if (client is null) return BadRequest(new ApiResponse(400));
            return CreatedAtRoute("clientById", new { clientId = client.Id }, client);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(int id)
        {
            await _service.Client.DeleteClientAsync(id, trackChanges: false);
            return NoContent();
        }

        [HttpPut("{id}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateClient(int id, [FromBody] ClientForUpdateDto updateDto)
        {
           
            await _service.Client.UpdateClientAsync(id, updateDto, trackChanges: false);
            return NoContent();
        }

        

        
    }
}
