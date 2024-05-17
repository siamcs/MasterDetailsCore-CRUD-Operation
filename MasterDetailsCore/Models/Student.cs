namespace MasterDetailsCore.Models
{
    public class Student
    {
        public int StudentId { get; set; }

        public string StudentName { get; set; } = null!;

        public DateTime Dob { get; set; }

        public string MobileNo { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsEnrolled { get; set; }

        public int CourseId { get; set; }

        public virtual Course Course { get; set; }

        public virtual ICollection<Module> Modules { get; set; } = new List<Module>();
    }
}