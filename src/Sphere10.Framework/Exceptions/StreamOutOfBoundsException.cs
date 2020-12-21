namespace Sphere10.Framework {

	public class StreamOutOfBoundsException : SoftwareException {
		public StreamOutOfBoundsException()
			: this("Stream was accessed beyond the permissible boundary") {
		}
		public StreamOutOfBoundsException(string message) : base(message) {
		}
	}

}