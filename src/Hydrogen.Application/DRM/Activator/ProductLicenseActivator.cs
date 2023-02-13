using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Hydrogen.Application;

public class ProductLicenseActivator : IProductLicenseActivator {

	public ProductLicenseActivator(IProductLicenseStorage productLicenseStorage, IProductLicenseClient productLicenseClient, IProductLicenseEnforcer productLicenseEnforcer, IProductInformationProvider productInformationProvider) {
		Storage = productLicenseStorage;
		ProductLicenseClient = productLicenseClient;
		ProductLicenseEnforcer = productLicenseEnforcer;
		ProductInformationProvider = productInformationProvider;
	}

	protected IProductLicenseStorage Storage { get; }

	protected IProductLicenseClient ProductLicenseClient { get; }
	protected IProductLicenseEnforcer ProductLicenseEnforcer { get; }
	protected IProductInformationProvider ProductInformationProvider { get; }

	public async Task ActivateLicense(string productKey) {
		var licenseResult = await ProductLicenseClient.ActivateLicenseAsync(ProductInformationProvider.ProductInformation.ProductCode, productKey, Environment.MachineName, Tools.Network.GetMacAddresses().ToArray());
		if (licenseResult.Failure)
			throw new InvalidOperationException(licenseResult.ErrorMessages.ToParagraphCase());
		await ApplyLicense(licenseResult.Value);
	}

	public async Task ApplyLicense(ProductLicenseActivationDTO licenseActivation) {
		Guard.ArgumentNotNull(licenseActivation, nameof(licenseActivation));
		var authority = Storage.TryGetDefaultLicense(out var trialActivation) ? trialActivation.Authority : licenseActivation.Authority;
		if (!ProductLicenseEnforcer.ValidateLicense(licenseActivation, authority)) 
			throw new ProductLicenseTamperedException();
	
		// All good, store this license
		Storage.SaveActivatedLicense(licenseActivation);
		ProductLicenseEnforcer.EnforceLicense(true);
	}

}
