namespace Subsy.Domain.Entities
{
    public class AuditLog
    {
        public int Id { get; private set; }
        public string UserId { get; private set; } = default!;
        public string Action { get; private set; } = default!;
        public string EntityName { get; private set; } = default!;
        public int EntityId { get; private set; }
        public string? Details { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private AuditLog() { }

        public static AuditLog Create(
            string userId,
            string action,
            string entityName,
            int entityId,
            string? details,
            DateTime timestamp)
        {
            return new AuditLog
            {
                UserId = userId,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Details = details,
                CreatedAt = timestamp
            };
        }
    }
}
