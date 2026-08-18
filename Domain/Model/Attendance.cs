using Domain.Attributes;
using Domain.Model.Base;
using System;

namespace Domain.Model
{
    public class Attendance : BaseGymEntity
    {
        [Filterable(FilterType.Exact)]
        public int MemberId { get; set; }
        public GymPerson Member { get; set; } = null!;

        [Filterable(FilterType.Between)]
        public DateTime CheckInTime { get; set; } = DateTime.UtcNow;

        [Filterable(FilterType.Exact)]
        public int? RecordedById { get; set; }
        public GymPerson? RecordedBy { get; set; }


    }
}
