using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizapp.Common;
using Quizapp.Constants;
using Quizapp.Exception;
using Quizapp.Models;
using Quizapp.Services;



namespace Quizapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenservice;

        public AuthController(DataContext context, IConfiguration configuration, ITokenService tokenservice)
        {
            _context = context;
            _configuration = configuration;
            _tokenservice = tokenservice;
        }

        [HttpPost(nameof(Login))]
        [AllowAnonymous]
        public ActionResult Login(LoginDto request)
        {
            bool isValid = false;
            User user = new User();
            LoginResponseDetails userDetails = new LoginResponseDetails();
            GeneralResponse response = new GeneralResponse();

            if (request.UserName == null || request.UserName == "")
            {
                //response.Status = MessageConstants.FAILED;
                //response.Message = MessageConstants.INVALID_USERNAME;
                //return BadRequest(response);
                throw new ValidationException(MessageConstants.INVALID_USERNAME);
            }
            if (request.Password == null || request.Password == "")
            {
                //response.Status = MessageConstants.FAILED;
                //response.Message = MessageConstants.INVALID_PASSWORD;
                //return BadRequest(response);
                throw new ValidationException(MessageConstants.INVALID_PASSWORD);
            }

            //  throw new Exception("hello from exception");

            user = _context.Users.Where(a => a.Email == request.UserName).FirstOrDefault();

            if (user != null)
            {
                isValid = PasswordManager.VerifyPassword(request.Password, user.Password, user.PasswordSalt);
                if (isValid)
                {
                    userDetails.Name = user.Name;
                    userDetails.Role = user.Role;
                    userDetails.Token = _tokenservice.CreateToken(user);
                }
                else
                {
                    //response.Status = MessageConstants.FAILED;
                    //response.Message = MessageConstants.INCORRECT_PASSWORD;
                    //return BadRequest(response);
                    throw new ValidationException(MessageConstants.INCORRECT_PASSWORD);
                }
            }
            else
            {
                //response.Status = MessageConstants.FAILED;
                //response.Message = String.Format(MessageConstants.USER_NOT_FOUND_FOR_GIVEN_USERNAME, request.UserName);
                //return NotFound(response);
                throw new DomainNotFoundException(String.Format(MessageConstants.USER_NOT_FOUND_FOR_GIVEN_USERNAME, request.UserName));
            }

            response.Data = userDetails;
            return Ok(response);
        }


        [HttpGet(nameof(testmethod))]
        [AllowAnonymous]
        public ActionResult testmethod(string name)
        {


            if (name == "prabhu")
                throw new DomainNotFoundException("hi prabhu");

            if (name == "deva")
                throw new System.Exception("Hi deva");

            return Ok(name);
        }

    }
}
