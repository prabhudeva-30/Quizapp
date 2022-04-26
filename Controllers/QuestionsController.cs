#nullable disable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quizapp.Constants;
using Quizapp.Models;
using Quizapp.Services;

using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Quizapp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenservice;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public QuestionsController(DataContext context, ITokenService tokenservice, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _tokenservice = tokenservice;
            _httpContextAccessor = httpContextAccessor;
        }


        [HttpGet(nameof(GetQuestions)), Authorize]
        public async Task<ActionResult<IEnumerable<QuestionsDTO>>> GetQuestions()
        {
            GeneralResponse response = new GeneralResponse();
            List<Question> questions = new List<Question>();
            List<QuestionsDTO> responseQuestions = new List<QuestionsDTO>();

            questions = await _context.Questions.ToListAsync();
            if (questions.Count > 0)
            {
                foreach (Question data in questions)
                {
                    List<OptionsDTO> options = new List<OptionsDTO>();

                    options.Add(new OptionsDTO { AnswerText = data.Option_A, IsCorrect = data.Answer.ToUpper() == "A" });
                    options.Add(new OptionsDTO { AnswerText = data.Option_B, IsCorrect = data.Answer.ToUpper() == "B" });

                    if (data.Option_C != null)
                        options.Add(new OptionsDTO { AnswerText = data.Option_C, IsCorrect = data.Answer.ToUpper() == "C" });
                    if (data.Option_D != null)
                        options.Add(new OptionsDTO { AnswerText = data.Option_D, IsCorrect = data.Answer.ToUpper() == "D" });

                    responseQuestions.Add(new QuestionsDTO
                    {
                        QuestionText = data.QuestionText,
                        QuestionId = data.QuestionId,
                        AnswerOptions = options
                    }
                    );
                }

                response.Data = responseQuestions;
            }
            else
            {
                response.Status = MessageConstants.FAILED;
                response.Message = MessageConstants.DATA_NOT_AVAILABLE;
            }

            return Ok(response);
        }


        [HttpPost(nameof(SaveQuizAnswers)), Authorize(Roles = RolesConstants.CANDIDATE)]
        public ActionResult SaveQuizAnswers(QuizDetails quizDetails)
        {
            GeneralResponse response = new GeneralResponse();
            List<Question> questions = new List<Question>();
            int CorrectAnswersCount = 0;
            int CandidateId = _tokenservice.GetUserId();

            if (quizDetails == null || CandidateId == 0 || quizDetails.Answers.Count == 0)
            {
                response.Status = MessageConstants.FAILED;
                response.Message = MessageConstants.INVALID_INPUT_DATA;
                return Ok(response);
            }

            questions = _context.Questions.Where(qn => quizDetails.Answers.Select(a => a.QuestionId).Contains(qn.QuestionId)).ToList();

            if (questions.Count > 0)
            {
                for (int i = 0; i < questions.Count; i++)
                {
                    if (quizDetails.Answers.Any(a => a.QuestionId == questions[i].QuestionId && a.ChoosenOption == questions[i].Answer))
                    {
                        CorrectAnswersCount++;
                    }
                }
            }
            else
            {
                response.Status = MessageConstants.FAILED;
                response.Message = MessageConstants.INVALID_INPUT_DATA;
                return Ok(response);
            }

            foreach (var data in quizDetails.Answers)
            {
                QuizAnswer tempQuizAnswer = new QuizAnswer { ChoosenAnswer = data.ChoosenOption, QuestionId = data.QuestionId, UserId = CandidateId };
                _context.QuizAnswers.Add(tempQuizAnswer);
            }
            _context.SaveChanges();

            _context.QuizResults.Add(new QuizResult { UserId = CandidateId, Score = CorrectAnswersCount });
            _context.SaveChanges();


            response.Message = MessageConstants.ANSWERS_SAVED_SUCCESSFULLY;
            return Ok(response);
        }


        [HttpPost(nameof(SaveQuizScore)), Authorize(Roles = RolesConstants.CANDIDATE)]
        public ActionResult SaveQuizScore(int Score)
        {
            GeneralResponse response = new GeneralResponse();
            int CandidateId = _tokenservice.GetUserId();
            //int Score = Convert.ToInt32(data);

            if (Score == 0)
            {
                response.Status = MessageConstants.FAILED;
                response.Message = MessageConstants.INVALID_INPUT_DATA;
                return Ok(response);
            }
            if (CandidateId == 0)
            {
                response.Status = MessageConstants.FAILED;
                response.Message = MessageConstants.INVALID_USER_ID;
                return Ok(response);
            }

            _context.QuizResults.Add(new QuizResult { UserId = CandidateId, Score = Score });
            _context.SaveChanges();

            response.Message = MessageConstants.SCORE_SAVED_SUCCESSFULLY;
            return Ok(response);
        }


        //[HttpPost]
        //[HttpPost(nameof(UploadFile)), Authorize(Roles = RolesConstants.CANDIDATE)]
        //public async Task UploadFile()
        //{


        //    if (!Request.Content.IsMimeMultipartContent())
        //    {
        //        throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
        //    }

        //    var provider = new MultipartMemoryStreamProvider();

        //    await Request.Content.ReadAsMultipartAsync(provider);

        //    var file = provider.Contents.FirstOrDefault();

        //    var fileExtension = file.Headers.ContentDisposition.FileName.Split('.')[1];
        //    if (!allowedFileExtensions.Any(a => a.Equals(fileExtension)))
        //    {
        //        return BadRequest($"File with extension {fileExtension} is not allowed");
        //    }


        //    await SaveFileToDatabase(file);

        //    return Ok();
        //}



        private async Task SaveFileToDatabase(HttpContent file)
        {
            var byteArray = await file.ReadAsByteArrayAsync();
            var base64String = Convert.ToBase64String(byteArray); // Save this base64 string to database
        }


        [RequestFormLimits(ValueLengthLimit = int.MaxValue, MultipartBodyLengthLimit = int.MaxValue)]
        [DisableRequestSizeLimit]
        [Consumes("multipart/form-data")]
        [HttpPost(nameof(Upload))]
        public IActionResult Upload(IFormFile FilePath)
        {

            if (!ModelState.IsValid)
            {
                bool a = ModelState.IsValid;
            }

            string[] FileDetails = FilePath.FileName.Split(".");

            string UploadedFileName = FilePath.FileName;
            string PDFFileName = FileDetails[0];
            string UploadedFileType = FileDetails[FileDetails.Length - 1];

            string UploadedFileSavePath = System.IO.Path.Combine(Environment.CurrentDirectory, "Docs", UploadedFileName);
            string PDFFileSavePath = System.IO.Path.Combine(Environment.CurrentDirectory, "Docs", PDFFileName.Trim() + UploadedFileType);

            using (var filestream = new FileStream(UploadedFileSavePath, FileMode.Create, FileAccess.Write))
            {
                FilePath.CopyTo(filestream);
            }
        
            return Ok();
        }


    }
}
