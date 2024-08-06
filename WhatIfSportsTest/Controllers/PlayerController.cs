using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WhatIfSportsTest.Models;
using System.Data;
using System;
using System.Net.Http.Json;
using System.Threading.Tasks;
using NJsonSchema;
using NJsonSchema.CodeGeneration.CSharp;
using Namotion.Reflection;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Drawing;

using System.Xml.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json.Nodes;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WhatIfSportsTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlayerController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public PlayerController(ILogger<PlayerController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        //uncomment populating Player.cs forgot that the class gets created before you can run this
        [HttpGet]
        [Route("[controller]/players")]
        public IEnumerable<Player> GetPlayers()
        {
            List<Player> players = new List<Player>();

            using (SqlConnection conn = new SqlConnection(ConfigurationExtensions.GetConnectionString(_configuration, "DevContext")))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("GetPlayerList", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter sda = new SqlDataAdapter();
                sda.SelectCommand = cmd;
                DataSet ds = new DataSet();
                sda.Fill(ds);
                DataTable dt = ds.Tables[0];
                sda.Dispose();

                foreach (DataRow dr in dt.Rows)
                {
                    Player player = new Player
                    {
                        //id = (int)dr["id"],
                        //firstname = dr["first_name"].ToString(),
                        //lastname = dr["last_name"].ToString(),
                        //position = dr["position"].ToString(),
                        //age = dr["age"].ToString(),
                        //stub = dr["stub"].ToString()
                    };
                    players.Add(player);
                }
            }
            return players.ToArray();
        }

        //Get Single Player, not complete
        
        //[Route("[controller]/player/id")]
        //[HttpGet("{id}", Name = "GetCashMovement")]
        //public IActionResult Get(int id)
        //{
        //    List<Player> players = new List<Player>();

        //    using (SqlConnection conn = new SqlConnection(ConfigurationExtensions.GetConnectionString(_configuration, "DevContext")))
        //    {
        //        conn.Open();

        //        SqlCommand cmd = new SqlCommand("GetPlayerList", conn);
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.Parameters.AddWithValue("@id", id);
        //        SqlDataAdapter sda = new SqlDataAdapter();
        //        sda.SelectCommand = cmd;
        //        DataSet ds = new DataSet();
        //        sda.Fill(ds);
        //        DataTable dt = ds.Tables[0];
        //        sda.Dispose();

        //        foreach (DataRow dr in dt.Rows)
        //        {
        //            Player player = new Player
        //            {
        //                id = (int)dr["id"],
        //                firstname = dr["first_name"].ToString(),
        //                lastname = dr["last_name"].ToString(),
        //                position = dr["position"].ToString(),
        //                age = dr["age"].ToString(),
        //                stub = dr["stub"].ToString()
        //            };
        //            players.Add(player);
        //        }
        //    }
        //    return players.ToArray();
        //}


        //call external api
        [HttpGet]
        [Route("[controller]/GetPlayersFromProvider")]
        public async Task<System.String> GetPlayersFromProvider()       
        {         
            string responseBody = string.Empty;

            try
            {
                //Calling external api and saving to local file root of C:\
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync("https://api.cbssports.com/fantasy/players/list?version=3.0&SPORT=basketball&response_format=JSON");
                response.EnsureSuccessStatusCode();
                responseBody = await response.Content.ReadAsStringAsync();

                var root = JToken.Parse(responseBody);
                var players = root.SelectTokens("..players[*]").ToList();

                string path = @"c:\results.JSON";
                           
                using (StreamWriter sw = System.IO.File.CreateText(path))
                {
                    sw.WriteLine("[");                        
                    var curRowCount = 0;
                    foreach (var player in players)
                    {
                        curRowCount++;

                        if (curRowCount == players.Count)
                        {
                            sw.WriteLine(player);
                        }
                        else
                        {
                            sw.WriteLine(player + ",");
                        }
                    }
                    sw.WriteLine("]");
                }

                //calling proc that reads the file and uses OpenJSON to create a table with the correct schema
                //no need to call an api or db 1058x to insert data:)
                string? connString = string.Empty;
                connString = ConfigurationExtensions.GetConnectionString(_configuration, "DevContext");
                if (connString != null)
                {
                    SqlConnection conn = new SqlConnection(connString);
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("InsertPlayerJSON", conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    int rowAffected = cmd.ExecuteNonQuery();
                    conn.Close();
                }

                //calling proc to create a POCO from the schema in the new table
                if (connString != null)
                {
                    string POCOString = "";

                    using (SqlConnection conn = new SqlConnection(ConfigurationExtensions.GetConnectionString(_configuration, "DevContext")))
                    {
                        conn.Open();
                                               
                        conn.FireInfoMessageEventOnUserErrors = false;
                        conn.InfoMessage += delegate (object sender, SqlInfoMessageEventArgs e)
                        {
                            POCOString += "\n" + e.Message;
                        };

                        SqlCommand cmd = new SqlCommand("CreatePlayerPOCO", conn);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.ExecuteReader();
                    }

                    string fileName = "Player.cs";
                    string path2 = Path.Combine(Environment.CurrentDirectory, @"Models\", fileName);

                    System.IO.File.WriteAllText(path2, POCOString);                  
                }

                return responseBody;
            }
            catch (Exception ex)
            {
                //Logger not fully implemented
                //_logger.LogError(ex.Message.ToString(), new object());
                responseBody = "Failure attempting to retrieve Player information, ex.message : " + ex.Message;
                return responseBody;
               
            }
        }
    }
}
