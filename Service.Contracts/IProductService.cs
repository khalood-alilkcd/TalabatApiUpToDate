using Shared.DataTransfierObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Contracts
{
    public interface IProductService
    {
        /*Task<IReadOnlyList<ProductDto>> GetAllProducts();
        Task<ProductDto> GetByIdProduct(int id);*/
        Task<IReadOnlyList<ProductDto>> GetProductsFromClientAsync(int clientId, bool trachChanges);
        Task<ProductDto> CreateProduct(int clientId, CreateProductDto createProduct, bool trackChanges);
        Task<ProductDto> GetProductFromClientAsync(int clientId, int productId, bool trackChanges);
        /*
        Task<(IEnumerable<ProductDto> products, string ids)> CreateProductCollection(Guid clientId,
            IEnumerable<CreateProductDto> createProduct, bool trackChanges);
        Task<IEnumerable<ProductDto>> GetProductsByIdsAsync(Guid clientId, IEnumerable<Guid> ids, bool trackChanges);*/
    }
}
