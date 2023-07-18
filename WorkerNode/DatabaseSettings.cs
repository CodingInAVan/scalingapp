namespace WorkerNode
{
	public record DatabaseSettings
    {
		public required string ConnectionString { get; init; }
	}
}

