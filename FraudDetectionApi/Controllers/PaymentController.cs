using FraudDetectionApi.DTOs;
using FraudDetectionApi.Interface;
using FraudDetectionEngine.Services;
using Microsoft.AspNetCore.Mvc;

namespace FraudDetectionApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _payment;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public PaymentController(IPaymentService payment, IConfiguration configuration)
        {
            _payment = payment;
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost("create")]
        public IActionResult Pay([FromBody] PaymentRequest request)
        {
            var verifySub = _payment.VerifySubscribe(request.ClientId, request.ClientSecret);
            if (verifySub != null)
            {
                // create a new instanse from FraudDecisionService
                var fraudService = new FraudDecisionService(_connectionString);

                Guid transactionId = Guid.NewGuid();

                // check transaction is fraud or not using a custom model
                var result = fraudService.Decide(new FraudDetectionEngine.Models.TransactionTraningData()
                {
                    CardNumber = request.CardNumber,
                    Amount = request.Amount,
                    TransactionType = request.TransactionType,
                    Device = request.Device,
                    IPAddress = request.IPAddress,
                    Location = request.Location,
                    Source = request.Source,
                    CreatedOn = DateTime.Now
                }, transactionId.ToString());

               
                if (result.Action == "Block")  // the transaction high risk score
                {
                    return BadRequest(new { Message = "Sorry, your transaction has been blocked because it's a fraudulent transaction", Status = false });
                }
                else if (result.Action == "Challenge") // the transaction medium risk score and need otp to verfied
                {
                    string otpCode = OTPService.Generate(request.CardNumber, transactionId, _connectionString);

                    return BadRequest(new { Message = "Sorry, your transaction needs to be confirmed using the Otp Code", OtpCode = otpCode, TransactionId = transactionId.ToString(), Status = false });
                }
                else // the transaction low risk score. can be added
                {
                    request.SubscribeId = verifySub.Id;
                    request.CreatedOn = DateTime.Now;

                    var payment = _payment.Add(request);
                    return Ok(new { Message = "Payment transaction added successfully", Status = true });
                }
            }

            return BadRequest(new { Message = "Your subscription expired or doesn't exist", Status = false });
        }      
        
        [HttpPost("verify-otp")]
        public IActionResult VerifyOtp([FromBody] VerifyOtp request)
        {
            var otpCode = OTPService.Verify(request.CardNumber, request.TransactionId, request.Code, _connectionString);

            if (otpCode)
            {
                return BadRequest(new { Message = "The payment tranaction was confirm successfully", Status = true });
            }

            return BadRequest(new { Message = "Sorry, Your Code doesn't exist or expired", Status = false });
        }

    }
}
