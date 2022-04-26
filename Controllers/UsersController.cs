#nullable disable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizapp.Common;
using Quizapp.Constants;
using Quizapp.Models;
using Quizapp.Services;

namespace Quizapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenservice;

        public UsersController(DataContext context, ITokenService tokenservice)
        {
            _context = context;
            _tokenservice = tokenservice;
        }


        [HttpGet(nameof(GetAllCandidates)), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<ActionResult<IEnumerable<UserFullDetails>>> GetAllCandidates()
        {
            GeneralResponse response = new GeneralResponse();
            List<UserDetailsDTO> userFullDetails = new List<UserDetailsDTO>();

            userFullDetails = await _context.Users
                .Where(x => x.Role == RolesConstants.CANDIDATE)
                .Select(a => new UserDetailsDTO
                {
                    Name = a.Name,
                    Email = a.Email,
                    UserId = a.UserId                  
                }).ToListAsync();

            if (userFullDetails != null)
            {
                response.Data = userFullDetails;
            }
            else
            {
                response.Status = MessageConstants.FAILED;
                response.Message = MessageConstants.DATA_NOT_AVAILABLE;
            }
            return Ok(response);
        }

        [HttpGet(nameof(GetCompletedCandidateDetails)), Authorize(Roles = RolesConstants.ADMIN)]
        public ActionResult<IEnumerable<UserFullDetails>> GetCompletedCandidateDetails()
        {
            GeneralResponse response = new GeneralResponse();
            List<UserFullDetails> userFullDetails = new List<UserFullDetails>();

            var testmodel = from a in _context.Users
                            from b in _context.QuizResults
                            where a.Role == RolesConstants.CANDIDATE && a.UserId == b.UserId
                            select new UserFullDetails
                            {
                                Name = a.Name,
                                Email = a.Email,
                                UserId = a.UserId,
                                Score = b.Score,
                                AttemptedDate = b.AttemptedDate
                            };

            //  List<UserFullDetails> listofusers = new List<UserFullDetails>();
            //_context.Users.Where(z => z.Role == RolesConstants.CANDIDATE).ToList().ForEach(a =>
            //  {
            //     var s = _context.QuizResults.ToList().Find(ab => ab.UserId == a.UserId);
            //      if (s != null)
            //      {
            //          listofusers.Add(
            //           new UserFullDetails
            //          {
            //              Name = a.Name,
            //              Email = a.Email,
            //              UserId = a.UserId,
            //              Score = s.Score,
            //              AttemptedDate = s.AttemptedDate
            //          });
            //      }
            //  });


            userFullDetails = testmodel.Select(a => a).ToList();

            if (userFullDetails != null)
            {
                response.Data = userFullDetails;
            }
            else
            {
                response.Status = MessageConstants.FAILED;
                response.Message = MessageConstants.DATA_NOT_AVAILABLE;
            }
            return Ok(response);
        }

        [HttpPost(nameof(AddCandidate)), Authorize(Roles = RolesConstants.ADMIN)]
        public async Task<ActionResult> AddCandidate(AddUserDTO user)
        {
            GeneralResponse response = new GeneralResponse();
            string PasswordHash;
            string PasswordSalt;
            int UserId = _tokenservice.GetUserId();

            #region Validations
            if (user?.CandidateName == null || user?.CandidateName == "")
            {
                response.Status = MessageConstants.FAILED;
                response.Message = MessageConstants.INVALID_NAME;
                return BadRequest(response);
            }
            if (user?.Email == null || user?.Email == "")
            {
                response.Status = MessageConstants.FAILED;
                response.Message = MessageConstants.INVALID_EMAIL;
                return BadRequest(response);
            }
            if (user?.Password == null || user?.Password == "")
            {
                response.Status = MessageConstants.FAILED;
                response.Message = MessageConstants.INVALID_PASSWORD;
                return BadRequest(response);
            }
            if (UserId == 0)
            {
                response.Status = MessageConstants.FAILED;
                response.Message = MessageConstants.INVALID_USER_ID;
                return BadRequest(response);
            }
            bool IsEmailAvailable = this.IsEmailAvailable(user.Email);
            if (IsEmailAvailable)
            {
                response.Status = MessageConstants.FAILED;
                response.Message = MessageConstants.EMAIL_EXSISTS;
                return BadRequest(response);
            }
            #endregion Validations

            PasswordManager.CreatePassword(user.Password, out PasswordSalt, out PasswordHash);

            User userDetails = new User
            {
                Name = user.CandidateName,
                Email = user.Email,
                Role = RolesConstants.CANDIDATE,
                Password = PasswordHash,
                PasswordSalt = PasswordSalt,
                CreatedBy = UserId
            };

            _context.Users.Add(userDetails);
            await _context.SaveChangesAsync();

            response.Message = String.Format(MessageConstants.USER_CREATION_SUCCESSFULLY, user.CandidateName);
            return Ok(response);
        }

        [HttpGet(nameof(CheckEmailAvailability)), Authorize(Roles = RolesConstants.ADMIN)]
        public ActionResult CheckEmailAvailability(string Email)
        {
            GeneralResponse response = new GeneralResponse();

            bool IsAvailable = IsEmailAvailable(Email);

            response.Status = !IsAvailable ? MessageConstants.SUCCESS : MessageConstants.FAILED;
            response.Message = !IsAvailable ? MessageConstants.AVAILABLE : MessageConstants.NOT_AVAILABLE;

            return Ok(response);
        }

        private bool IsEmailAvailable(string Email)
        {
            return _context.Users.Any(o => o.Email == Email);
        }

        [HttpGet(nameof(CheckQuizAttemptStatus)), Authorize(Roles = RolesConstants.CANDIDATE)]
        public ActionResult CheckQuizAttemptStatus()
        {
            GeneralResponse response = new GeneralResponse();
            int id = _tokenservice.GetUserId();
            bool IsAvailable = _context.QuizResults.Any(o => o.UserId == id);

            response.Status = !IsAvailable ? MessageConstants.SUCCESS : MessageConstants.FAILED;
            response.Message = !IsAvailable ? MessageConstants.CANDIDATE_NOT_ATTEMPTED_THE_QUIZ : MessageConstants.CANDIDATE_ATTEMPTED_THE_QUIZ;

            return Ok(response);
        }

    }
}
