namespace Domain.Enum
{
    public enum GymRole
    {
        Manager = 1,
        Coach = 2,
        Receptionist = 3,
        Cleaner = 4,
        Other = 5
    }

    public static class GymRoleExtensions
    {
        public static string ToRoleString(this GymRole role)
        {
            return role switch
            {
                GymRole.Manager => "Manager",
                GymRole.Coach => "Coach",
                GymRole.Receptionist => "Receptionist",
                GymRole.Cleaner => "Cleaner",
                GymRole.Other => "Other",
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
            };
        }
    }


}


