using AutoMapper;

namespace AspNetCoreHistory.Models;

public class ProductMapper : Profile
{
    public ProductMapper()
    {
        CreateMap<ProductCreate, Product>();
        CreateMap<ProductUpdate, Product>();
        CreateMap<ProductHistory, Product>();
    }
}
