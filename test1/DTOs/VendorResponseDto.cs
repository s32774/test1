namespace test1.DTOs;

public class VendorResponseDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<ProductResponseDto> Products { get; set; } = [];
}