using FraudDetectionEngine.Services;
using Microsoft.AspNetCore.Mvc;

namespace FraudDetectionApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FraudDetectionController : ControllerBase
    {
        [HttpGet("train-model")]
        public ActionResult TrainModelAsync()
        {
            var getData = ModelTrainerService.LoadDataFromDatabase("Data Source=.;Initial Catalog=Bulky;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True");
           
            ModelTrainerService.TrainModel(getData, "fraudDetectionModel.zip");
            return Ok(new { message = "train model was updated." });
        }
    }
}
