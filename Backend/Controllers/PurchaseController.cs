using Backend.Application.Dtos;
using Backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Backend.Controllers;

public class PurchaseController : ApiController
    {
    private readonly IPurchaseService _purchaseService;

    public PurchaseController(IPurchaseService purchaseService)
        {
        _purchaseService = purchaseService;
        }
    
    [Route("purchases"), HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<PurchaseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetResultset([FromQuery] PurchaseFilterDto filter)
        {
        // Validate start parameter
        if (filter.Start < 0)
            return BadRequest(new { message = "Start must be greater than or equal to 0" });

        // Validate pageSize parameter
        if (filter.PageSize is <= 0 or > 100)
            return BadRequest(new { message = "PageSize must be between 1 and 100" });

        // Validate date range if both dates are provided
        if (filter.TransactionStartDate.HasValue && filter.TransactionEndDate.HasValue && filter.TransactionStartDate > filter.TransactionEndDate)
            return BadRequest(new { message = "TransactionStartDate must be before or equal to TransactionEndDate" });

        // Validate amount range if both amounts are provided
        if (filter is { MinAmount: not null, MaxAmount: not null } && filter.MinAmount > filter.MaxAmount)
            return BadRequest(new { message = "MinAmount must be less than or equal to MaxAmount" });

        var result = await _purchaseService.GetPagedAsync(filter);
        return Ok(result);
        }
    
    [Route("purchases/{id:int}"), HttpGet]
    [ProducesResponseType(typeof(PurchaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
        {
        var purchase = await _purchaseService.GetByIdAsync(id);
        if (purchase != null) 
            return Ok(purchase);
        
        return NotFound(new { message = $"Purchase with ID {id} not found" });
        }
    
    [Route("purchases/{transactionIdentifier:guid}"), HttpGet]
    [ProducesResponseType(typeof(PurchaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByTransactionIdentifier(Guid transactionIdentifier)
        {
        var purchase = await _purchaseService.GetByTransactionIdentifierAsync(transactionIdentifier);
        if (purchase != null) 
            return Ok(purchase);
        
        return NotFound(new { message = $"Purchase with transaction identifier {transactionIdentifier} not found" });
        }
    
    [Route("purchases"), HttpPost]
    [ProducesResponseType(typeof(PurchaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(int), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseDto dto)
        {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
            {
            var purchase = await _purchaseService.CreateAsync(dto);
            return Ok(purchase.Id);
            }
        catch (Exception ex)
            {
            Log.Error(ex, "Error creating purchase");
            return BadRequest(new { message = "Error creating purchase", error = ex.Message });
            }
        }
    
    [Route("purchases/{id:int}"), HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePurchaseDto dto)
        {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
            {
            await _purchaseService.UpdateAsync(id, dto);
            return Ok();
            }
        catch (KeyNotFoundException ex)
            {
            return NotFound(new { message = ex.Message });
            }
        catch (Exception ex)
            {
            Log.Error(ex, "Error updating purchase with ID {PurchaseId}", id);
            return BadRequest(new { message = "Error updating purchase", error = ex.Message });
            }
        }
    
    [Route("purchases/{id:int}"), HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
        {
        try
            {
            await _purchaseService.DeleteAsync(id);
            return Ok();
            }
        catch (KeyNotFoundException ex)
            {
            return NotFound(new { message = ex.Message });
            }
        catch (Exception ex)
            {
            Log.Error(ex, "Error deleting purchase with ID {PurchaseId}", id);
            return BadRequest(new { message = "Error deleting purchase", error = ex.Message });
            }
        }
    }
