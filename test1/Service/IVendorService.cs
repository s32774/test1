namespace test1.Service;
using test1.DTOs;

public interface IVendorService
{
    Task<VendorResponseDto?> GetVendorAsync(string code);

    Task AddVendorAsync(CreateVendorRequest request);
}
