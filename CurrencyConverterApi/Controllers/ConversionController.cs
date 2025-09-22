using CurrencyConverterApi.Models;
using CurrencyConverterApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Mime;
using System.Threading.Tasks;

namespace CurrencyConverterApi.Controllers
{
    [ApiController]
    [Authorize]                                   // Protects all endpoints in this controller
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public class ConversionController : ControllerBase
    {
        private readonly ICurrencyConverterService _converter;
        private readonly ILogger<ConversionController> _logger;

        // SINGLE constructor for DI – no overloads allowed
        public ConversionController(
            ICurrencyConverterService converter,
            ILogger<ConversionController> logger)
        {
            _converter = converter;
            _logger = logger;
        }

        /// <summary>
        /// Converts USD to SLL.
        /// </summary>
        /// <param name="request">Contains the USD amount to convert.</param>
        /// <returns>A 200 with ConversionResponse or 400/500 on error.</returns>
        [HttpPost("usd-to-sll")]
        public async Task<IActionResult> Post([FromBody] ConversionRequest request)
        {
            if (request.Amount <= 0)
                return BadRequest("Amount must be greater than zero.");

            try
            {
                var result = await _converter.ConvertUsdToSllAsync(request.Amount);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Log full exception and return message (development only!)
                _logger.LogError(ex, "Conversion failed for amount {Amount}", request.Amount);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
