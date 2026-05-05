using Microsoft.AspNetCore.Mvc;

using test1.DTOs;
using test1.Exception;
using test1.Service;

namespace test1.Controllers;
// endpoints 
[ApiController]
[Route("api/vendors")]
public class VendorsController : ControllerBase
{
    private readonly IVendorService _vendorService;

    public VendorsController(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetVendor(string code)
    {
        var vendor = await _vendorService.GetVendorAsync(code);

        if (vendor is null)
        {
            return NotFound($"Vendor with code {code} was not found.");
        }

        return Ok(vendor);
    }

    [HttpPost]
    public async Task<IActionResult> AddVendor([FromBody] CreateVendorRequest request)
    {
        try
        {
            await _vendorService.AddVendorAsync(request);
        }
        catch (NotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        return Ok();
    }
}