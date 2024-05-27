namespace BuilderService;

internal sealed class SyncfusionLicenseRegistration(IConfiguration configuration)
{
    public void RegisterLicense(string licenseKeyName)
    {
        var value = configuration.GetValue<string>(licenseKeyName);
        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(value);
    }
}