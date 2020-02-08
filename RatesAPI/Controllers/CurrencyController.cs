using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RatesAPI.Data;
using RatesAPI.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RatesAPI.Controllers
{
    [Route("RatesAPI/[controller]")]
    public class CurrencyController:ControllerBase
    {
        // GET: /<controller>/
        private readonly CurrencyRepository _repository;

        public CurrencyController(CurrencyRepository repository) {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        [HttpGet("currency")]
        public async Task<List<Currency>> GetByDateAndCode(string date, string code) {
            var response = await _repository.GetCurrency(date, code);
            return response;
        }

        [HttpGet("save")]
        public async Task<Response> InsertByDate(string date) {

            var response = await _repository.InsertByDate(date);
            return response;
        }
    }
}
