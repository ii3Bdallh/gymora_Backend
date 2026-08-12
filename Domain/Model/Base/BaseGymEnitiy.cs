using Domain.Attributes;
using Domain.Interface;
using Domain.Model.Base;

namespace Domain.Model.Base
{
    public abstract class BaseGymEntity : BaseEntity, IBaseGymEntity
    {
    [Filterable(FilterType.Exact)]

        public int GymId { get; set; }

        public Gym Gym { get; set; } = default!;
    }
}






