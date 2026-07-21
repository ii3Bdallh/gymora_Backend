using Domain.Interface;
using Domain.Model.Base;

namespace Domain.Model.Base
{
    public abstract class BaseGymEntity : BaseEntity, IBaseGymEntity
    {
        public int GymId { get; set; }
    }
}