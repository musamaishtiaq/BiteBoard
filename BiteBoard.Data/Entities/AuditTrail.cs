using Finbuckle.MultiTenant;
using SequentialGuid;
using System;

namespace BiteBoard.Data.Entities
{
    [MultiTenant]
    public class AuditTrail
	{
		public Guid Id { get; set; } = SequentialGuidGenerator.Instance.NewGuid();
		public Guid UserId { get; set; }
		public string Type { get; set; }
		public string TableName { get; set; }
		public DateTime DateTime { get; set; }
		public string OldValues { get; set; }
		public string NewValues { get; set; }
		public string AffectedColumns { get; set; }
		public string PrimaryKey { get; set; }
		public string Description { get; set; }
	}
}