# Troubleshooting Guide

## Build and Restore Issues

### MAUI workload missing
- **Symptom**: Build fails with MAUI SDK/targets not found.
- **Fix**:
  1. Open Visual Studio Installer.
  2. Ensure the .NET MAUI workload is installed.
  3. Restart Visual Studio and rebuild.

### NuGet restore failures
- **Symptom**: Package restore errors or missing assemblies.
- **Fix**:
  1. Run `dotnet restore` from the solution root.
  2. Clear local NuGet cache if needed: `dotnet nuget locals all --clear`.
  3. Restore again.

## Runtime Configuration Issues

### Azure connection test fails
- **Symptom**: Settings page test connection returns an error.
- **Fix**:
  1. Verify endpoint format: `https://<resource>.cognitiveservices.azure.com/`.
  2. Verify API key is valid and active.
  3. Verify region value matches the Azure resource.
  4. Save settings and retry.

### Document processing does not progress
- **Symptom**: Upload succeeds but pipeline does not complete.
- **Fix**:
  1. Confirm a supported image format (`JPEG`, `JPG`, `PNG`).
  2. Confirm size and dimension constraints in `IImageService` validation.
  3. Check app logs for stage-level errors.

## Platform Issues

### Camera selection unavailable
- **Symptom**: Capture option unavailable or returns permission error.
- **Fix**:
  1. Confirm device/emulator supports camera capture.
  2. Grant camera permission when prompted.
  3. Use photo library fallback if capture is unsupported.

### UI tests are skipped
- **Symptom**: UI tests do not run locally.
- **Fix**:
  1. Install Appium 2 and required driver.
  2. Start Appium server.
  3. Set UI test environment variables documented in `README.md`.
  4. Run tests with `Category=UI` filter.

## Diagnostics Checklist

Before opening an issue, collect:
- Target platform and OS version
- Build configuration (`Debug`/`Release`)
- Repro steps
- Relevant logs or stack trace
- Screenshot of Settings values (without exposing secrets)
