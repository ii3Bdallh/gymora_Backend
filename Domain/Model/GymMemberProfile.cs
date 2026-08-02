
namespace Domain.Model
{
    public class GymMemberProfile
    {
        public int Id { get; set; }
        public string? MedicalNotes { get; set; }
        public string? Notes { get; set; }
        public GymPerson GymPerson { get; set; } = default!;
    }
}
