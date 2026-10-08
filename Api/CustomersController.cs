using AcxiomCRM.Authorization;
using AcxiomCRM.Constants;
using AcxiomCRM.DTOs;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using AcxiomCRM.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Api
{
    [Authorize(Policy = "CrmUser")]
    [Route("api/customers")]
    public class CustomersApiController : ApiControllerBase
    {
        private readonly ICustomerService _customers;

        public CustomersApiController(ICustomerService customers)
        {
            _customers = customers;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var (items, total) = await _customers.GetCustomersAsync(search, status, page, pageSize, CurrentUserId, CurrentRole);
            return Ok(new { total, page, pageSize, items = items.Select(DtoMapper.ToDto) });
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var customer = await _customers.GetByIdAsync(id);
            if (customer == null) return ApiError(404, "Customer not found.");
            if (!ResourceAuthorization.CanAccessCustomer(CurrentRole, CurrentUserId, CurrentUserName, customer))
            {
                return ApiError(403, "You are not allowed to access this customer.");
            }

            return Ok(DtoMapper.ToDto(customer));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerWriteDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ApiError(400, "Validation failed.", ModelState.ToDictionary(k => k.Key, k => k.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()));
            }

            var uniqueError = await UniqueErrors(dto);
            if (uniqueError != null) return uniqueError;

            var customer = ToEntity(dto);
            if (CurrentRole == AppRoles.SalesExecutive) customer.AssignedToUserId = CurrentUserId;
            var created = await _customers.CreateAsync(customer, CurrentUserId, CurrentUserName, ClientIp);
            return StatusCode(201, DtoMapper.ToDto(created));
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CustomerWriteDto dto)
        {
            var existing = await _customers.GetByIdAsync(id);
            if (existing == null) return ApiError(404, "Customer not found.");
            if (!ResourceAuthorization.CanAccessCustomer(CurrentRole, CurrentUserId, CurrentUserName, existing))
            {
                return ApiError(403, "You are not allowed to update this customer.");
            }

            if (!ModelState.IsValid)
            {
                return ApiError(400, "Validation failed.");
            }

            var uniqueError = await UniqueErrors(dto, id);
            if (uniqueError != null) return uniqueError;

            var entity = ToEntity(dto);
            entity.CustomerId = id;
            if (CurrentRole == AppRoles.SalesExecutive) entity.AssignedToUserId = existing.AssignedToUserId;
            var updated = await _customers.UpdateAsync(entity, CurrentUserId, CurrentUserName, ClientIp);
            return Ok(DtoMapper.ToDto(updated));
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _customers.GetByIdAsync(id);
            if (existing == null) return ApiError(404, "Customer not found.");
            if (!ResourceAuthorization.CanAccessCustomer(CurrentRole, CurrentUserId, CurrentUserName, existing))
            {
                return ApiError(403, "You are not allowed to delete this customer.");
            }

            var (ok, message) = await _customers.DeleteOrDeactivateAsync(id, true, CurrentUserId, CurrentUserName, ClientIp);
            return ok ? Ok(new { message }) : ApiError(400, message);
        }

        private async Task<IActionResult?> UniqueErrors(CustomerWriteDto dto, int? excludeId = null)
        {
            if (!ValidationHelpers.IsValidEmail(dto.Email)) return ApiError(400, "Enter a valid email address.");
            if (!ValidationHelpers.IsValidPhone(dto.Phone)) return ApiError(400, "Enter a valid phone number.");
            if (!await _customers.IsEmailUniqueAsync(dto.Email, excludeId)) return ApiError(409, "A customer with this email already exists.");
            if (!await _customers.IsPhoneUniqueAsync(dto.Phone, excludeId)) return ApiError(409, "A customer with this phone already exists.");
            return null;
        }

        private static Customer ToEntity(CustomerWriteDto dto) => new()
        {
            CustomerName = dto.CustomerName,
            Email = dto.Email,
            Phone = dto.Phone,
            CompanyName = dto.CompanyName,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            Status = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status,
            AssignedToUserId = dto.AssignedToUserId
        };
    }
}
