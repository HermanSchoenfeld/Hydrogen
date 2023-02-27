using System;
using System.Text;

namespace Hydrogen.Application;

public class ProductLicenseAuthorityDTOSerializer : ItemSerializer<ProductLicenseAuthorityDTO> {

	private readonly StringSerializer _stringSerializer = new(Encoding.ASCII);
	private readonly ByteArraySerializer _byteArraySerializer = new();

	public override int CalculateSize(ProductLicenseAuthorityDTO item) 
		=> 
		sizeof(int) + _stringSerializer.CalculateSize(item.Name) + 
		1 + 
		sizeof(int) + item.LicensePublicKey.Length;

	public override bool TrySerialize(ProductLicenseAuthorityDTO item, EndianBinaryWriter writer, out int bytesWritten) {
		bytesWritten = 0;

		writer.Write(_stringSerializer.CalculateSize(item.Name));
		if (!_stringSerializer.TrySerialize(item.Name, writer, out var nameBytes))
			return false;
		bytesWritten += nameBytes;

		writer.Write((byte)item.LicenseDSS);
		bytesWritten += 1;

		writer.Write(item.LicensePublicKey.Length);
		if (!_byteArraySerializer.TrySerialize(item.LicensePublicKey, writer, out var pubKeyBytes))
			return false;
		bytesWritten += pubKeyBytes;

		return true;
	}

	public override bool TryDeserialize(int byteSize, EndianBinaryReader reader, out ProductLicenseAuthorityDTO item) {
		throw new NotImplementedException();
	}
}