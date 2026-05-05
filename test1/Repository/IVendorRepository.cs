namespace test1.Repository;

using test1.DTOs;
public interface IVendorRepository
{
    Task<VendorResponseDto?> GetVendorAsync(string code);

    Task AddVendorAsync(CreateVendorRequest request);
}