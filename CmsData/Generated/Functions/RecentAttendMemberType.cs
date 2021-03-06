using System;
using System.ComponentModel;
using System.Data.Linq.Mapping;

namespace CmsData.View
{
    [Table(Name = "RecentAttendMemberType")]
    public partial class RecentAttendMemberType
    {
        private static PropertyChangingEventArgs emptyChangingEventArgs => new PropertyChangingEventArgs("");

        private int? _PeopleId;

        public RecentAttendMemberType()
        {
        }

        [Column(Name = "PeopleId", Storage = "_PeopleId", DbType = "int")]
        public int? PeopleId
        {
            get => _PeopleId;

            set
            {
                if (_PeopleId != value)
                {
                    _PeopleId = value;
                }
            }
        }
    }
}
