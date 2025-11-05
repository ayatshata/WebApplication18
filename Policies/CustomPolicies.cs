using Microsoft.AspNetCore.Authorization;

namespace MughtaribatHouse.Policies
{
    public static class CustomPolicies
    {
        public static AuthorizationPolicy RequireAdminRole()
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole("Admin")
                .Build();
        }

        public static AuthorizationPolicy RequireManagerRole()
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole("Admin", "Manager")
                .Build();
        }

        public static AuthorizationPolicy RequireStaffRole()
        {
            return new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireRole("Admin", "Manager", "Staff")
                .Build();
        }
    }
}