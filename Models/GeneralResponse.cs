using Quizapp.Constants;

namespace Quizapp.Models
{
    public class GeneralResponse
    {
        public string Status { get; set; } = MessageConstants.SUCCESS;
        public string Message { get; set; } = MessageConstants.SUCCESS;
        public object? Data { get; set; }
    }
}
