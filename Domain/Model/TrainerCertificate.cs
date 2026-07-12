using Domain.Model.Base;

namespace Domain.Model;

public class TrainerCertificate : BaseAuditableFileEntity
{
    public int TrainerId { get; set; }
    public string Title { get; set; } = string.Empty;
}
