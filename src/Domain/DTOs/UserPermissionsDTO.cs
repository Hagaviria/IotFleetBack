using System.Collections.Generic;

namespace Domain.DTOs
{
    public class UserPermissionsDTO
    {
        public int ProfileId { get; set; }
        public List<ScreenPermission> Permissions { get; set; } = [];
    }
    public class Permission
    {
        public bool View { get; set; } = false;
        public bool Add { get; set; } = false;
        public bool Edit { get; set; } = false;
        public bool Delete { get; set; } = false;
        public bool Special { get; set; } = false;
    }

    public class ScreenPermission
    {
        public int ScreenId { get; set; }
        public string Screen { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public Permission Permissions { get; set; } = new Permission();
    }


}
