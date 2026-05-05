namespace test1.DTOs;

public class CreateVendorRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<CreateVendorProductRequest> Products { get; set; } = [];
}