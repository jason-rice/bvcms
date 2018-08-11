﻿namespace CmsWeb.Areas.Coordinator.Models
{
    public class CheckinActionDto
    {
        public const string IncrementCapacity = "IncrementCapacity";
        public const string DecrementCapacity = "DecrementCapacity";
        public const string ToggleCheckinOpen = "ToggleCheckinOpen";

        public string SelectedTimeslot { get; set; }
        public int OrganizationId { get; set; }
        public int SubgroupId { get; set; }
        public string SubgroupName { get; set; }
        public string Action { get; set; }
    }
}
