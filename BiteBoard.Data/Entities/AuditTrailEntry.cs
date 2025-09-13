using Microsoft.EntityFrameworkCore.ChangeTracking;
using BiteBoard.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace BiteBoard.Data.Entities
{
    public class AuditTrailEntry
	{
		public EntityEntry Entry { get; }
		public Guid UserId { get; set; }
		public string TableName { get; set; }
		public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();
		public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
		public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
		public List<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();
		public AuditTrailType AuditType { get; set; }
		public List<string> ChangedColumns { get; } = new List<string>();
		public bool HasTemporaryProperties => TemporaryProperties.Any();

		public AuditTrailEntry(EntityEntry entry)
		{
			Entry = entry;
		}

		public AuditTrail ToAuditTrail()
		{
			AuditTrail auditTrail = new()
			{
				UserId = UserId,
				Type = AuditType.ToString(),
				TableName = TableName,
				DateTime = DateTime.UtcNow,
				PrimaryKey = JsonSerializer.Serialize(KeyValues),
				OldValues = OldValues.Count == 0 ? null : JsonSerializer.Serialize(OldValues),
				NewValues = NewValues.Count == 0 ? null : JsonSerializer.Serialize(NewValues),
				AffectedColumns = ChangedColumns.Count == 0 ? null : JsonSerializer.Serialize(ChangedColumns),
			};
			return auditTrail;
		}
	}
}