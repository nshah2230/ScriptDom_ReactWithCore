using Microsoft.AspNetCore.Mvc;

namespace ScriptDom.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DataExtractController : ControllerBase
    {
        private readonly ILogger<DataExtractController> _logger;
        private readonly IConfiguration _configuration;
        public DataExtractController(ILogger<DataExtractController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet(Name = "GetDataExtract")]
        public IEnumerable<object>  Get()
        {
            string dirPath = _configuration["OtherSettings:dirPath"].ToString();

            // List<DataExtract> dataExtracts = new List<DataExtract>();
            //Dictionary<string, List<DataExtract>> dic = new Dictionary<string, List<DataExtract>>();
            List<object> dic = new List<object>();
            foreach (var file in Directory.GetFiles(dirPath, "*.sql"))
            {
                string sqlText = System.IO.File.ReadAllText(file);
                _logger.LogInformation(file);

                var parser = new Microsoft.SqlServer.TransactSql.ScriptDom.TSql170Parser(false);
                using (var reader = new StringReader(sqlText))
                {
                    var fragment = parser.ParseStatementList(reader, out var errors);
                    if (errors.Count == 0)
                    {
                        SqlVisitor visitor = new SqlVisitor();
                        foreach (var statement in fragment.Statements)
                        {
                            statement.Accept(visitor);
                            // dic.Add(statement.GetType().Name, visitor.sqlDataExtracts.ToList());
                            dic.Add(new JsonResult(new { Key = statement.GetType().Name, Value = visitor.sqlDataExtracts.ToList() }));
                        }
                        //dataExtracts.AddRange(visitor.sqlDataExtracts);
                    }
                    else
                    {
                        _logger.LogError($"Errors in file {file}: {string.Join(", ", errors.Select(e => e.Message))}");
                    }
                }
            }

           return dic;
        }
    }
}
