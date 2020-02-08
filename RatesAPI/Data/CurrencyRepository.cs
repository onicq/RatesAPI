using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RatesAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;

namespace RatesAPI.Data
{
    public class CurrencyRepository
    {
        private readonly string _connectionString;

        public CurrencyRepository(IConfiguration configuration) {
            _connectionString = configuration.GetConnectionString("MSSQLConnection");
        }

        public async Task<List<Currency>> GetCurrency(string date, string code) {
            using (SqlConnection con = new SqlConnection(_connectionString)) {
                using (SqlCommand cmd = new SqlCommand("sp_GetRates", con)) {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@A_DATE", System.Data.SqlDbType.Date)).Value = date;
                    cmd.Parameters.Add(new SqlParameter("@CODE", System.Data.SqlDbType.VarChar, 3)).Value = code == null ? "" : code;
                    var response = new List<Currency>();
                    await con.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync()) {
                        while (await reader.ReadAsync()) {
                            response.Add(MapToValue(reader));
                        }
                    }

                    return response;
                }
            }
        }


        public async Task<Response> InsertByDate(string date) {
            Response response = new Response();
            response.count = 0;
            string apiBasicUri = "https://nationalbank.kz/rss/";
            string url = $"get_rates.cfm?fdate={date}";
            XmlDocument resultContent = new XmlDocument();
            using (var client = new HttpClient()) {
                client.BaseAddress = new Uri(apiBasicUri);
                var reponseContent = await client.GetAsync(url);
                reponseContent.EnsureSuccessStatusCode();
                string resultContentString = await reponseContent.Content.ReadAsStringAsync();
                resultContent.LoadXml(resultContentString);
            }
            if (resultContent == null)
                return null;

            foreach (XmlNode node in resultContent.SelectNodes("rates/item")) {
                response.count += await AddRates(new Currency {
                    Title = node.SelectSingleNode("fullname").InnerText,
                    Code = node.SelectSingleNode("title").InnerText,
                    Value = node.SelectSingleNode("description").InnerText,
                    Date = date
                });
            }

            return response;
        }

        public async Task<int> AddRates(Currency value) {
            using (SqlConnection con = new SqlConnection(_connectionString)) {
                using (SqlCommand cmd = new SqlCommand("sp_InsertRates", con)) {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@TITLE", System.Data.SqlDbType.VarChar, 60)).Value = value.Title;
                    cmd.Parameters.Add(new SqlParameter("@CODE", System.Data.SqlDbType.VarChar, 3)).Value = value.Code;
                    cmd.Parameters.Add(new SqlParameter("@VALUE", System.Data.SqlDbType.VarChar)).Value = value.Value;
                    cmd.Parameters.Add(new SqlParameter("@A_DATE", System.Data.SqlDbType.VarChar)).Value = value.Date;
                    cmd.Parameters.Add(new SqlParameter("@MESSAGE", System.Data.SqlDbType.VarChar, 100)).Direction = System.Data.ParameterDirection.Output;
                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                    if (cmd.Parameters["@MESSAGE"] == null) {
                        return 0;
                    }

                    return 1;
                }
            }
        }

        private Currency MapToValue(SqlDataReader reader) {
            return new Currency() {
                Id = (int)reader["Id"],
                Title = reader["TITLE"].ToString(),
                Code = reader["CODE"].ToString(),
                Value = reader["Value"].ToString(),
                Date = reader["A_DATE"].ToString()
            };
        }
    }
}
