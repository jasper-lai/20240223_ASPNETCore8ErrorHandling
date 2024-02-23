namespace MyBakeryMvcWeb.Services
{
    using Microsoft.AspNetCore.Mvc;
    using MyBakeryMvcWeb.ViewModels;

    public interface IProductService
    {
        int Create(ProductViewModel product);
        int OccursOutRangeException(ProductViewModel product);
    }
}