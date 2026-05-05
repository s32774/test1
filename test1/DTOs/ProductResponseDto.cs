namespace test1.DTOs;

public class ProductResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal StrickerPrice { get; set; }
    public ProductTypeDto ProductType { get; set; } = new();
    public MakerDto Maker { get; set; } = new();
    public VendorOfferDto VendorOffer { get; set; } = new();
}