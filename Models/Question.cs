using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Quizapp.Models
{
    public class Question
    {
        public int QuestionId { get; set; }

        [Required]
        [StringLength(100)]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Option_A { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Option_B { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Option_C { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Option_D { get; set; } = string.Empty;

        [StringLength(1)]
        public string Answer { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Required]
        public int CreatedBy { get; set; }

    }

    public class QuizAnswer
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Question")]
        public int QuestionId { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [StringLength(1)]
        public string ChoosenAnswer { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

    }

    public class QuestionsDTO
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public List<OptionsDTO> AnswerOptions { get; set; }
    }

    public class OptionsDTO
    {
        public string AnswerText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }

    public class QuizResult
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public int Score { get; set; }
        public DateTime AttemptedDate { get; set; } = DateTime.Now;
    }

    public class QuizDetails
    {
        public int CandidateId { get; set; }
        public List<QuestionDetails> Answers { get; set; }
    }
    public class QuestionDetails
    {
        public int QuestionId { get; set; }
        public string ChoosenOption { get; set; }
    }



}
