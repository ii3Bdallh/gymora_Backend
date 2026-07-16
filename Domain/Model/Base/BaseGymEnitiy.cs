using Domain.Model.Base;

namespace Domain.Model.Base
{
    public abstract class BaseGymEntity : BaseEntity
    {
        public int GymId { get; set; }
    }
}