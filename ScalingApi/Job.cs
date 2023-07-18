namespace ScalingApi
{
	public record Job
	{
        public required Guid Id { get; init; }
        public required string CurrentWorker { get; init; }

    }
}

