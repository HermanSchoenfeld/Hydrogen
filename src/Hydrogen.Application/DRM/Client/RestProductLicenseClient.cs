using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Hydrogen.Application;

public class RestProductLicenseClient : RestClient, IProductLicenseClient {

	public RestProductLicenseClient(string baseUrl, JsonSerializerSettings defaultSerializerSettings = null, ILogger logger = null)
		: base(baseUrl, defaultSerializerSettings, logger) {
	}

	public async Task<Result<ProductLicenseDTO>> GetLicenseAsync(string productKey) 
		=> await base.GetAsync<ProductLicenseDTO>("get_license", new Dictionary<string, string> { ["productKey"] = productKey });

	public async Task<Result<ProductLicenseActivationDTO>> ActivateLicenseAsync(Guid productCode, string productKey, string machineName, string[] macAddresses) 
		=> await base.GetAsync<Result<ProductLicenseActivationDTO>>(
			"activate_license", 
			new Dictionary<string, string> { 
				["productCode"] = productCode.ToStrictAlphaString(),
				["productKey"] = productKey,
				["machineName"] = machineName,
				["macAddresses"] = macAddresses.ToDelimittedString(","),			}
		);
	

	public async Task<Result<SignedItem<ProductLicenseCommandDTO>>> NotifyLicenseUsage(Guid productCode, string productKey, string machineName, string[] macAddresses) 
		=> await base.GetAsync<Result<SignedItem<ProductLicenseCommandDTO>>>(
			"notify_license_use", 
			new Dictionary<string, string> { 
				["productCode"] = productCode.ToStrictAlphaString(),
				["productKey"] = productKey,
				["machineName"] = machineName,
				["macAddresses"] = macAddresses.ToDelimittedString(","),
			}
		);
}