namespace test1.Service;

using test1.DTOs;
using test1.Repository;
public class VendorService : IVendorService
{
    private readonly IVendorRepository _vendorRepository;

    public VendorService(IVendorRepository vendorRepository)
    {
        _vendorRepository = vendorRepository;
    }

    public Task<VendorResponseDto?> GetVendorAsync(string code)
    {
        return _vendorRepository.GetVendorAsync(code);
    }

    public Task AddVendorAsync(CreateVendorRequest request)
    {
        return _vendorRepository.AddVendorAsync(request);
    }
}